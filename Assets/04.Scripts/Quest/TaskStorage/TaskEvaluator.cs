using System.Collections.Generic;
using UnityEngine;

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

    protected string    _questtCode; 
    protected string    _taskCode;
    protected int       _goalAmount;


    public virtual void ReadySubscribe() { }
    public virtual void ReadyUnsubscribe() { }
    protected TaskEvaluator(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    {
        _targetID = targetID;
        _questData = questData;
        _questRunTimeProcess = questRunTimeProcess;
        _subTaskRunTimeProcess = subTaskRuntimeProcess;
        _questtCode =_questRunTimeProcess._runtimeProcess.QuestCODE;
        _taskCode = _subTaskRunTimeProcess._runTimeProcess.SubTaskCODE;

        _goalAmount = Managers.Quest.GetGoalAmount(_questtCode, _taskCode);
    }
    public void CheckTaskCompletedAndAcceptNextAfterIncreaseOne()
    {
        if (_goalAmount <= (++_subTaskRunTimeProcess._runTimeProcess.CurrentAmount))
        {
            //task 수행 완료 
            _subTaskRunTimeProcess._runTimeProcess.TaskState = eQuestTaskState.COMPLETED;
            SubTask nextTask = Managers.Quest.GetNextTask(_questtCode, _taskCode);

            if (null !=nextTask)
            {

                SubTaskRuntimeProcess subTaskRuntimeProcessNext = Managers.Quest.GetTaskRunTimeProcess(_questtCode, nextTask.SubTaskCODE);
                subTaskRuntimeProcessNext._runTimeProcess.TaskState = eQuestTaskState.ACCEPTED;
            }

            Event_TaskUpdated evt = new Event_TaskUpdated(_questtCode, _taskCode);
            Managers.Event.Publish<Event_TaskUpdated>(evt);
        }
    }
    public void justCheckNowAndCheckNext()
    {
        SubTask nextTask = Managers.Quest.GetNextTask(_questtCode, _taskCode);
        SubTaskRuntimeProcess subTaskRuntimeProcessNext = null;
        if (null!=nextTask)
        {
            subTaskRuntimeProcessNext = Managers.Quest.GetTaskRunTimeProcess(_questtCode, nextTask.SubTaskCODE);
        }

        if (_goalAmount <= _subTaskRunTimeProcess._runTimeProcess.CurrentAmount)
        {
            

            //task 수행 완료 
            _subTaskRunTimeProcess._runTimeProcess.TaskState = eQuestTaskState.COMPLETED;
            if (null ==subTaskRuntimeProcessNext) return;
            subTaskRuntimeProcessNext._runTimeProcess.TaskState = eQuestTaskState.ACCEPTED;

            Event_TaskUpdated evt = new Event_TaskUpdated(_questtCode, _taskCode);
            Managers.Event.Publish<Event_TaskUpdated>(evt);
        }
        else
        {
            _subTaskRunTimeProcess._runTimeProcess.TaskState= eQuestTaskState.ACCEPTED;
            if (null ==subTaskRuntimeProcessNext) return;
            subTaskRuntimeProcessNext._runTimeProcess.TaskState = eQuestTaskState.NOT_STARTED;

        }

    }
   public void CheckAndCallCompleteEvent()
    {
        //전체 quest 완료 했는 지 확인   
        List<SubTask> subTasks = _questData.tasks;
        bool isCompleted = true;
        foreach (SubTask subTask in subTasks)
        {
            eQuestTaskState state = Managers.Quest.GetTaskRunTimeProcess(_questtCode, subTask.SubTaskCODE)._runTimeProcess.TaskState;
            if (eQuestTaskState.COMPLETED != state)
            {
                isCompleted = false;
                break;
            }
        }
        if (true ==isCompleted)
        {
            foreach(SubTask subTask in subTasks)
            {
               SubTaskRuntimeProcess subTaskRuntime=   Managers.Quest.GetTaskRunTimeProcess(_questtCode, subTask.SubTaskCODE);
                subTaskRuntime._taskEvaluator.ReadyUnsubscribe();
            }
            _questRunTimeProcess._runtimeProcess.State =QUEST_STATE.QUEST_COMPLETED;

            Event_QuestCompleted QuestCompletedEvent = new Event_QuestCompleted(_questtCode);
            Managers.Event.Publish<Event_QuestCompleted>(QuestCompletedEvent);// 나 등록한 퀘스트 QuestData  처리 아니면 여기서 처리해도 되고                                                                  // 사실 그냥 여기서 처리하는게 성능 상 이점
            _questData.GiveReward(null);
        }
    }

}
