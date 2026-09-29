using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class UI_SkillSlot : UI_Popup
{
    /*Components*/
    public enum Buttons
    {
        UpgradeButton_Button
    }
    public enum TMPs
    {
        SkillName_TMP,
        SkillType_TMP,
        //SkillLevel_TMP
    }
    public enum Images
    {
        //SkillIcon_Image
    }
   public enum GameObjects
    {
       // ItemImage_Prefab,
        SkillSlot_Pannel_GO
    }

    SkillSO _skillSO;
    GameObject _itemIcon;//Copyable
    GameObject[] _itemIconDraggable= new GameObject[2]; 
    public void Injectinfo(SkillSO skillSO)
    {
        _skillSO = skillSO;
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        //Bind<Image>(typeof(Images));
        Bind<GameObject> (typeof(GameObjects));


        (_objects[typeof(TextMeshProUGUI)][(int)TMPs.SkillName_TMP] as TextMeshProUGUI).text= _skillSO.SkillName;
        (_objects[typeof(TextMeshProUGUI)][(int)TMPs.SkillType_TMP] as TextMeshProUGUI).text= _skillSO.SkillClass;

      
        //이벤트 등록 
        (_objects[typeof(Button)][(int)Buttons.UpgradeButton_Button] as Button).onClick.RemoveAllListeners();
        (_objects[typeof(Button)][(int)Buttons.UpgradeButton_Button] as Button).onClick.AddListener(AddSkillLevel);

        //이미지,스킬레벨 등록 
      
        GameObject go = (_objects[typeof(GameObject)][(int)GameObjects.SkillSlot_Pannel_GO] as GameObject);


        AddIcon(out _itemIcon,go.transform);

        for (int i = 0; i<2; ++i)
        {
            AddIcon(out _itemIconDraggable[i], go.transform);//draggable
            _itemIconDraggable[i].AddComponent<UI_Draggable_Move>().SetOriginTransform(go.transform);
            

        }
    }
    public void AddIcon(out GameObject icon,Transform parent)
    {
        icon =(Managers.Resource.Instantiate("UI/Part/ItemIcon_Prefab", null)); //미니 스롯 생성
        var rect = icon.GetComponent<RectTransform>();

        // 1) 부모 붙일 때 로컬 기준 보존

        rect.SetParent(parent, worldPositionStays: false);
        icon.transform.SetAsLastSibling();
        FixItem(rect, new Vector2(95, 95));

        int skillLevel = Managers.Player.GetSkillInfoByHandle(_skillSO.handle).Level;
        _skillSO.SkillLevel =skillLevel;
        icon.GetComponentInChildren<Image>().sprite = _skillSO.SkillIcon;
        icon.GetComponentInChildren<TextMeshProUGUI>().text = skillLevel.ToString();
        icon.GetComponent<ItemDataStorage>().UpdateSkillSOData(_skillSO);
        icon.GetComponent<ItemDataStorage>().Init();
        if (0 >= skillLevel)
            icon.GetComponentInChildren<Image>().color*=0.5f;

    }

    public override void FixDropItem(in RectTransform rect)
    {
        FixItem(rect, new Vector2(95, 95));
    }
    
    private void UpdateInfo()
    {
        Debug.Assert(null != _skillSO);//barriar
        Debug.Assert(null != _itemIcon);
        
        if (0<_skillSO.SkillLevel)
        {
            Image img = _itemIcon.GetComponentInChildren<Image>();
            img.color = UnityEngine.Color.white; 
        }
        _itemIcon.GetComponentInChildren<TextMeshProUGUI>().text= _skillSO.SkillLevel.ToString();


        /*JsonDataUpdate*/
        PlayerSkillInfo skillInfo = Managers.Player.GetSkillInfoByHandle(_skillSO.handle);
        skillInfo.Level = _skillSO.SkillLevel;
        Managers.Player.UpdateSkillInfo(skillInfo);

        Managers.Event.Publish<Event_UseSkillPoint>(new Event_UseSkillPoint(_skillSO.handle));
        //level 비교후에 enable
    }
   
    public void AddSkillLevel()
    {
        ++_skillSO.SkillLevel;
        UpdateInfo();
    }

}
