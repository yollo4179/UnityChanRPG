using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework.Interfaces;

public class Task_TalkToNPC : TaskEvaluator
{
    public Task_TalkToNPC(int targetID, QuestData questData, QuestRuntimeProcess questRunTimeProcess, SubTaskRuntimeProcess subTaskRuntimeProcess)
    : base(targetID, questData,questRunTimeProcess, subTaskRuntimeProcess)
    {
        //Managers.Event.Subscribe<Event_TalkToNPC>(Execute);
    }
    public override void ReadySubscribe()
    {
        Managers.Event.Subscribe<Event_TalkToNPC>(Execute);
    }
    public override void ReadyUnsubscribe()
    {
        Managers.Event.UnSubscribe<Event_TalkToNPC>(Execute);
    }
    public  void Execute(Event_TalkToNPC evt)
    {
        if (null ==evt) return;

        if (_targetID != evt.ID_NPC) return;

        if (evt.QuestCode != _questRunTimeProcess._runtimeProcess.QuestCODE) return;

        if(evt.TaskCode != _subTaskRunTimeProcess._runTimeProcess.SubTaskCODE) return;
        SubTask subTask =  Managers.Quest.GetSubTask(_questtCode, _taskCode);
        if (eTASK_TYPE.TALK != subTask.Type) return;
        if (_subTaskRunTimeProcess._runTimeProcess.TaskState != eQuestTaskState.ACCEPTED) return;

        //내가 찾는 이벤트이다 
        //subStack하나 올리고 퀘스트 완료됐는지 체크하자.

        CheckTaskCompletedAndAcceptNextAfterIncreaseOne();
        CheckAndCallCompleteEvent();

        Managers.Event.UnSubscribe<Event_TalkToNPC>(Execute);
    }


}
