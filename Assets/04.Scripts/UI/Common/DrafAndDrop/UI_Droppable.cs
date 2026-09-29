using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class UI_Droppable : MonoBehaviour, IPointerEnterHandler, IDropHandler, IPointerExitHandler//슬롯의 컴포넌트
{


    private Image image;
    private RectTransform rect;

    public void Awake()
    {
        rect= GetComponent<RectTransform>();
        image = GetComponentInChildren<Image>();
    }

    public void OnDrop(PointerEventData eventData) // 나 슬롯
    {
        if (eventData.pointerDrag.transform.name !="ItemIcon_Prefab") return;
        /*오브젝트 새로 생성*/
        GameObject itemGO = Managers.Pool.LendPoolableTo("ItemIcon_Prefab", null).gameObject;
        itemGO.GetComponent<ItemDataStorage>().Init();

        /*드래그 가능한 오브젝트의 복사 완료*/
        ItemDataStorage copyTargetIDS = eventData.pointerDrag.GetComponentInChildren<ItemDataStorage>();
        eITEMTYPE type = copyTargetIDS.GetItemType();
        switch(type)
        {
            case eITEMTYPE.SKILL:
                itemGO.GetComponentInChildren<ItemDataStorage>().UpdateSkillSOData(copyTargetIDS.GetSkillSO());
                break;
            default:
                itemGO.GetComponentInChildren<ItemDataStorage>().UpdateItemData(copyTargetIDS);
                break;
        }
        UI_Draggable_Move eventDrag = eventData.pointerDrag.GetComponentInChildren<UI_Draggable_Move>();
        if (null != eventDrag.OriginTransform)
        {
            eventDrag.GetBackToOrigin();
            eventDrag.transform.SetAsLastSibling();
        }
        itemGO.GetOrAddComponent<UI_Draggable_Move>().SetDraggable(eventDrag);


        
        
            /*변수 준비*/
        ItemDataStorage StorageFromTarget = itemGO.GetComponentInChildren<ItemDataStorage>();
        ItemDataStorage storageFromThis = this.GetComponentInChildren<ItemDataStorage>();
        UI_Draggable_Move uiDraggableFromTarget = itemGO.GetComponent<UI_Draggable_Move>();
        UI_Draggable_Move uiDraggableFromThis = storageFromThis?.gameObject.GetComponent<UI_Draggable_Move>();


        /*1-같은 슬롯에 아이템이 들어 있는 경우*/
        if (null!=StorageFromTarget && null != storageFromThis)
        {
            switch (uiDraggableFromTarget.IsPrevOriginal)
            {
                /*현재 오브젝트가 */
                case true:
                    Managers.Pool.GetBack(storageFromThis.gameObject.GetComponent<Poolable>());
                    break;
                case false:
                    /*내가 원래 있던 자리가 있는 경우*/
                    /*1 내가 있던 자리에 자식으로 등록한다.*/
                    storageFromThis.gameObject.transform.SetParent(uiDraggableFromTarget.PreviousTransform);
                    /*내가 있던 자리에 맞는 사이즈로 변경한다.*/
                    RectTransform rectex = storageFromThis.gameObject.GetComponentInChildren<RectTransform>();
                    uiDraggableFromTarget.PreviousTransform.gameObject.GetComponentInChildren<UI_Base>().FixDropItem(rectex);
                    /*내가 있던 자리의 퀵슬롯을 캐싱한다.*/
                    Transform nowTransform = uiDraggableFromThis.PreviousTransform = uiDraggableFromTarget.PreviousTransform;
                    /*퀵슬롯 디렉터에 해당 아이템을 등록한다. */
                    gameObject.GetComponentInChildren<UI_Base>().EmptySlotKey(storageFromThis);
                    nowTransform.gameObject.GetComponentInChildren<UI_Base>().SetSlotKey(storageFromThis);
                    break;

            }
        }
        /*같은 아이템(Info 혹은 SkillSO가 존재한다면, 거기는 일단 없애버린다.)*/
        if (gameObject.GetComponentInChildren<UI_Base>().HasSameItem(StorageFromTarget))
        {
            //그냥 옮기는 경우
            if (true ==uiDraggableFromTarget.IsPrevOriginal)
                gameObject.GetComponentInChildren<UI_Base>().EmptySlot(StorageFromTarget); //원래 자리하고 있던 친구가 있다면 지우고 (슬롯 키로 찾아서 지우고 내부에서 슬롯 키 제거)

        }
        uiDraggableFromTarget.IsPrevOriginal=false;
        uiDraggableFromTarget.PreviousTransform=transform;
        itemGO.transform.SetParent(transform);
        RectTransform rect = itemGO.GetComponentInChildren<RectTransform>();
        gameObject.GetComponentInChildren<UI_Base>()?.FixDropItem(rect);

        gameObject.GetComponentInChildren<UI_Base>().EmptySlotKey(StorageFromTarget);
        gameObject.GetComponentInChildren<UI_Base>().SetSlotKey(StorageFromTarget);

        Action GetBackToPool = () =>
        {
            gameObject.GetComponentInChildren<UI_Base>().EmptySlotKey(StorageFromTarget);
            Managers.Pool.GetBack(itemGO.GetComponent<Poolable>());

        };
        uiDraggableFromTarget.DropEvent = null;
        uiDraggableFromTarget.DropEvent += GetBackToPool; //Drop시 지우는 거 잊지 말고 추가

      




        //    if (gameObject.GetComponentInChildren<UI_Base>().HasSameItem(StorageFromTarget))
        //    {
        //        //그냥 옮기는 경우
        //        if(true ==uiDraggableFromTarget.IsPrevOriginal)
        //            gameObject.GetComponentInChildren<UI_Base>().EmptySlot(StorageFromTarget); //원래 자리하고 있던 친구가 있다면 지우고 (슬롯 키로 찾아서 지우고 내부에서 슬롯 키 제거)


        //    }
        //    /*내  슬롯에 추가*/

        //    uiDraggableFromTarget.IsPrevOriginal=false;
        //    uiDraggableFromTarget.PreviousTransform=transform;
        //    eventData.pointerDrag.transform.SetParent(transform);
        //    RectTransform rect = eventData.pointerDrag.GetComponentInChildren<RectTransform>();
        //    gameObject.GetComponentInChildren<UI_Base>()?.FixDropItem(rect);

        //    gameObject.GetComponentInChildren<UI_Base>().EmptySlotKey(StorageFromTarget);
        //    gameObject.GetComponentInChildren<UI_Base>().SetSlotKey(StorageFromTarget);

        //}


    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //image?.color = Color.yellow;
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //image.color = Color.white;
    }
}
