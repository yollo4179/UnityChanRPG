using UnityEngine;

public class Event_UseItem :CEvent
{
     
  
   
    public ItemInfo _itemInfo;
    //ID만 넘겨주고 데이터 매니저에서 ID 찾아서 갱신  
    public Event_UseItem( ItemInfo itemInfo)
    {
      
        _itemInfo=itemInfo;
    }
}
