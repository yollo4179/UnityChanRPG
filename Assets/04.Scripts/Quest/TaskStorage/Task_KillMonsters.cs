using UnityEngine;


public class Task_KillMonsters : TaskEvaluator
{
    public Task_KillMonsters(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    : base(targetID, questData,questRunTimeProcess, subTaskRuntimeProcess)
    {
        Managers.Event.Subscribe<Event_KillTarget>(Execute);
    }

    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_KillTarget>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_KillTarget>(Execute);
    }
    public void Execute(Event_KillTarget evt)
    {
        if ( null==evt) return;
        if (_targetID != evt.MonsterID) return;
        if(_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;

        CheckTaskCompletedAndAcceptNextAfterIncreaseOne();
        CheckAndCallCompleteEvent();

    }
}
