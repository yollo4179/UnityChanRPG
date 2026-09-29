using Item;
using UnityEngine;


public class Task_CollectItem : TaskEvaluator
{
    public Task_CollectItem(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    : base(targetID, questData, questRunTimeProcess, subTaskRuntimeProcess)
    {
        
    }
    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_AcquireItem>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_AcquireItem>(Execute);
    }
    public void Execute(Event_AcquireItem evt)
    {
        if (evt == null) return;
        if (_questRunTimeProcess._runtimeProcess.State == QUEST_STATE.QUEST_COMPLETED) return;


        if (_targetID != evt.ItemID) return;
        int amount = Managers.Inventory.AmountItem(_targetID);

        
        //모아서 가져가기
       
        justCheckNowAndCheckNext();
        CheckAndCallCompleteEvent();

    }
}
