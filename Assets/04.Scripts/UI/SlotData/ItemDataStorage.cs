using NUnit.Framework.Interfaces;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.VersionControl;
#endif
using UnityEngine;
using UnityEngine.UI;
public class ItemDataStorage : MonoBehaviour  //나도 Subscribe 해야한다.
{
    eITEMTYPE _itemType;
    /*Skill*/
    SkillSO _skillSO;
    /*Item*/
    ItemData _itemData;
    ItemInfo _itemInfo; public ItemInfo ItemInfo{ get=> _itemInfo; }
    bool _hasRegisteredEvent = false; 

    bool _removable = false;


    public void Init()
    {
        if (true ==_hasRegisteredEvent) return;
        
        _hasRegisteredEvent  = true; 
        Managers.Event.Subscribe<Event_UseItem>(OnItemChanged);
        Managers.Event.Subscribe<Event_UseSkillPoint>(OnSkillChanged);
        Managers.Event.Subscribe<Event_Reinforce>(OnEquipmentReinforced);
    }
    private void OnSkillChanged(Event_UseSkillPoint evt)
    {
        if (_skillSO == null) return;
        if (evt._skillID != _skillSO.handle) return;

        PlayerSkillInfo skillInfo= Managers.Player.GetSkillInfoByHandle(_skillSO.handle);
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = skillInfo.Level.ToString();
        
    }
    private void OnEquipmentReinforced (Event_Reinforce evt)
    {
        if (null == evt.ItemInfo) return;
        ItemData itemData = Managers.Data.GetItemData(evt.ItemInfo.ID);
        if(null == itemData) return;
        if (eITEMTYPE.EQUIPMENT != itemData.Type) return;

        InstanceItemInfo instance = (_itemInfo as InstanceItemInfo);
        if (null ==instance) return; 
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (0 < instance.CurrentReinforce)
            text.text = "+" + instance.CurrentReinforce.ToString();
        else
            text.text ="";


    }
    private void OnItemChanged(Event_UseItem evt)// 스킬이랑 아이템이랑 번호 겹치면 안된다. 
    {
        if (evt._itemInfo != _itemInfo) return;

       TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
       if(1 < _itemInfo.Amount)
       text.text = _itemInfo.Amount.ToString();
       else 
       text.text ="";

       if(_itemInfo.Amount <=0)
       {
            UI_Draggable_Move draggableItem = GetComponent<UI_Draggable_Move>();
            if (null==draggableItem) return;

            if (null != draggableItem.OriginTransform) return;     
            Managers.Resource.Destroy(this.gameObject);// 이면 나 지워// 프레임까지 지우지는말고 
        }
        
    }
    public eITEMTYPE GetItemType()
    {
        return _itemType;
    }
    public void UpdateSkillSOData(SkillSO skillSO)
    {
        _itemType = eITEMTYPE.SKILL;
        _skillSO =skillSO;

        GameObject icon = gameObject;
        Image img = icon.GetComponentInChildren<Image>();
        TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();
        img.sprite=skillSO.SkillIcon;
        text.text= skillSO.SkillLevel.ToString();
        _itemData =null;
    }
    public void UpdateItemData(ItemDataStorage itemDataStorage) 
    {
        _itemType = itemDataStorage._itemData.Type;
        _itemData = itemDataStorage._itemData;
        _itemInfo = itemDataStorage._itemInfo;
        Image img = gameObject.GetComponentInChildren<Image>();
  

        img.sprite = itemDataStorage.GetComponentInChildren<Image>().sprite;
        ChangeText(_itemData);
        _skillSO =null;
    }
    public void UpdateItemData(ItemData itemData,Sprite sprite , ItemInfo itemInfo)
    {
        _itemType = itemData.Type;
        _itemData = itemData;
        _itemInfo = itemInfo;

        Image img = gameObject.GetComponentInChildren<Image>();
        

        img.sprite = sprite;
        ChangeText(itemData);
        _skillSO =null; 
    }

    public void ChangeText(ItemData itemData)
    {
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        InstanceItemInfo instance = (_itemInfo as InstanceItemInfo);
        if (eITEMTYPE.EQUIPMENT == itemData.Type &&  null != instance)
        {
            text.color = Color.magenta;
            if (0 < instance.CurrentReinforce)
            {
                text.text = "+" +instance.CurrentReinforce.ToString();
            }
            else
            {
                text.text = "";
            }

        }
        else
        {
            text.color = Color.white;
            if (1 < _itemInfo.Amount)
                text.text = _itemInfo.Amount.ToString();
            else
                text.text = "";
        }

    }
   
    public SkillSO GetSkillSO() {
        return _skillSO;
    }
    public ItemData GetItemData()
    {
        return _itemData;
    }
}
