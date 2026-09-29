using UnityEngine;

public class Event_TaskUpdated :CEvent
{
    public string QuestCODE;
    public string TaskCODE; 

    public Event_TaskUpdated(string questCODE, string taskCODE)
    {
        QuestCODE=questCODE;
        TaskCODE=taskCODE;
    }
}
