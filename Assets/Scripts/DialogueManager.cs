using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using VIDE_Data;
using Invector.vCamera;
using System.Security.Cryptography.X509Certificates;
using Invector;
using Invector.vCharacterController;

public class DialogueManager : MonoBehaviour
{
    #region VARS
    [Header("Containers")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private GameObject NPC_Container;
    [SerializeField] private GameObject playerContainer;

    [Header("Text")]
    [SerializeField] private Text NPC_Text;
    [SerializeField] private Text NPC_label;
    [SerializeField] private Text playerLabel;

    [Header("Sprites and etc")]
    [SerializeField] private Image NPCSprite;
    [SerializeField] private Image playerSprite;
    [SerializeField] private GameObject playerChoicePrefab;
    [SerializeField] private GameObject _dialogueCamera;
    [SerializeField] private vThirdPersonInput _vThirdPersonInput;

    private bool dialoguePaused = false;
    private bool animatingText = false;

    [SerializeField] private StartDialogue player;

    private List<Text> currentChoices = new List<Text>();
    private IEnumerator NPC_TextAnimator;

    #endregion

    #region MAIN

    public void Interact(VIDE_Assign dialogue)
    {

        _dialogueCamera.SetActive(true);
        _vThirdPersonInput.enabled = false;
        var doNotInteract = PreConditions(dialogue);
        if (doNotInteract) return;

        if (!VD.isActive)
        {
            Begin(dialogue);
        }
        else
        {
            CallNext();
        }
    }

    private void Begin(VIDE_Assign dialogue)
    {
        NPC_Text.text = "";
        NPC_label.text = "";
        playerLabel.text = "";

        VD.OnActionNode += ActionHandler;
        VD.OnNodeChange += UpdateUI;
        VD.OnEnd += EndDialogue;

        VD.BeginDialogue(dialogue);

        dialogueContainer.SetActive(true);
    }

    public void CallNext()
    {
        if (animatingText) { CutTextAnim(); return; }

        if (!dialoguePaused)
        {
            VD.Next();
        }
    }

    private void Update()
    {
        var data = VD.nodeData;

        if (VD.isActive)
        {
            if (!data.pausedAction && data.isPlayer)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (data.commentIndex < currentChoices.Count - 1)
                        data.commentIndex++;
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    if (data.commentIndex > 0)
                        data.commentIndex--;
                }

                for (int i = 0; i < currentChoices.Count; i++)
                {
                    currentChoices[i].color = Color.white;
                    if (i == data.commentIndex) currentChoices[i].color = Color.yellow;
                }
            }
        }
    }
    private void UpdateUI(VD.NodeData data)
    {
        foreach (Text op in currentChoices)
        {
            Destroy(op.gameObject);
        }
        currentChoices = new List<UnityEngine.UI.Text>();
        NPC_Text.text = "";
        NPC_Container.SetActive(false);
        playerContainer.SetActive(false);
        playerSprite.sprite = null;
        NPCSprite.sprite = null;

        PostConditions(data);

        if (data.isPlayer)
        {
            if (data.sprite != null)
            {
                playerSprite.sprite = data.sprite;
            }
            else if (VD.assigned.defaultPlayerSprite != null)
            {
                playerSprite.sprite = VD.assigned.defaultPlayerSprite;
            }
            SetOptions(data.comments);

            if (data.tag.Length > 0)
            {
                playerLabel.text = data.tag;
            }
            else
            {
                playerLabel.text = player.playerName;
            }
            playerContainer.SetActive(true);
        }
        else  
        {
            if (data.sprite != null)
            {
                if (data.extraVars.ContainsKey("sprite"))
                {
                    if (data.commentIndex == (int)data.extraVars["sprite"])
                    {
                        NPCSprite.sprite = data.sprite;
                    }
                    else
                        NPCSprite.sprite = VD.assigned.defaultNPCSprite;
                }
                else
                {
                    NPCSprite.sprite = data.sprite;
                }
            }
            else if (VD.assigned.defaultNPCSprite != null)
            {
                NPCSprite.sprite = VD.assigned.defaultNPCSprite;
            }
            NPC_TextAnimator = DrawText(data.comments[data.commentIndex], 0.02f);
            StartCoroutine(NPC_TextAnimator);

            if (data.tag.Length > 0)
            {
                NPC_label.text = data.tag;
            }
            else
            {
                NPC_label.text = VD.assigned.alias;
            }
            NPC_Container.SetActive(true);
        }
    }

    public void SetOptions(string[] choices)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            GameObject newOp = Instantiate(playerChoicePrefab.gameObject, playerChoicePrefab.transform.position, Quaternion.identity) as GameObject;
            newOp.transform.SetParent(playerChoicePrefab.transform.parent, true);
            newOp.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20 - (20 * i));
            newOp.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            newOp.GetComponent<UnityEngine.UI.Text>().text = choices[i];
            newOp.SetActive(true);

            currentChoices.Add(newOp.GetComponent<UnityEngine.UI.Text>());
        }
    }

    private void EndDialogue(VD.NodeData data)
    {
        _vThirdPersonInput.enabled = true;
        _dialogueCamera.SetActive(false);
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        dialogueContainer.SetActive(false);
        VD.EndDialogue();

        //VD.SaveState("VIDEDEMOScene1", true);
        QuestChartDemo.SaveProgress();
    }

    private void OnDisable()
    {
        VD.OnActionNode -= ActionHandler;
        VD.OnNodeChange -= UpdateUI;
        VD.OnEnd -= EndDialogue;
        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);
        VD.EndDialogue();
    }

    #endregion

    #region DIALOGUE CONDITIONS 
    private bool PreConditions(VIDE_Assign dialogue)
    {
        var data = VD.nodeData;

        if (VD.isActive)
        {
            if (!data.isPlayer)
            {
                if (data.extraVars.ContainsKey("item") && !data.dirty)
                {
                    if (data.commentIndex == (int)data.extraVars["itemLine"])
                    {
                        if (data.extraVars.ContainsKey("item++"))
                        {
                            Dictionary<string, object> newVars = data.extraVars;
                            int newItem = (int)newVars["item"];
                            newItem += (int)data.extraVars["item++"]; 
                            newVars["item"] = newItem;
                            VD.SetExtraVariables(25, newVars);
                        }
                    }
                }
            }
            else
            {
                if (data.extraVars.ContainsKey("outCondition"))
                {
                    if (data.extraVars.ContainsKey("condInfo"))
                    {
                        int[] nodeIDs = VD.ToIntArray((string)data.extraVars["outCondition"]);
                        if (VD.assigned.interactionCount < nodeIDs.Length)
                        {
                            VD.SetNode(nodeIDs[VD.assigned.interactionCount]);
                        }
                        else
                        {
                            VD.SetNode(nodeIDs[nodeIDs.Length - 1]);
                        }
                        return true;
                    }
                }

            }
        }
        return false;
    }

    private void PostConditions(VD.NodeData data)
    {
        if (data.pausedAction) return;

        if (!data.isPlayer)
        {
            ReplaceWord(data);

            if (data.extraData[data.commentIndex].Contains("fs"))
            {
                int fSize = 14;

                string[] fontSize = data.extraData[data.commentIndex].Split(","[0]);
                int.TryParse(fontSize[1], out fSize);
                NPC_Text.fontSize = fSize;
            }
            else
            {
                NPC_Text.fontSize = 14;
            }
        }
    }
    private void ReplaceWord(VD.NodeData data)
    {
        if (data.comments[data.commentIndex].Contains("[NAME]"))
        {
            data.comments[data.commentIndex] = data.comments[data.commentIndex].Replace("[NAME]", VD.assigned.gameObject.name);
        }
    }

    #endregion

    #region EVENTS AND HANDLERS

    private void OnLoadedAction()
    {
        Debug.Log("Finished loading all dialogues");
        VD.OnLoaded -= OnLoadedAction;
    }

    private void ActionHandler(int actionNodeID)
    {
        //Debug.Log("ACTION TRIGGERED: " + actionNodeID.ToString());
    }

    //Adds item to demo inventory, shows item popup, and pauses dialogue


    IEnumerator DrawText(string text, float time)
    {
        animatingText = true;

        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            if (i != words.Length - 1) word += " ";

            string previousText = NPC_Text.text;

            float lastHeight = NPC_Text.preferredHeight;
            NPC_Text.text += word;
            if (NPC_Text.preferredHeight > lastHeight)
            {
                previousText += System.Environment.NewLine;
            }

            for (int j = 0; j < word.Length; j++)
            {
                NPC_Text.text = previousText + word.Substring(0, j + 1);
                yield return new WaitForSeconds(time);
            }
        }
        NPC_Text.text = text;
        animatingText = false;
    }

    private void CutTextAnim()
    {
        StopCoroutine(NPC_TextAnimator);
        NPC_Text.text = VD.nodeData.comments[VD.nodeData.commentIndex]; 	
        animatingText = false;
    }

    public void UnlockMovement()
    {
        //FPL.UseLocomotion(true);
    }

    #endregion
}
