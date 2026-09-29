using NUnit.Framework;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class QuestMiniPart : UI_Base
{

    public enum TMPs
    {
        QuestMini_TMP
    }
    public enum GameObjects
    {
        QuestMini_TMP
    }


    private bool _hasInitialized = false;
    private QuestData _questData = null;
    private Dictionary<Poolable,SubTask> _textTaskHandles;
    private Dictionary<string, Poolable> _dicTasks;
    private TextMeshProUGUI _textQuestTitle;
    private GameObject _tasksParent; 
    public override void Init()
    {
        if (true ==_hasInitialized) return; 
        _hasInitialized = true;
        Bind<TextMeshProUGUI>(typeof(TMPs));
        Bind<GameObject>(typeof(GameObjects));
        _tasksParent = Get<GameObject>((int)GameObjects.QuestMini_TMP);
        _textQuestTitle =Get<TextMeshProUGUI>((int)TMPs.QuestMini_TMP);
        _textTaskHandles = new Dictionary<Poolable, SubTask>();
        _dicTasks = new Dictionary<string, Poolable>();
    }
    public void Unsubscribe()
    {
        Managers.Event.UnSubscribe<Event_TaskUpdated>(TaskChanged);
    }
    public void SetQuestData (QuestData  questData )
    {

        
        _questData =questData;
        Init();
        _questData =  questData;
        _textQuestTitle.text = questData.QuestTitle;
       
        ClearHandles();
        foreach (var task in _questData.tasks)
        {
            Poolable pool = Managers.Pool.LendPoolableTo("TaskMini_TMP", _tasksParent.transform);
            _textTaskHandles.Add(pool, task);
            _dicTasks.Add(task.SubTaskCODE, pool);
            updateTaskDisplay(pool, task);
        }
        Managers.Event.UnSubscribe<Event_TaskUpdated>(TaskChanged);
        Managers.Event.Subscribe<Event_TaskUpdated>(TaskChanged);
    }

    public void TaskChanged(Event_TaskUpdated evt)
    {
        if (false == enabled) return;
        if (evt.QuestCODE != _questData.QuestCODE) return;

        if (!_dicTasks.ContainsKey(evt.TaskCODE)) return;

        //Poolable handle = _dicTasks[evt.TaskCODE];
        //updateTaskDisplay(handle, _textTaskHandles[handle]); //이벤트 받은 테스크만 업데이트 한다. 
        foreach (var task in _questData.tasks)
        {
            updateTaskDisplay(_dicTasks[task.SubTaskCODE], task); //여러개Task를 동시 수행하면 최적화 필요
        }
    }

    public void updateTaskDisplay(Poolable handle, SubTask task)
    {
        TextMeshProUGUI TMP = handle.GetComponent<TextMeshProUGUI>();
        string taskExplanation = task.TaskExplantion;
        TMP.text ="?????";
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
                if (0<(TMP.fontStyle & FontStyles.Strikethrough))
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


}
