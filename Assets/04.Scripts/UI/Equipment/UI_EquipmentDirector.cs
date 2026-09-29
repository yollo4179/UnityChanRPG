using System.ComponentModel.Design.Serialization;
using Unity.AppUI.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;
using JetBrains.Annotations;
using Player;
public class UI_EquipmentDirector : UI_Base, IPointerEnterHandler, IDropHandler, IPointerExitHandler//
{   public enum TMPs
    {
        AttackV_TMP,
        CriticalChanceV_TMP,
        CriticalDamageV_TMP,
        DefenseV_TMP
    }

    public enum Buttons
    {
        Close_Button,
        END,
    }

    public enum GameObjects
    {
        Slot_Weapon_GO,
        Slot_Armor_GO,
        Slot_Head_GO,
        Slot_Shoes_GO,
        Slot_Gem_GO,
        Slot_Gloves_GO,
        Slot_End,
    }
    public enum eStatus
    {
        Attack,
        CriticalChance,
        CriticalDamage,
        Defense,
        END,
    }
    int numEquippedItems = 0; 
    UI_Popup _handler;  
    UI_EquipmentSlot[] _equipmentSlots = new UI_EquipmentSlot[(int)GameObjects.Slot_End];
    int[] _cachedAblity = new int[(int)eStatus.END]; 
    eEQUIPMENTTYPE[] _equipmentTypes = new eEQUIPMENTTYPE[]
    {
        eEQUIPMENTTYPE.WEAPON,
        eEQUIPMENTTYPE.ARMOR,
        eEQUIPMENTTYPE.HEAD,
        eEQUIPMENTTYPE.SHOES,
        eEQUIPMENTTYPE.GEM,
        eEQUIPMENTTYPE.GLOVES,
    };

    TextMeshProUGUI[] _text= new TextMeshProUGUI[(int)eStatus.END];
    
    public override void Init()
    {
        _handler = transform.root.GetComponent<UI_Popup>();

        Bind<Button>(typeof(Buttons));  
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(TMPs));
       
        for(int i=0;i<(int)eStatus.END;++i)
        {
            _text[i] = Get<TextMeshProUGUI>(i);
        }

        GetButton((int)Buttons.Close_Button).onClick.AddListener(() => 
        {
            Managers.UI.ClosePopupUI(_handler);
        });

        for(int i =0; i < (int)GameObjects.Slot_End; ++i) //슬롯 개수 만큼 컴포넌트 추가하고 드롭 가능하게 만들기
        {
            var go = _objects[typeof(GameObject)][i];
            _equipmentSlots[i] = go.GetComponent<UI_EquipmentSlot>();
            _equipmentSlots[i].SetEquipmentType(_equipmentTypes[i]);
        }

        UpdateExtraAbilities();
        Managers.Event.Subscribe<Event_LevelUP>(UpdateStatus);

    }
    public void Awake()
    {
        Init();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        return;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        return;
    }
    //일단 화면 (root ) 전체에 올려놓기만 해도 알맞는 장비 타입을 찾아가도록 구현
    public void OnDrop(PointerEventData eventData)
    {
        if(null == eventData.pointerDrag) return;

        if (eventData.pointerDrag.transform.name !="ItemIcon_Prefab") return; 

        GameObject itemGO = Managers.Pool.LendPoolableTo("ItemIcon_Prefab", null).gameObject;
        itemGO.GetComponentInChildren<ItemDataStorage>().UpdateItemData(eventData.pointerDrag.GetComponentInChildren<ItemDataStorage>());
        itemGO.GetOrAddComponent<UI_Draggable_Move>();

        ItemDataStorage itemDataStorage = itemGO?.GetComponentInChildren<ItemDataStorage>();
        ItemData itemData = itemDataStorage?.GetItemData(); 
        if (null ==itemGO) return;
        if(null == itemDataStorage) return;
        if (null == itemData) return;  // 아이템이 비어있으면 안됨
        if (itemData.Type != eITEMTYPE.EQUIPMENT) return; // 장비 아이템이 아니면 안됨

        GameObject slotOrigin =null;
        //슬롯 조사
        for(int i=0;i< _objects[typeof(GameObject)].Length;++i)
        {
            if (itemData.EquipmentType !=  _equipmentSlots[i].GetEquipmentType()) continue;  // 장비 타입이 맞지 않으면 패스
            // 장비 타입이 맞으나 이미 장착된 상태면 교체 ,장착된 상태가 아니면 착용 
            slotOrigin = (_objects[typeof(GameObject)][i] as GameObject);
            slotOrigin.GetComponent<UI_EquipmentSlot>().UnEqup();
            slotOrigin.GetComponent<UI_EquipmentSlot>().Equip(itemData);

            break; 
        }
        if (null ==slotOrigin) return;
        UI_Base slotOrigin_UI_Base = slotOrigin.GetComponent<UI_Base>();
        if (null == slotOrigin_UI_Base) return; 

       


       
       ItemDataStorage StorageFromTarget = itemGO.GetComponentInChildren<ItemDataStorage>();
       ItemDataStorage storageFromOrigin= slotOrigin.GetComponentInChildren<ItemDataStorage>(); //dnjsfo dl
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
            //우너래 슬롯에 있던거 제거 

            //Managers.Equipment.TryMoveToInven(storageFromOrigin.ItemInfo);

            Managers.Pool.GetBack(storageFromOrigin.GetComponentInChildren<ItemDataStorage>().GetComponent<Poolable>());
            Managers.Equipment.TryRemoveEquipment(storageFromOrigin.ItemInfo);
            Managers.Inventory.TryAddItem(storageFromOrigin.ItemInfo);
            if(storageFromOrigin)
            (storageFromOrigin.ItemInfo as InstanceItemInfo).IsEquipped = false;

        }
        if (StorageFromTarget)
        {
            // Managers.Inventory.TryMoveToEquipment(StorageFromTarget.ItemInfo);
            Managers.Equipment.TryAddEquipment(StorageFromTarget.ItemInfo);
            Managers.Inventory.TryRemoveItem(StorageFromTarget.ItemInfo,1);
            if(StorageFromTarget)
            (StorageFromTarget.ItemInfo as InstanceItemInfo).IsEquipped = true;

            

            /*Drop Event 추가 */
            Action backToInven = () => 
            {
                Managers.Equipment.TryRemoveEquipment(StorageFromTarget.ItemInfo);
                Managers.Equipment.EquipBaseWeapon();

                Managers.Inventory.TryAddItem(StorageFromTarget.ItemInfo);
                (StorageFromTarget.ItemInfo as InstanceItemInfo).IsEquipped = false;
                Managers.Pool.GetBack(itemGO.GetComponent<Poolable>());

                UpdateExtraAbilities();

                Event_EquipItem evt = new Event_EquipItem(CheckNumEquippedItems());
                Managers.Event.Publish<Event_EquipItem>(evt);
                //Managers.Equipment.TryMoveToInven(StorageFromTarget.ItemInfo);
            };
            uiDraggableFromTarget.DropEvent = null;
            uiDraggableFromTarget.DropEvent += backToInven;
        }
        /*1. 비우고*/
        slotOrigin_UI_Base?.EmptySlot(); 
        /*2. 아이콘 자식으로 만들고*/
        uiDraggableFromTarget.IsPrevOriginal=false;
        uiDraggableFromTarget.PreviousTransform=slotOrigin.transform;
        itemGO.transform.SetParent(slotOrigin.transform);
        itemGO.transform.SetAsLastSibling();
       /*3.모양 세팅하고*/
       RectTransform rect = itemGO.GetComponentInChildren<RectTransform>();
        slotOrigin_UI_Base?.FixDropItem(rect); // 슬롯 사이즈에 맞게 재조정 . 
        UpdateExtraAbilities();


        Event_EquipItem evt = new Event_EquipItem(CheckNumEquippedItems());
        Managers.Event.Publish<Event_EquipItem>(evt);
        return; 
    }
    public void UpdateStatus (Event_LevelUP evt)
    {
        UpdateExtraAbilities();

        PlayerInfo playerInfo = Managers.Player.PlayerInfo;

        int damage = (int)playerInfo.Attack;
        int defense = (int)playerInfo.Defense;
        int criDamage = (int)playerInfo.CriDemage;
        int criChance = (int)playerInfo.CriChance;

        _text[(int)eStatus.Attack].text         =   $"{damage}+({_cachedAblity[(int)eStatus.Defense]})";
        _text[(int)eStatus.Defense].text        =   $"{defense}+({_cachedAblity[(int)eStatus.Attack] })";
        _text[(int)eStatus.CriticalDamage].text =   $"{criDamage}+({_cachedAblity[(int)eStatus.CriticalChance]})";
        _text[(int)eStatus.CriticalChance].text =   $"{criChance}+({_cachedAblity[(int)eStatus.CriticalDamage]})";
    }
    public void UpdateExtraAbilities() //status 바뀌면 이벤트 호출 //Subscribe
    {
     
        //이전 무기 영향 제거 
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        StatusScript status = playerGO.GetComponent<StatusScript>();
        status.Defense -= _cachedAblity[(int)eStatus.Defense];
        status.AttackDamage -= _cachedAblity[(int)eStatus.Attack];
        status.CriChance -= _cachedAblity[(int)eStatus.CriticalChance];
        status.CriDamage -= _cachedAblity[(int)eStatus.CriticalDamage];


        PlayerInfo playerInfo =  Managers.Player.PlayerInfo;

       int damage     = (int)playerInfo.Attack;
       int defense    = (int)playerInfo.Defense;
       int criDamage  = (int)playerInfo.CriDemage;
       int criChance  = (int)playerInfo.CriChance;


        /*Extra Ability*/
        int exDamage    = 0;
        int exDefense   = 0;
        int exCriDamage = 0;
        int exCriChance = 0;
        
        for(int i = 0 ; i< _equipmentTypes.Length; ++i)
        {
          ItemDataStorage storage = _equipmentSlots[i].GetComponentInChildren<ItemDataStorage>() ;
          if (null ==storage) continue;
            ItemInfo itemInfo = storage.ItemInfo;
          if (null ==itemInfo) continue;
          InstanceItemInfo instanceItemInfo_Reinforce = (itemInfo as InstanceItemInfo);
          if (null == instanceItemInfo_Reinforce) continue;

           ItemData itemData =  storage.GetItemData(); 
           exDamage    += instanceItemInfo_Reinforce.ExtraDamage +itemData.ExtraAttack;
           exDefense   += instanceItemInfo_Reinforce.ExtraDefense + itemData.ExtraDefense;
           exCriChance += instanceItemInfo_Reinforce.ExtraCriChance +itemData.ExtraCriChance;
           exCriDamage += instanceItemInfo_Reinforce.ExtraCriDamage +itemData.ExtraCriDemage;
        }
        _cachedAblity[(int)eStatus.Attack]  = exDamage;
        _cachedAblity[(int)eStatus.Defense]  = exDefense;
        _cachedAblity[(int)eStatus.CriticalDamage]  = exCriChance;
        _cachedAblity[(int)eStatus.CriticalChance]  = exCriDamage;

       

        /*무기 영향 갱신*/
        _text[(int)eStatus.Attack].text         =   $"{damage}+({exDamage})";
        _text[(int)eStatus.Defense].text        =   $"{defense}+({exDefense})";
        _text[(int)eStatus.CriticalDamage].text =   $"{criDamage}+({exCriDamage})";
        _text[(int)eStatus.CriticalChance].text =   $"{criChance}+({exCriChance})";
        /*무기 영향 갱신*/
        status.Defense      += _cachedAblity[(int)eStatus.Defense];
        status.AttackDamage += _cachedAblity[(int)eStatus.Attack];
        status.CriChance    += _cachedAblity[(int)eStatus.CriticalChance];
        status.CriDamage    += _cachedAblity[(int)eStatus.CriticalDamage];


    }
    public int CheckNumEquippedItems()
    {
        int numEquippedItems = 0;
        for (int i = 0; i<_equipmentSlots.Length; ++i)
        {

            ItemDataStorage oth = _objects[typeof(GameObject)][i].GetComponentInChildren<ItemDataStorage>();
            if (null ==oth) continue;
            ++numEquippedItems;
        }
        return numEquippedItems;
    }
    public override bool HasSameItem(ItemDataStorage dataStorage)
    {
     
        for(int i=0;i<_equipmentSlots.Length;++i)
        {
           
            ItemDataStorage oth = _objects[typeof(GameObject)][i].GetComponentInChildren<ItemDataStorage>();
            if (null ==oth) continue;
            if (dataStorage.ItemInfo ==oth.ItemInfo)return true;

        }
        return false;
    }

}
