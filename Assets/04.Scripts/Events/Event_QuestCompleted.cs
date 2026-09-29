using UnityEngine;

public class Event_QuestCompleted :CEvent
{
    string QuestCode; 
    
    public Event_QuestCompleted( string questCode)
    {
        QuestCode = questCode;
    }
}
