using UnityEngine;

public class Task_ConsumeItem : TaskEvaluator
{

    public Task_ConsumeItem(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
     : base(targetID, questData, questRunTimeProcess, subTaskRuntimeProcess)
    {
       
    }
    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_UseItem>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_UseItem>(Execute);
    }
    public void Execute(Event_UseItem evt)
    {
        //사실 assert가 맞다. 
        if (evt == null) return;
        if (evt._itemInfo == null || evt._itemInfo.ID != _targetID) return;
        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;
        CheckTaskCompletedAndAcceptNextAfterIncreaseOne();
        CheckAndCallCompleteEvent(); 

    }

}
