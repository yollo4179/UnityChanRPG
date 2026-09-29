using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
public class UI_QuestDisplayContents : UI_Base
{


    public enum TMPs {
        QuestName_TMP,
        QuestDescription_TMP,
    }
    public enum GameObjects
    {
        TaskProcessMask_GO
    }

    TextMeshProUGUI _textQuestName;
    TextMeshProUGUI _textQuestDesc; 
    GameObject      _taskParent;

    QuestData _questData;
    QuestRuntimeProcess _process;


    private Dictionary<Poolable,SubTask> _textTaskHandles;
    private Dictionary<string, Poolable> _dicTasks;
    bool hasInited = false; 
    public override void Init()
    {
        if(true==hasInited) { return; }
        hasInited =true; 
        Bind<TextMeshProUGUI>(typeof(TMPs));
        Bind<GameObject>(typeof(GameObjects));
        _textQuestName = Get<TextMeshProUGUI>((int)TMPs.QuestName_TMP);
        _textQuestDesc = Get<TextMeshProUGUI>((int)TMPs.QuestDescription_TMP);
        _taskParent = Get<GameObject>((int)GameObjects.TaskProcessMask_GO);

        _dicTasks = new Dictionary<string, Poolable>();
        _textTaskHandles = new Dictionary<Poolable, SubTask>();
    }
    public void SetQuestData (QuestData questData)
    {
        Init();
        _textQuestName.text = "";
        _textQuestDesc.text = "";
        ClearHandles();
        if (null==questData) return;
        Managers.Event.UnSubscribe<Event_TaskUpdated>(TaskChanged);
       
        _questData =  questData;
        _textQuestName.text = questData.QuestTitle;
        _textQuestDesc.text = questData.QuestDescription;
       
        foreach ( var task in _questData.tasks)
        {
            Poolable pool = Managers.Pool.LendPoolableTo("TaskProcess_TMP", _taskParent.transform);
            _textTaskHandles.Add(pool,task);
            _dicTasks.Add(task.SubTaskCODE, pool);
            updateTaskDisplay(pool,task);

        }
        Managers.Event.Subscribe<Event_TaskUpdated>(TaskChanged);
    }

    public void TaskChanged(Event_TaskUpdated evt)
    {
        if (false == enabled) return;
        if (evt.QuestCODE != _questData.QuestCODE) return;
        if (!_dicTasks.ContainsKey(evt.TaskCODE)) return;

        Poolable handle = _dicTasks[evt.TaskCODE];
        updateTaskDisplay(handle, _textTaskHandles[handle]);


    }

    public void updateTaskDisplay(Poolable handle, SubTask task )
    {
        TextMeshProUGUI TMP = handle.GetComponent<TextMeshProUGUI>();
        string taskExplanation = task.TaskExplantion;
        TMP.text ="???";
        TMP.color = Color.gray;
        SubTaskRuntimeProcess taskRuntime = Managers.Quest.GetTaskRunTimeProcess(_questData.QuestCODE, task.SubTaskCODE);

        
        if (null ==taskRuntime) return;
        if (taskRuntime._runTimeProcess.TaskState ==eQuestTaskState.NOT_STARTED) return;
        int currentAmount = taskRuntime._runTimeProcess.CurrentAmount;
        int goalAmount = task.GoalAmount;
        TMP.text =taskExplanation + currentAmount +"/" +goalAmount;

        switch (taskRuntime._runTimeProcess.TaskState)
        {
            case eQuestTaskState.COMPLETED:
                TMP.color = Color.green;
                TMP.fontStyle|=FontStyles.Strikethrough;
                break;
            case eQuestTaskState.ACCEPTED:
                TMP.color = Color.white;
                if( 0<( TMP.fontStyle & FontStyles.Strikethrough) )
                TMP.fontStyle ^=FontStyles.Strikethrough;
                break;
            case eQuestTaskState.NOT_STARTED:
                TMP.color = Color.gray;
                if (0<(TMP.fontStyle & FontStyles.Strikethrough))
                    TMP.fontStyle ^=FontStyles.Strikethrough;
                break;
        }

    }
    public void ClearHandles()
    {
        foreach(var pair in _textTaskHandles)
        {
            Managers.Pool.GetBack(pair.Key);
        }
        _textTaskHandles.Clear();

        _dicTasks.Clear();
    }
    public void UnSubscribe()
    {
        Managers.Event.UnSubscribe<Event_TaskUpdated>(TaskChanged);
    }

}
