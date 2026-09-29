using UnityEngine;

public class Event_AcquireItem
{
    public int ItemID;
    public int Amount;
    public Event_AcquireItem(int itemID, int amount)
    {
        this.ItemID=itemID;
        Amount=amount;
    }
}
