using UnityEngine;

public class Event_EnterArea : CEvent
{
    public int AreaID; 
    public Event_EnterArea(int areaID)
    {
        AreaID = areaID;
    }

}
