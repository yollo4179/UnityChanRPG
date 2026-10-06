using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using NUnit.Framework.Interfaces;

public class QuestManager 
{

    Dictionary <string, QuestData>_dicCurrentQuest;
    Dictionary <string,Dictionary<string, SubTaskRuntimeProcess>> _taskRuntimeProcesses;
    Dictionary<string, QuestRuntimeProcess> _questRunTimeProcesses;
    
    string _pathTask;
    string _pathQuest;
    //Dictionary<int, QuestInfo> _dicQuestInfo;
    //List<QuestTaskInfo> _tasks;
    public QUEST_STATE QuestState(string questCODE)
    {
        if (false ==_questRunTimeProcesses.ContainsKey(questCODE)) return QUEST_STATE.QUEST_END;

        return _questRunTimeProcesses[questCODE]._runtimeProcess.State;
    }
    public QUEST_STATE QuestState(QuestData questData)
    {

        string questCODE = questData.QuestCODE;

        if (false ==_questRunTimeProcesses.ContainsKey(questCODE)) return QUEST_STATE.QUEST_END;

        return _questRunTimeProcesses[questCODE]._runtimeProcess.State; 
    }
    public eQuestTaskState TaskState(string questCODE, string taskCODE)
    {
        
        if (false ==_questRunTimeProcesses.ContainsKey(questCODE)) return eQuestTaskState.END;
        return _taskRuntimeProcesses[questCODE][taskCODE]._runTimeProcess.TaskState;
    }
    public eQuestTaskState TaskState(QuestData questData,string taskCODE)
    {
        string questCODE = questData.QuestCODE;
        if (false ==_questRunTimeProcesses.ContainsKey(questCODE)) return eQuestTaskState.END;
        return _taskRuntimeProcesses[questCODE][taskCODE]._runTimeProcess.TaskState;
    }
    public eQuestTaskState TaskState(QuestData questData, SubTask subTask)
    {
        string questCODE = questData.QuestCODE;
        string subTaskCODE = subTask.SubTaskCODE;
        if (false ==_questRunTimeProcesses.ContainsKey(questCODE)) return eQuestTaskState.END;
        if (false == _taskRuntimeProcesses.ContainsKey(questCODE)) return eQuestTaskState.NOT_STARTED;
        return _taskRuntimeProcesses[questCODE][subTaskCODE]._runTimeProcess.TaskState;
    }
    public QuestData GetQuestData(string questCODE)
    {
        return _dicCurrentQuest[questCODE];
    }
    public int GetGoalAmount(string questCode, string taskCode)
    {
        Debug.Assert(true==_dicCurrentQuest.ContainsKey(questCode));
        List<SubTask>questDataList=  _dicCurrentQuest[questCode].tasks;

        int goalAmount = questDataList.FirstOrDefault(x=> x.SubTaskCODE ==taskCode  ).GoalAmount;
        return goalAmount;

    }
    public QuestRuntimeProcess GetQuestRuntimeProcess(QuestData questData)
    {
        if(questData == null) return null;
        if ( !_questRunTimeProcesses.ContainsKey(questData.QuestCODE)) return null;
        return _questRunTimeProcesses[questData.QuestCODE];  
    }
    public QuestRuntimeProcess GetQuestRuntimeProcess(string  questCODE)
    {
   
        if (!_questRunTimeProcesses.ContainsKey(questCODE)) return null;
        return _questRunTimeProcesses[questCODE];
    }

    public SubTask  GetSubTask(string questCode, string taskCode)
    {
        Debug.Assert(true==_dicCurrentQuest.ContainsKey(questCode));
        List<SubTask> questDataList = _dicCurrentQuest[questCode].tasks;

        SubTask task  = questDataList.FirstOrDefault(x => x.SubTaskCODE ==taskCode);
        return task;
    }
    public SubTask GetNextTask(string questCode, string taskCode)
    {
        
        SubTask task = GetSubTask(questCode, taskCode);
        List<SubTask> questDataList = _dicCurrentQuest[questCode].tasks;
        int index    = questDataList.IndexOf(task);
        Debug.Assert(index <  questDataList.Count);
        int nextIndex = index+1;

        if (nextIndex >= questDataList.Count) return null; 
        task = questDataList[nextIndex];

        return task;
    }
    public List<QuestRuntimeProcess> GetQuestRuntimeProcessList()
    {
        return _questRunTimeProcesses.Values.ToList();
    }
    public List<QuestData> GetQuestDataList ()
    {
        return _dicCurrentQuest.Values.ToList();
    }
    public bool HasStarted (string questCODE)
    {
        return _questRunTimeProcesses.ContainsKey(questCODE);
    }
    public bool ReadyQuest(QuestData questData)
    {
        if (false ==questData.CheckPreRequisites())
            return false;
        if (_questRunTimeProcesses.ContainsKey(questData.QuestCODE)) 
        {
            return false;
            // 이미 퀘스트가 존재한다면  시작하지 않은 상태일 경우에만 Ready상태로 바꾼다 .   
            //if (QUEST_STATE.QUEST_NOT_STARTED ==_questRunTimeProcesses[questData.QuestCODE]._runtimeProcess.State)
            //    _questRunTimeProcesses[questData.QuestCODE]._runtimeProcess.State = QUEST_STATE.QUEST_READY;
           
        }
        //이미 생성되었던 데이터가 없으면 새로 생성하고, Ready 상태로 만든다 . 
        MakePhaseSubTasks(questData, QUEST_STATE.QUEST_READY);
        return true;
    }
    public void UpdateActivatedQuests(Event_QuestCompleted evt)
    {
        UpdateReadyQuests();
    }
    public void UpdateReadyQuests(Event_QuestCompleted evt)
    {
        UpdateReadyQuests();
    }
    public void UpdateReadyQuests(Event_LevelUP evt)
    {
        UpdateReadyQuests();
    }
    public void UpdateReadyQuests()
    {
        foreach (var questData in _dicCurrentQuest)
        {
            ReadyQuest(questData.Value);
            if (questData.Value.useAutoAcception &&
                QuestState(questData.Value) == QUEST_STATE.QUEST_READY)
                ActivateQuest(questData.Value);
        }
    }
    public List<QuestRuntimeProcess> GetReadyQuests()
    {
        return _questRunTimeProcesses.Values.Where(x => x._runtimeProcess.State == QUEST_STATE.QUEST_READY).ToList();
    }
    public List<QuestRuntimeProcess>GetCompletedQuests()
    {
        return _questRunTimeProcesses.Values.Where(x => x._runtimeProcess.State == QUEST_STATE.QUEST_COMPLETED).ToList();
    }

    public List<QuestRuntimeProcess>GetOnQuests()
    {
        return _questRunTimeProcesses.Values.Where(x => x._runtimeProcess.State == QUEST_STATE.QUEST_ACCEPTED).ToList();
    }
    public void ActivateQuest(QuestData questData) //키만 주입하고 나중에 Load
    {
        if (false ==questData.CheckPreRequisites()) 
            return;
        if (!_questRunTimeProcesses.ContainsKey(questData.QuestCODE)) 
            return;

        //퀘스트 발행은 Ready상태일때 처음 발행할 수 있다 .
        if (_questRunTimeProcesses[questData.QuestCODE]._runtimeProcess.State !=QUEST_STATE.QUEST_READY) return;

        _questRunTimeProcesses[questData.QuestCODE]._runtimeProcess.State = QUEST_STATE.QUEST_ACCEPTED;
        if (!_taskRuntimeProcesses.ContainsKey(questData.QuestCODE))
            CreateTaskProcesses(questData);
        ReconcileTaskStates(questData);

        Event_QuestAtivated evt = new Event_QuestAtivated(questData.QuestCODE);
        Managers.Event.Publish<Event_QuestAtivated>(evt);
    }
   public void MakePhaseSubTasks(QuestData questData, QUEST_STATE qeuestState)
    {
        QuestProcessDTO questProcessDTO = new QuestProcessDTO();
        questProcessDTO.QuestCODE = questData.QuestCODE;
        questProcessDTO.State = qeuestState;
        QuestRuntimeProcess questProcess = new QuestRuntimeProcess(questProcessDTO);
        _questRunTimeProcesses[questData.QuestCODE] = questProcess;



        CreateTaskProcesses(questData);
        if (qeuestState == QUEST_STATE.QUEST_ACCEPTED)
            ReconcileTaskStates(questData);
    }

    private void CreateTaskProcesses(QuestData questData, IEnumerable<SubTaskProcessDTO> savedTasks = null)
    {
        var map = new Dictionary<string, SubTaskRuntimeProcess>();
        var saved = savedTasks?.GroupBy(task => task.SubTaskCODE)
            .ToDictionary(group => group.Key, group => group.First());
        foreach (var subTask in questData.tasks)
        {
            SubTaskProcessDTO subTaskProcess;
            if (saved == null || !saved.TryGetValue(subTask.SubTaskCODE, out subTaskProcess))
                subTaskProcess = new SubTaskProcessDTO
            {
                QuestCODE     = questData.QuestCODE,
                SubTaskCODE   = subTask.SubTaskCODE,
                CurrentAmount = 0,
                TaskState     = eQuestTaskState.NOT_STARTED
            };
            map[subTask.SubTaskCODE] =   new SubTaskRuntimeProcess(subTaskProcess);

            map[subTask.SubTaskCODE].InjectEvaluator(
                EvaluatorSpawner.GetEvaluator(
                    subTask.Type,
                    subTask.TargetID,
                    questData,
                    _questRunTimeProcesses[questData.QuestCODE],
                    map[subTask.SubTaskCODE]));

            //진행도 애트리뷰트, 이벤트 기반 평가자 주입
        }
        _taskRuntimeProcesses[questData.QuestCODE] = map;
    }

    private void ReconcileTaskStates(QuestData questData)
    {
        var questState = QuestState(questData);
        var map = _taskRuntimeProcesses[questData.QuestCODE];
        bool hasActiveTask = false;
        foreach (var task in questData.tasks)
        {
            var runtime = map[task.SubTaskCODE];
            var process = runtime._runTimeProcess;
            if (questState == QUEST_STATE.QUEST_COMPLETED ||
                (questState == QUEST_STATE.QUEST_ACCEPTED && process.CurrentAmount >= task.GoalAmount))
            {
                process.TaskState = eQuestTaskState.COMPLETED;
                process.CurrentAmount = Math.Max(process.CurrentAmount, task.GoalAmount);
            }
            if (questState == QUEST_STATE.QUEST_ACCEPTED && !hasActiveTask &&
                process.TaskState != eQuestTaskState.COMPLETED)
            {
                process.TaskState = eQuestTaskState.ACCEPTED;
                runtime._taskEvaluator.ReadySubscribe();
                hasActiveTask = true;
            }
            else if (questState != QUEST_STATE.QUEST_COMPLETED &&
                     process.TaskState != eQuestTaskState.COMPLETED)
                process.TaskState = eQuestTaskState.NOT_STARTED;
        }
        if (questState == QUEST_STATE.QUEST_ACCEPTED && !hasActiveTask)
            Debug.LogWarning($"Quest {questData.QuestCODE} has no unfinished task; completion will be checked on the next task event.");
    }

    public void ActivateNextTask(string questCode, string taskCode)
    {
        var tasks = _dicCurrentQuest[questCode].tasks;
        int index = tasks.FindIndex(task => task.SubTaskCODE == taskCode);
        for (int i = index + 1; i < tasks.Count; i++)
        {
            var runtime = _taskRuntimeProcesses[questCode][tasks[i].SubTaskCODE];
            if (runtime._runTimeProcess.TaskState == eQuestTaskState.COMPLETED)
                continue;
            runtime._runTimeProcess.TaskState = eQuestTaskState.ACCEPTED;
            runtime._taskEvaluator.ReadySubscribe();
            return;
        }
    }

    public SubTaskRuntimeProcess GetTaskRunTimeProcess(string questCode, string taskCode)
    {
        if (false ==_taskRuntimeProcesses.ContainsKey(questCode)) return null;
        if (false ==_taskRuntimeProcesses[questCode].ContainsKey(taskCode)) return null;
        return _taskRuntimeProcesses[questCode][taskCode];
    }
    public void SaveQuests()
    {
        var all = new List<QuestProcessDTO>();
        foreach (var pair in _questRunTimeProcesses)
        {
            all.Add(pair.Value._runtimeProcess);
        }
        var json = JsonConvert.SerializeObject(all, Formatting.Indented);
        File.WriteAllText(_pathQuest, json);
    }
    public void LoadQuests()
    {
        if (!File.Exists(_pathQuest)) return;
        var json = File.ReadAllText(_pathQuest);
        var list = JsonConvert.DeserializeObject<List<QuestProcessDTO>>(json);
        if (null==list) return;

        foreach (var questDTO in list)
        {
            if (questDTO == null || !_dicCurrentQuest.ContainsKey(questDTO.QuestCODE))
            {
                Debug.LogWarning($"Unknown quest in save: {questDTO?.QuestCODE}");
                continue;
            }
            QuestRuntimeProcess questRuntimeProcess = new QuestRuntimeProcess(questDTO);
            _questRunTimeProcesses[questRuntimeProcess._runtimeProcess.QuestCODE]= questRuntimeProcess;
        }
    }
    public void SaveTasks() //종료전에 완료된 혹은 진행중인 퀘스트들만 기록 
    {
        // 모든 런타임을 저장용 DTO로 평탄화
        var all = new List<SubTaskProcessDTO>();
        foreach (var (questID, subTasks) in _taskRuntimeProcesses)
        {
            foreach (var (_,runtimeProcesses) in subTasks)
            {
                all.Add(runtimeProcesses._runTimeProcess); // 이미 questId/subTaskId가 채워져 있음
            }
        }
        var json = JsonConvert.SerializeObject(all, Formatting.Indented);
        File.WriteAllText(_pathTask, json);
    }
    public void LoadAndJoinTasks()
    {
        List<SubTaskProcessDTO> list = null;
        if (File.Exists(_pathTask))
            list = JsonConvert.DeserializeObject<List<SubTaskProcessDTO>>(File.ReadAllText(_pathTask));

        foreach (var questPair in _questRunTimeProcesses)
        {
            var questData = _dicCurrentQuest[questPair.Key];
            CreateTaskProcesses(questData, list?.Where(task => task.QuestCODE == questPair.Key));
            ReconcileTaskStates(questData);
        }
    }

    public void Init()
    {

        
        
        _dicCurrentQuest = new Dictionary<string, QuestData>();
        _taskRuntimeProcesses = new Dictionary<string, Dictionary<string, SubTaskRuntimeProcess>>();
        _questRunTimeProcesses= new Dictionary<string, QuestRuntimeProcess>();


        EvaluatorSpawner.Init();
        _pathTask = Application.persistentDataPath+ "/QuestTaskProcess.json";
        _pathQuest =Application.persistentDataPath+"/QuestProcess.json";
        _dicCurrentQuest = new Dictionary<string, QuestData>();
        var catalogs = Resources.LoadAll<QuestCatalog>("Data/ScriptableObjects/Quests");
        var allQuestData  = catalogs.SelectMany(catalog=>catalog._quests).Distinct();// SelectMany로 평탄화 ->하나의 반복자로 ()
        
        foreach(var questData in allQuestData)
        {
            _dicCurrentQuest.Add(questData.QuestCODE, questData);    
        }
        
        LoadQuests();//진행 중인 퀘스트와 진행 완료된 퀘스트 들을 Load한다. 
        LoadAndJoinTasks();//테스크와 퀘스트를 join해서 역직렬화한다.

        UpdateReadyQuests();
        Managers.Event.Subscribe<Event_QuestCompleted>(UpdateReadyQuests);
        Managers.Event.Subscribe<Event_LevelUP>(UpdateReadyQuests);
        //Managers.Event.Subscribe<Event_QuestCompleted>(UpdateActivatedQuests);
    
    }
    public void OnDisable()
    {
        int a = 0;
    }
    public void OnDestroy()
    {
        int b = 0;
    }

    public void Clear()//x
    {
        SaveQuests();
        SaveTasks();
        



    
    }







}
