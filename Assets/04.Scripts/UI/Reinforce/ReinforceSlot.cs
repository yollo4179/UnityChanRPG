using UnityEngine;

public class ReinforceSlot : UI_Base
{
    //Add Component Droppable은외부에서  추가 
    //타입에 따라 자동 배치 알고리즘 작성 

    public override void Init()
    {

    }
    ItemData _itemData;
   
    eEQUIPMENTTYPE _equipmentType;
   
    
    public ItemData GetItemData()
    {
        return _itemData;
    }
    public override void FixDropItem(in RectTransform rect)
    {
        FixItem(rect, new Vector2(150, 150));
    }
    public override void EmptySlot(ItemDataStorage itemData = null)
    {
        //나 비우기 //게임 오브젝트 채로(icon이 드래거불 (최상위 부모) 
        UI_Draggable_Move draggable = GetComponentInChildren<UI_Draggable_Move>();
        if (draggable == null) return;
        draggable.transform.SetParent(draggable.OriginTransform);
        draggable.SetOriginTransform(draggable.OriginTransform); //원래 장비창으로 
    }

}