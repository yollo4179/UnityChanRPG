using UnityEngine;

public class Event_TalkToNPC :CEvent
{
   public int ID_NPC; 
   public string QuestCode;
    public string TaskCode; 
   
    public Event_TalkToNPC(int _ID_NPC, string questCODE, string taskCODE)
    {
        ID_NPC = _ID_NPC;
        QuestCode = questCODE;
        TaskCode = taskCODE;
    }
    /*Publicsh ->Type ºñ±³ IsCompleted =¤·¤»*/

}
