using UnityEngine;

public class Event_ConsumeItem
{
    int itemID;
    int consumeAmount;

    Event_ConsumeItem(int itemID, int consumeAmount)
    {
        this.itemID=itemID;
        this.consumeAmount=consumeAmount;
    }
}
