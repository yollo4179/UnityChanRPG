using UnityEngine;

public class Event_UseSkillPoint :CEvent
{
    public int _skillID; 

    public Event_UseSkillPoint(int skillID=0 )
    {
        _skillID = skillID;
    }
    
}
