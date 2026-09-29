using UnityEngine;

public class Task_EquipItem : TaskEvaluator
{
    public Task_EquipItem(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    : base(targetID, questData, questRunTimeProcess, subTaskRuntimeProcess)
    {
        Managers.Event.Subscribe<Event_EquipItem>(Execute);
    }
    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_EquipItem>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_EquipItem>(Execute);
    }
    public void Execute(Event_EquipItem evt)
    {
        if (evt== null) return;

        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return; //바로 포블리시 날리기 
        _subTaskRunTimeProcess._runTimeProcess.CurrentAmount = evt.NumEquippedItems;
        justCheckNowAndCheckNext();
        CheckAndCallCompleteEvent();

    }
}
