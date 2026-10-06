using System.Collections.Generic;

public enum eTASK_TYPE
{
    TALK,
    EQUIP,
    CONSUME,
    VISIT_AREA,
    LEARN_SKILL,
    KILL,
    COLLECT,
    REINFORCE,
}

public class TaskEvaluator
{
    protected int _targetID;
    protected QuestRuntimeProcess _questRunTimeProcess;
    protected SubTaskRuntimeProcess _subTaskRunTimeProcess;
    protected QuestData _questData;
    protected string _questtCode;
    protected string _taskCode;
    protected int _goalAmount;

    public virtual void ReadySubscribe() { }
    public virtual void ReadyUnsubscribe() { }

    protected TaskEvaluator(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess,
        SubTaskRuntimeProcess subTaskRuntimeProcess)
    {
        _targetID = targetID;
        _questData = questData;
        _questRunTimeProcess = questRunTimeProcess;
        _subTaskRunTimeProcess = subTaskRuntimeProcess;
        _questtCode = questRunTimeProcess._runtimeProcess.QuestCODE;
        _taskCode = subTaskRuntimeProcess._runTimeProcess.SubTaskCODE;
        _goalAmount = Managers.Quest.GetGoalAmount(_questtCode, _taskCode);
    }

    public void CheckTaskCompletedAndAcceptNextAfterIncreaseOne()
    {
        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;
        ++_subTaskRunTimeProcess._runTimeProcess.CurrentAmount;
        justCheckNowAndCheckNext();
    }

    public void justCheckNowAndCheckNext()
    {
        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;
        if (_goalAmount <= _subTaskRunTimeProcess._runTimeProcess.CurrentAmount)
        {
            _subTaskRunTimeProcess._runTimeProcess.TaskState = eQuestTaskState.COMPLETED;
            ReadyUnsubscribe();
            Managers.Quest.ActivateNextTask(_questtCode, _taskCode);
        }
        Managers.Event.Publish(new Event_TaskUpdated(_questtCode, _taskCode));
    }

    public void CheckAndCallCompleteEvent()
    {
        if (_questRunTimeProcess._runtimeProcess.State != QUEST_STATE.QUEST_ACCEPTED) return;
        List<SubTask> subTasks = _questData.tasks;
        foreach (SubTask subTask in subTasks)
        {
            if (Managers.Quest.GetTaskRunTimeProcess(_questtCode, subTask.SubTaskCODE)
                    ._runTimeProcess.TaskState != eQuestTaskState.COMPLETED)
                return;
        }

        foreach (SubTask subTask in subTasks)
            Managers.Quest.GetTaskRunTimeProcess(_questtCode, subTask.SubTaskCODE)
                ._taskEvaluator.ReadyUnsubscribe();

        _questRunTimeProcess._runtimeProcess.State = QUEST_STATE.QUEST_COMPLETED;
        var completed = new Event_QuestCompleted(_questtCode);
        _questData.GiveReward(completed);
        Managers.Event.Publish(completed);
    }
}
