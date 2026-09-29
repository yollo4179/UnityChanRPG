using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "QuestPrerequisite", menuName = "Scriptable Objects/QuestPrerequisite")]
public class QuestPrerequisite : ScriptableObject
{

    [Header("QuestCode")]
    [SerializeField]public  List<string> PrerequisiteQuests;
    [Header("Open Level")]
    [SerializeField]public  int LimitedLevel;
    

    public bool CheckCondition()
    {
        foreach(var questCode in PrerequisiteQuests)
        {
            QUEST_STATE questState=  Managers.Quest.QuestState(questCode);
            if (QUEST_STATE.QUEST_COMPLETED != questState)
                return false;
        }
        if (Managers.Player.PlayerInfo.Level < LimitedLevel) return false; 


        return true;
    }


}
