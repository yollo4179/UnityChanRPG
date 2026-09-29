using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;



[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{




    [Header("Quest Description")]
    [SerializeField]public  string QuestTitle;
    [TextArea(5,5)]
    [SerializeField]public  string QuestDescription;

    [Header("Contents")]
    [SerializeField] public List<SubTask> tasks ;
   

    [Header("Reward")]
    [SerializeField]public QuestRewards QuestReward;


    [Header("Condition")]
    [SerializeField] public QuestPrerequisite AcceptionCondition;
    [Header("NPC ")]
    [SerializeField] public string QuestCODE; //내가 임의로 지정
    [SerializeField] public int NPC_ID; //없어도 됧듯

    public bool useAutoComplete = false;
    public bool useAutoAcception = false; 
    public bool CheckPreRequisites()
    {
        Debug.Assert(null !=AcceptionCondition);

        return  AcceptionCondition.CheckCondition();
        
    }

    public void GiveReward(Event_QuestCompleted evt)
    {

       
        int gold = QuestReward.Gold;
        int exp = QuestReward.Exp;
       foreach(var Reward in QuestReward.RewardItemSets)
        {
            eITEMTYPE type =  Managers.Data.GetItemData(Reward.ItemID).Type;
            Managers.Inventory.TryAddItem(type, Reward.ItemID,Reward.ItemAmount);
        }
        Managers.Player.AddMoney(gold);



    }

}

[Serializable] public class QuestRewards
{
    /*골드 아이템 겨험치*/
    [SerializeField]    public int Exp;
    [SerializeField]    public int Gold;
    [SerializeField]    public QuestRewardItemSet[] RewardItemSets;
    
}
[Serializable] public class QuestRewardItemSet
{
    
    [SerializeField]
    public int ItemID;
    [SerializeField]
    public int ItemAmount;
}


public class QuestProcessDTO
{
    //저장용
    public string QuestCODE;
    [JsonConverter(typeof(StringEnumConverter))]
    public QUEST_STATE State;

  
    //저장용

}
[System.Serializable] public class SubTask
{
    [SerializeField] public string       SubTaskCODE;
    [SerializeField] public int          TargetID;
    [SerializeField] public string       TaskExplantion;
    [SerializeField] public int          GoalAmount;
    [SerializeField] public string       DialogKey;
    [SerializeField] public int          NPCID;
    [SerializeField] public eTASK_TYPE   Type;
}
public class SubTaskProcessDTO
{
    //저장용
    public string QuestCODE;
    public string SubTaskCODE;
    public int CurrentAmount;

    [JsonConverter(typeof(StringEnumConverter))]
    public eQuestTaskState TaskState;
    //저장용
    
}
public class SubTaskRuntimeProcess
{
    public SubTaskProcessDTO _runTimeProcess;
    public TaskEvaluator _taskEvaluator;

    public SubTaskRuntimeProcess(SubTaskProcessDTO save)
    {
        _runTimeProcess = save;
    }
    public void InjectEvaluator(TaskEvaluator taskEvaluator)
    {
        _taskEvaluator = taskEvaluator;
    }
   
}
public class QuestRuntimeProcess //테스크 하나 끝낼때마다 확인해서 여부 결정
{
    public QuestRuntimeProcess(QuestProcessDTO questProcessDTO)
    {
        _runtimeProcess =questProcessDTO; 
    }
    public QuestProcessDTO _runtimeProcess;


}
