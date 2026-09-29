using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Event_KillTarget : CEvent
{
    public int MonsterID;
    
    public Event_KillTarget(int monsterID)
    {
        this.MonsterID = monsterID;
        
    }
}
