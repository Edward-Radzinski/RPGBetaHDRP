using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GetQuest : MonoBehaviour
{
    //private List<Quest> _quests; //if more than 1 quest//
    private Quest _questMan;
    [SerializeField] private GameObject _questPanel;

    [SerializeField] private Text _questLabel;
    [SerializeField] private Text _questDiscription;
    [SerializeField] private Slider _questProgress;

    private bool _questStart = false;
    private float _partsOfQuest = float.MaxValue;
    private float _currentParts = 0;

    public void GetQuestTask(string QL, string QD, float POQ, Quest QM)
    {
        _questStart = true;

        _currentParts = _currentParts != 0 ? _currentParts : 0;
        _partsOfQuest = POQ;
        _questLabel.text = QL;
        _questDiscription.text = QD;
        _questMan = QM;
        _questProgress.value = _currentParts / _partsOfQuest * 100;
        _questPanel.SetActive(true);
        
        if (_currentParts >= _partsOfQuest)
        {
            CompleteTask();
        }
    }

    public void ChangeProgress(int value)
    {
        _currentParts += value;
        if (_questStart)
        {
            _questProgress.value = _currentParts / _partsOfQuest * 100;
            if (_currentParts >= _partsOfQuest)
            {
                CompleteTask();
            }
        }
    }

    private void CompleteTask()
    {
        _currentParts = 0;
        _questStart = false;
        _questMan.OnCompleteQuestTask();
        _questLabel.text += " [COMPLETE]";
    }
}
