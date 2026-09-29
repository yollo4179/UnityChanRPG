using UnityEngine;

public class Task_LearnSkill :TaskEvaluator
{
    public Task_LearnSkill(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
   : base(targetID, questData,questRunTimeProcess, subTaskRuntimeProcess)
    {
        
    }

    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_UseSkillPoint>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_UseSkillPoint>(Execute);
    }
    public void Execute(Event_UseSkillPoint evt) 
    {
        if (null ==evt) return;


        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;

        CheckTaskCompletedAndAcceptNextAfterIncreaseOne();
        CheckAndCallCompleteEvent(); //여기서 끊내든가 담으로 넘어가든가.

    }
}
