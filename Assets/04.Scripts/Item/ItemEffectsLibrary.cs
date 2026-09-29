using NUnit.Framework.Interfaces;
using System;
using UnityEngine;


namespace Item
{
    public enum ITEM_ID
    {
        RED_POTION =5015 , 
        BLUE_POTION =5016,
        RANDOM_BOX =5017,
        END
    }
}
public static  class ItemEffectsLibrary
{

    public static Action<ItemInfo>[] ItemEffecsList = new Action<ItemInfo>[(int)Item.ITEM_ID.END];
    private static bool _hasBeenInitialized = false;
    public static void Init()
    {
        #region 붉은 포션 5015
        ItemEffecsList[(int)Item.ITEM_ID.RED_POTION] = (itemInfo)=> {
            ItemData itemData  = Managers.Data.GetItemData((int)Item.ITEM_ID.RED_POTION);

            StatusScript _status = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<StatusScript>();

            UI_PlayerStatusBar statusBar = Managers.UI.GetCachedUIByName("HUD_Canvas_Prefab").GetComponentInChildren<UI_PlayerStatusBar>();
            _status.CurHealth =(int)(_status.CurHealth * 1.3f);
            if(_status.CurHealth > _status.MaxHealth)
                _status.CurHealth = _status.MaxHealth;
            statusBar.UpdateSlider(_status,Player.ePlayerSlider.HEALTH ,3);
            //한 개 지우기
            Managers.Inventory.TryRemoveItem(itemInfo,1);

            //이벤트 할당 
            Managers.Event.Publish<Event_UseItem>(new Event_UseItem( itemInfo));//이미 줄여놨으니까 amount는 의미 없긴 하다.
        };
        #endregion 붉은 포션 5015

        #region 파란 포션 5016
        ItemEffecsList[(int)Item.ITEM_ID.BLUE_POTION] = (itemInfo) => {

            ItemData itemData = Managers.Data.GetItemData((int)Item.ITEM_ID.BLUE_POTION);

            StatusScript _status = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<StatusScript>();

            UI_PlayerStatusBar statusBar = Managers.UI.GetCachedUIByName("HUD_Canvas_Prefab").GetComponentInChildren<UI_PlayerStatusBar>();
            _status.CurMana=(int)(_status.CurMana * 1.3f);
            if (_status.CurMana> _status.CurMana)
                _status.CurMana = _status.CurMana;
            statusBar.UpdateSlider(_status, Player.ePlayerSlider.MANA, 3);
            Managers.Inventory.TryRemoveItem(itemInfo, 1);

            Managers.Event.Publish<Event_UseItem>(new Event_UseItem( itemInfo));//이미 줄여놨으니까 amount는 의미 없긴 하다.
        };
        #endregion 파란 포션 5016
        #region 랜덤박스 5017
        ItemEffecsList[(int)Item.ITEM_ID.RANDOM_BOX] = (itemInfo) => {
            ItemData itemData = Managers.Data.GetItemData((int)Item.ITEM_ID.RANDOM_BOX);
            Managers.Inventory.TryRemoveItem(itemInfo, 1);
            Managers.Event.Publish<Event_UseItem>(new Event_UseItem( itemInfo));//이미 줄여놨으니까 amount는 의미 없긴 하다.
        };
        #endregion 랜덤박스 5017
    }
    public static void UseItem(ItemInfo itemInfo)
    {
        if(false ==_hasBeenInitialized)
        {
            _hasBeenInitialized = true;
            Init();  
        }

        ItemEffecsList[itemInfo.ID]?.Invoke(itemInfo);
    }



    





}
