using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class UI_EquipmentSlot : UI_Base
{
    //Add Compoennt Dropable은외부에서  추가 
    //타입에 따라 자동 배치 알고리즘 작성 

    public override void Init()
    {
        
    }
    ItemData _itemData;
    bool _isEqupped = false;    
    eEQUIPMENTTYPE _equipmentType;
    public void SetEquipmentType(eEQUIPMENTTYPE type)
    {
        _equipmentType = type; 
    }   
    public eEQUIPMENTTYPE GetEquipmentType()
    {
        return _equipmentType; 
    }
    public bool IsEqupped()
    {
        return _isEqupped; 
    }
    public void Equip(ItemData itemData)
    {
        _itemData = itemData;
        _isEqupped = true;
       //director에서 아이템 계산 결과 합산, 
       //장착 해제시에는 빼주기
    }
    public void UnEqup ()
    {
        _isEqupped = false;
        _itemData = null; 
    }
    public ItemData GetItemData()
    {
        return _itemData;
    }
    public override void FixDropItem(in RectTransform rect)
    {
        FixItem(rect, new Vector2(80, 80));
    }
    public override void EmptySlot(ItemDataStorage itemData=null)
    {
        //나 비우기 //게임 오브젝트 채로(icon이 드래거불 (최상위 부모) 
        UI_Draggable_Move draggable= GetComponentInChildren<UI_Draggable_Move>();
        if (draggable == null) return;  
        draggable.transform.SetParent(draggable.OriginTransform);
        draggable.SetOriginTransform(draggable.OriginTransform); //원래 장비창으로 
    }

}  