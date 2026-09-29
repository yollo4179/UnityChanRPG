using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class EvaluatorSpawner
{

    private  static  Dictionary<eTASK_TYPE, Func<int, QuestData,QuestRuntimeProcess, SubTaskRuntimeProcess, TaskEvaluator>> evaluatorsSpawner; 

    public static TaskEvaluator GetEvaluator(eTASK_TYPE taskType, int targetID, QuestData questData, QuestRuntimeProcess questRuntime, SubTaskRuntimeProcess subTaskRunTime )
    {   
        TaskEvaluator evaluator = evaluatorsSpawner[taskType].Invoke(targetID, questData, questRuntime, subTaskRunTime) ;
        return evaluator;
    }

    public static void Init()
    {
        evaluatorsSpawner= new Dictionary<eTASK_TYPE,  Func<int,QuestData, QuestRuntimeProcess ,SubTaskRuntimeProcess,TaskEvaluator>>();

        evaluatorsSpawner.Add(eTASK_TYPE.TALK, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) =>{return new Task_TalkToNPC(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess);});


        evaluatorsSpawner.Add(eTASK_TYPE.CONSUME, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_ConsumeItem(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.COLLECT, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_CollectItem(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.EQUIP, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_EquipItem(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.KILL, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_KillMonsters(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.LEARN_SKILL, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_LearnSkill(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.REINFORCE, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_ReinforceItem(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });
        evaluatorsSpawner.Add(eTASK_TYPE.VISIT_AREA, (targetID, questData, questRuntimeProcess, subTaskRuntimeProcess) => { return new Task_VisitArea(targetID, questData, questRuntimeProcess, subTaskRuntimeProcess); });

    }
   


}
