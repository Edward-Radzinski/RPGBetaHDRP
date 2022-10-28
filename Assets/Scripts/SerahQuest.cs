using UnityEngine;

public class SerahQuest : Quest
{
    private Animator _animator;
    private GetQuest _playerQuest;
    private VIDE_Assign _myDialog;
    [SerializeField] private SkinnedMeshRenderer _serahTop;

    [SerializeField] private float _keyParts = 0;
    [SerializeField] private string _questLabel;
    [SerializeField] private string _questDiscription;

    private bool _questBeginning = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _myDialog = GetComponent<VIDE_Assign>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<GetQuest>())
        {
            _playerQuest = other.GetComponent<GetQuest>();
        }
    }

    public override void OnBeginDialog()
    {
        if (_questBeginning) return;
        _playerQuest.GetQuestTask(_questLabel, _questDiscription, _keyParts, this);
        _questBeginning = true;
    }

    public override void OnEndQuest()
    {
        _animator.SetTrigger("HipHop");
        _serahTop.enabled = false;
    }

    public override void OnCompleteQuestTask()
    {
        _myDialog.overrideStartNode = 9; //Dialog ID
    }

    public void EventDialogSadEmotion()
    {
        _animator.SetTrigger("Sad");
    }
    public void EventDialogHappyEmotion()
    {
        _animator.SetTrigger("Happy");
    }
}
