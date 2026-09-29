using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Runtime.ConstrainedExecution;
using NUnit.Framework.Interfaces;
using Reinforce;
using JetBrains.Annotations;
using TMPro;
using System.Collections;
namespace Reinforce
{
    public class ReinforceResult
    {
        public int Damage;
        public int CriDamage;
        public int CriChance;
        public int Defense;
        public int MaxHP;
        public int MaxMP;
        public int ReinforceLevel;
        public string strReinforceResult;
        public bool bReinforceResult;  
    }
}

public class UI_ReinforceDirector : UI_Base, IPointerEnterHandler, IDropHandler, IPointerExitHandler
{
    
    public enum GameObjects
    {
        ExtraStatus,
        ItemFrame_GO,

    }
    public enum Buttons
    {
        Reinforce_Button,
        Close_Button

    }

    public ReinforceDisplayResult _resultDisplayer;
    private Button _btnReinforce;
    private Button _btnClose;
    private GameObject _slotOrigin;
    ItemDataStorage _nowIDS;
    UI_Popup _handle;
    Coroutine _co; 
    private void Awake()
    {
        Init();
    }

    public override void Init()
    {

        _handle = GetComponentInParent<UI_Popup>(); 
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));

        _btnReinforce = Get<Button>((int)Buttons.Reinforce_Button);
        _btnClose = Get<Button>((int)Buttons.Close_Button);
        _resultDisplayer =Get<GameObject>((int)GameObjects.ExtraStatus).GetComponent<ReinforceDisplayResult>();
        _resultDisplayer.Init();
        _slotOrigin = Get<GameObject>((int)GameObjects.ItemFrame_GO);

        _btnClose.onClick.AddListener(() => {

            OnClose();
        });

        _btnReinforce.onClick.AddListener(() => {
            if (null != _co) return;

            if(null != _nowIDS)
            {
                ItemInfo itemInfo = _nowIDS.ItemInfo;
                InstanceItemInfo instance = (itemInfo as InstanceItemInfo);
                if (null == instance) return;
                if (instance.CurrentReinforce >= instance.MaxReinforce) return;

                //TextMeshProUGUI text = _btnReinforce.transform.GetComponentInChildren<TextMeshProUGUI>();
                //if (Managers.Player.PlayerInfo.Money <instance.Level*60) {
                //    text.text = "잔액 부족";
                //    return;
                //}
                //text.text = "강화";
                Managers.Player.AddMoney(- instance.Level*60);

                int reinforceIntensity = instance.Level;
                int randomVar = UnityEngine.Random.Range(1, 4);//1~2 
                reinforceIntensity *= randomVar;
                ItemData  itemData= Managers.Data.GetItemData(itemInfo.ID);

                ReinforceResult result = new ReinforceResult();
                result.bReinforceResult = false;
                int ExtraStatus = UnityEngine.Random.Range(0, 2) ;

                switch (itemData.EquipmentType)
                {
                    case eEQUIPMENTTYPE.SHOES:
                        result.Defense +=reinforceIntensity*2;
                        if (ExtraStatus>0)
                        {
                            result.Damage +=reinforceIntensity/2;
                            result.CriChance +=reinforceIntensity/2;
                            result.CriDamage +=reinforceIntensity/2;
                        }
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;
                    case eEQUIPMENTTYPE.GLOVES:
                        result.Defense +=reinforceIntensity*2;
                        if (ExtraStatus>0)
                        {
                            result.Damage +=reinforceIntensity;
                            result.CriChance +=reinforceIntensity;
                            result.CriDamage +=reinforceIntensity;
                        }
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;
                    case eEQUIPMENTTYPE.WEAPON:
                        result.Defense +=reinforceIntensity*2;
                        result.Damage +=reinforceIntensity;
                        result.CriChance +=reinforceIntensity;
                        result.CriDamage +=reinforceIntensity;
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;
                    case eEQUIPMENTTYPE.ARMOR:
                        result.Defense +=reinforceIntensity*5;
                        if (ExtraStatus>0)
                        {
                            result.Damage +=reinforceIntensity/2;
                            result.CriChance +=reinforceIntensity/2;
                            result.CriDamage +=reinforceIntensity/2;
                        }
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;
                    case eEQUIPMENTTYPE.HEAD:
                        result.Defense +=reinforceIntensity*5;
                        if (ExtraStatus>0)
                        {
                            result.Damage +=reinforceIntensity/2;
                            result.CriChance +=reinforceIntensity/2;
                            result.CriDamage +=reinforceIntensity/2;
                        }
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;
                    case eEQUIPMENTTYPE.GEM:
                        result.Defense +=reinforceIntensity*2;          
                        result.Damage +=reinforceIntensity;
                        result.CriChance +=reinforceIntensity;
                        result.CriDamage +=reinforceIntensity;    
                        result.MaxHP +=reinforceIntensity*5;
                        result.MaxMP +=reinforceIntensity*5;
                        break;

                }
                if(UnityEngine.Random.Range(0,9)<=6)
                {
                    instance.ExtraCriChance +=result.CriChance;
                    instance.ExtraCriDamage +=result.CriDamage;
                    instance.ExtraDamage +=result.Damage;
                    instance.ExtraDefense +=result.Defense;
                    instance.ExtraHealth +=result.MaxHP;
                    instance.ExtraMana +=result.MaxMP;
                    result.bReinforceResult = true;
                }
                else 
                    result = new ReinforceResult();
              
                ++instance.CurrentReinforce;
                Managers.Event.Publish<Event_Reinforce>(new Event_Reinforce(itemInfo));
                _resultDisplayer.SetResult(result);

                _btnReinforce.interactable = false;
                _co = CoroutineRunner.Instance.StartCoroutine(ReinforceEffect());
            }
        });

    }
    IEnumerator ReinforceEffect()
    {

        CanvasGroup canvasG =  _resultDisplayer.GetComponentInChildren<CanvasGroup>();
        float duration = 0.5f; // 0.5초 동안
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasG.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null; // 다음 프레임까지 대기
        }
        _btnReinforce.interactable = true;
        canvasG.alpha = 1f; // 마지막 보정
        _co = null;
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        return;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        return;
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        if (null == eventData.pointerDrag) return;

        if (eventData.pointerDrag.transform.name !="ItemIcon_Prefab") return;

        GameObject itemGO = Managers.Pool.LendPoolableTo("ItemIcon_Prefab", null).gameObject;
        itemGO.GetComponentInChildren<ItemDataStorage>().UpdateItemData(eventData.pointerDrag.GetComponentInChildren<ItemDataStorage>());
        itemGO.GetOrAddComponent<UI_Draggable_Move>();

        GetBackToOrigin(eventData.pointerDrag);

         ItemDataStorage itemDataStorage = itemGO?.GetComponentInChildren<ItemDataStorage>();
        ItemData itemData = itemDataStorage?.GetItemData();
        if (null ==itemGO) return;
        if (null == itemDataStorage) return;
        if (null == itemData) return;  // 아이템이 비어있으면 안됨
        if (itemData.Type != eITEMTYPE.EQUIPMENT) return; // 장비 아이템이 아니면 안됨

        
      
        UI_Base slotOrigin_UI_Base = _slotOrigin.GetComponent<UI_Base>(); //Slot (ItemSlot)
        if (null == slotOrigin_UI_Base) return;





        ItemDataStorage StorageFromTarget = itemGO.GetComponentInChildren<ItemDataStorage>();
        ItemDataStorage storageFromOrigin = _slotOrigin.GetComponentInChildren<ItemDataStorage>(); //dnjsfo dl
        UI_Draggable_Move uiDraggableFromTarget = StorageFromTarget.gameObject.GetComponent<UI_Draggable_Move>(); // 이제 장착할 무기
        UI_Draggable_Move uiDraggableFromOrigin = storageFromOrigin?.gameObject.GetComponent<UI_Draggable_Move>();  //이미 장착되어있던 무기 


        //1. 교환은 필요없다. 
        // 2.현재 장착되 아이템이 있는지 확인 
        //3, 장착된 아이템이 있다면 원래 자리로 돌려보내기
        //4. 내 슬롯에 아이템 착용하기 

        if (HasSameItem(StorageFromTarget))
        {
            // 장착 하려는 아이템이 같은 아이템인지 확인필요(객체 아이디로 비교 ), 같은 어이템이하면  원래 부모로 되돌리기 
            UI_Draggable_Move _draggableMove = itemGO.GetComponent<UI_Draggable_Move>();

            _draggableMove.transform.SetParent(_draggableMove.OriginTransform);
            RectTransform rectex = _draggableMove.OriginTransform.GetComponentInChildren<RectTransform>();
            _draggableMove.OriginTransform.gameObject.GetComponentInChildren<UI_Base>().FixDropItem(rectex);
            return;

        }

        if (storageFromOrigin)
        {
            Managers.Pool.GetBack(storageFromOrigin.GetComponentInChildren<ItemDataStorage>().GetComponent<Poolable>());
        }
        if (StorageFromTarget)
        {
         
            /*Drop Event 추가 */
            Action backToInven = () =>
            {
               
                Managers.Pool.GetBack(itemGO.GetComponent<Poolable>());
                _nowIDS = null; 
            };
            uiDraggableFromTarget.DropEvent = null;
            uiDraggableFromTarget.DropEvent += backToInven;
        }
        /*1. 비우고*/
        slotOrigin_UI_Base?.EmptySlot();
        /*2. 아이콘 자식으로 만들고*/
        uiDraggableFromTarget.IsPrevOriginal=false;
        uiDraggableFromTarget.PreviousTransform=_slotOrigin.transform;
        itemGO.transform.SetParent(_slotOrigin.transform);
        itemGO.transform.SetAsLastSibling();
        /*3.모양 세팅하고*/
        RectTransform rect = itemGO.GetComponentInChildren<RectTransform>();
        slotOrigin_UI_Base?.FixDropItem(rect); // 슬롯 사이즈에 맞게 재조정 . 
        
        _nowIDS =  StorageFromTarget;
        
        return;
    }
    public override bool HasSameItem(ItemDataStorage dataStorage)
    {

  
        ItemDataStorage oth = _slotOrigin.GetComponentInChildren<ItemDataStorage>();
        if (null ==oth) return false ;
        if (dataStorage.ItemInfo ==oth.ItemInfo) return true;
        return false;
    }
    public void Clear()
    {
        ItemDataStorage storageFromOrigin = _slotOrigin.GetComponentInChildren<ItemDataStorage>();
        if (storageFromOrigin)
        {
            Managers.Pool.GetBack(storageFromOrigin.GetComponentInChildren<ItemDataStorage>().GetComponent<Poolable>());
            _nowIDS =null;
        }
        _resultDisplayer.ClearTexts();
    }

    public void  GetBackToOrigin(GameObject itemGO)
    {
        UI_Draggable_Move _draggableMove = itemGO.GetComponent<UI_Draggable_Move>();

        _draggableMove.transform.SetParent(_draggableMove.OriginTransform);
        RectTransform rectex = _draggableMove.OriginTransform.GetComponentInChildren<RectTransform>();
        _draggableMove.OriginTransform.gameObject.GetComponentInChildren<UI_Base>().FixDropItem(rectex);

    }
    public void OnClose()
    {
        Clear();
        Managers.UI.ClosePopupUI(_handle);
    }
}
