using UnityEngine;
public abstract class Quest : MonoBehaviour
{
    public abstract void OnBeginDialog();
    public abstract void OnEndQuest();
    public abstract void OnCompleteQuestTask();
}
