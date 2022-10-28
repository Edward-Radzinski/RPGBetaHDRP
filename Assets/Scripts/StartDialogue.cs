using UnityEngine;
using System.Collections.Generic;
using Invector.vCharacterController;

public class StartDialogue : MonoBehaviour
{
    [SerializeField] private string _playerName = "VIDE User";
    public string playerName
    {
        get { return _playerName; }
        private set { _playerName = value; }
    }

    [SerializeField] private DialogueManager _dialogueUI;
    [SerializeField] private GameObject _dialoguePromt;
    private VIDE_Assign _isDialogTrigger;

    [SerializeField] private List<string> _items = new List<string>();        //for quest items
    [SerializeField] private List<string> _itemInventory = new List<string>();//

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.visible = !Cursor.visible;
            if (Cursor.visible)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<VIDE_Assign>() != null)
        {
            _isDialogTrigger = other.GetComponent<VIDE_Assign>();
            _dialoguePromt.SetActive(true);
        }
    }

    void OnTriggerExit()
    {
        _isDialogTrigger = null;
        _dialoguePromt.SetActive(false);
    }

    private void TryInteract()
    {
        if (_isDialogTrigger)
        {
            _dialoguePromt.SetActive(false);
            _dialogueUI.Interact(_isDialogTrigger);

            return;
        }

        //if more than 1 character//
        RaycastHit rHit;
        if (Physics.Raycast(transform.position, transform.forward, out rHit, 2))
        {
            VIDE_Assign assigned;
            if (rHit.collider.GetComponent<VIDE_Assign>())
            {
                assigned = rHit.collider.GetComponent<VIDE_Assign>();
                _dialoguePromt.SetActive(false);
            }
            else return;
            _dialogueUI.Interact(assigned);
        }
    }
}
