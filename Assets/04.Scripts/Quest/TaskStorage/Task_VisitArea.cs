using UnityEngine;


public class Task_VisitArea : TaskEvaluator
{
    public Task_VisitArea(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    : base(targetID, questData, questRunTimeProcess, subTaskRuntimeProcess)
    {
        //AREAid == targetID

        Managers.Event.Subscribe<Event_EnterArea>(Execute);
    }
    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_EnterArea>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_EnterArea>(Execute);
    }
    public void Execute(Event_EnterArea evt)
    {
        if (null==evt) return;

        if (_targetID !=evt.AreaID) return;
        
        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;

        ++_subTaskRunTimeProcess._runTimeProcess.CurrentAmount;

        justCheckNowAndCheckNext();
        CheckAndCallCompleteEvent();
        
        


    }

    
}
