using UnityEngine;

public enum eQuestTaskState
{
    NOT_STARTED,
    ACCEPTED,
    COMPLETED,
    END,
}
public class QuestTaskInfo
{
    public int QuestID;
    public int TaskID;
    public int CurrentAmount;
    public int GaolAmount;
    public eQuestTaskState TaskState;
    public string QuestExplanation;
    public eTASK_TYPE Type;


    public TaskEvaluator _taskEvaluator;
    public void SetEvaluator(TaskEvaluator taskEvaluator)
    {
        _taskEvaluator =taskEvaluator;
    }
    public TaskEvaluator GetEvaluator()
    {
        return _taskEvaluator;
    }
   

}
