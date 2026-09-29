using TMPro;
using UnityEngine;

public class UI_QuickSlot : UI_Scene
{

    public  enum Texts
    {
        SlotNoText_TMP
    }
    private int _slotNO = 0;
    public int GetSlotNo() { return _slotNO;  }

    public void SetSlotNO(int slotNO )
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        (_objects[typeof(TextMeshProUGUI)][(int)Texts.SlotNoText_TMP] as TextMeshProUGUI).text = slotNO.ToString();
        _slotNO = slotNO; 

    }
    public override void FixDropItem(in RectTransform rect)
    {
        FixItem(rect, new Vector2(135, 135));
    }
    public override void SetSlotKey(ItemDataStorage ids)
    {
        switch(ids.GetItemType())
        {
            case eITEMTYPE.SKILL:
                GetComponentInParent<UI_DisplayQuickSlots>()?.AddKey(ids.GetSkillSO().handle, _slotNO); //+update
                break;
            default:
                GetComponentInChildren<UI_DisplayQuickSlots>()?.AddKey(ids.ItemInfo, _slotNO);
                break;
        }
        

    }

    public override bool HasSameItem(ItemDataStorage ids)
    {
        if (null==ids) return false;

        switch (ids.GetItemType())
        {
            case eITEMTYPE.SKILL:

                return GetComponentInParent<UI_DisplayQuickSlots>().HasKey(ids.GetSkillSO().handle);
            default:
                return GetComponentInParent<UI_DisplayQuickSlots>().HasKey(ids.ItemInfo);
        }
    }

    public override void EmptySlotKey(ItemDataStorage ids)
    {
        switch (ids.GetItemType())
        {
        case eITEMTYPE.SKILL:
                GetComponentInParent<UI_DisplayQuickSlots>().RemoveKey(ids.GetSkillSO().handle);
                break;

        default:
                GetComponentInParent<UI_DisplayQuickSlots>().RemoveKey(ids.ItemInfo);
                break;
        }

        

    }

   
    public override void EmptySlot(ItemDataStorage ids)// 같은 종류의 스킬이 드랍되면 미리 비우기 (새로운ㄴ 거 넣기 전에 )
    {
        //ToDo : 종류로 비교하지 않고 인스턴스 아이디로 비교 
        if (null!=ids)
        {
            UI_QuickSlot oth = null;
            //키 제거 
            switch (ids.GetItemType()) {
                case eITEMTYPE.SKILL:
                    oth = GetComponentInParent<UI_DisplayQuickSlots>().GetQuickSlotByHandle(ids.GetSkillSO().handle);
                    GetComponentInParent<UI_DisplayQuickSlots>().RemoveKey(ids.GetSkillSO().handle);
                    break; 
                default:
                    oth = GetComponentInParent<UI_DisplayQuickSlots>().GetQuickSlotByHandle(ids.ItemInfo);
                    GetComponentInParent<UI_DisplayQuickSlots>().RemoveKey(ids.ItemInfo);
                    break;
            }
            
            
            ItemDataStorage othDataStorage = oth.GetComponentInChildren<ItemDataStorage>();
            Managers.Pool.GetBack(othDataStorage.GetComponent<Poolable>());

            //UI_Draggable_Move othUIDraggable = othDataStorage.gameObject.GetComponent<UI_Draggable_Move>();   
            //othDataStorage.gameObject.transform.SetParent(othUIDraggable.OriginTransform);
            //RectTransform rect = othDataStorage.GetComponentInParent<RectTransform>();
            //othUIDraggable.OriginTransform.gameObject.GetComponentInParent<UI_Base>().FixDropItem(rect);



        }
    }

}
