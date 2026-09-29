using UnityEngine;

public class Task_ReinforceItem:TaskEvaluator
{
    public Task_ReinforceItem(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
  : base(targetID, questData,questRunTimeProcess, subTaskRuntimeProcess)
    {

    }
}
