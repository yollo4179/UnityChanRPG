using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
public enum QUEST_STATE
{
    QUEST_NOT_STARTED,
    QUEST_READY,
    QUEST_ACCEPTED,
    QUEST_COMPLETED,
    QUEST_END
}
public class QuestInfo
{
    /*JsonLoad*/
    public int QuestID;
    [JsonConverter(typeof(StringEnumConverter))]
    QUEST_STATE State;
    /*InjectTasks*/
    public List<QuestTaskInfo> TaskList =new List<QuestTaskInfo>();
    public void AddTask(QuestTaskInfo task)
    {
        TaskList.Add(task);
    }

}
