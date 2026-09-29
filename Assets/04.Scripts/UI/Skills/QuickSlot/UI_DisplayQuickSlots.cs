using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UI_DisplayQuickSlots : UI_Scene
{
    public enum GameObjects
    {
        Contents_GO
    }

    
    [SerializeField] int _numSlots = 6;
    //2개 돌려서 아이템과 스킬을 모두 사용할 수있도록 한다.
    Dictionary<int, int > _dicSkillIdxToSlotIdx;
    Dictionary<ItemInfo, int> _dicItemInfoToSlotIdx;

    List<UI_QuickSlot> _quickSlotList;

    public void AddKey(int skillIdx,int slotKey)
    {
        if(false==_dicSkillIdxToSlotIdx.ContainsKey(skillIdx))
            _dicSkillIdxToSlotIdx.Add(skillIdx, slotKey);
    }


    public void RemoveKey(int skillIdx) {
        

        if (true == _dicSkillIdxToSlotIdx.ContainsKey(skillIdx))
            _dicSkillIdxToSlotIdx.Remove(skillIdx);
    }
    public bool HasKey(int skillIdx)
    {
        return _dicSkillIdxToSlotIdx.ContainsKey(skillIdx);
    }
    public UI_QuickSlot GetQuickSlotByHandle(int handle)
    {
        if(_dicSkillIdxToSlotIdx.ContainsKey(handle)) return _quickSlotList[_dicSkillIdxToSlotIdx[handle]-1];
        return null; 
    }

    public void AddKey(ItemInfo itemInfo, int slotKey)
    {
        if (false==_dicItemInfoToSlotIdx.ContainsKey(itemInfo))
            _dicItemInfoToSlotIdx.Add(itemInfo, slotKey);
    }


    public void RemoveKey(ItemInfo itemInfo)
    {
        if (true == _dicItemInfoToSlotIdx.ContainsKey(itemInfo))
            _dicItemInfoToSlotIdx.Remove(itemInfo);
    }
    public bool HasKey(ItemInfo itemInfo)
    {
        return _dicItemInfoToSlotIdx.ContainsKey(itemInfo);
    }
    public UI_QuickSlot GetQuickSlotByHandle(ItemInfo itemInfo)
    {
        if (_dicItemInfoToSlotIdx.ContainsKey(itemInfo)) return _quickSlotList[_dicItemInfoToSlotIdx[itemInfo]-1];
        return null;
    }



    /*외부 호출용*/

    public UI_QuickSlot GetQuickSlotByNumber(int slotNO)
    {
        Debug.Assert(1<=slotNO &&_numSlots >= slotNO);

        if (1<=slotNO &&_numSlots >= slotNO)
            return _quickSlotList[slotNO-1];  
        return null;
    }
    public SkillSO GetSkillInfoBySlotNO(int slotNO)
    {
        var slot= GetQuickSlotByNumber(slotNO);
        if (null ==slot) return null;

        var itemInfo = slot.GetComponentInChildren<ItemDataStorage>(); //현재 슬롯이 차있는 상태 

        if (null == itemInfo) return null; 

        return itemInfo.GetSkillSO();  
    }
    //public ItemData GetItemDataBySlotNO(int slotNO)
    //{
    //    var slot = GetQuickSlotByNumber(slotNO);
    //    if (null == slot) return null;

    //    var itemInfo = slot.GetComponentInChildren<ItemDataStorage>(); //현재 슬롯이 차있는 상태 

    //    if (null == itemInfo) return null;

    //    return itemInfo.GetItemData();
    //}
    public ItemInfo GetItemInfoBySlotNo(int slotNO)
    {
        var slot = GetQuickSlotByNumber(slotNO);
        if (null == slot) return null;

        var itemDataStorage = slot.GetComponentInChildren<ItemDataStorage>();
        if (null == itemDataStorage) return null;

        return itemDataStorage.ItemInfo;
    }
    public void Start()
    {

        _quickSlotList=new List<UI_QuickSlot>();
        _dicSkillIdxToSlotIdx=new Dictionary<int, int>();
        _dicItemInfoToSlotIdx =new Dictionary<ItemInfo, int>();
        Bind<GameObject>(typeof(GameObjects));
            
        for(int i=0;i<_numSlots;++i)
        {
            GameObject parent = (_objects[typeof(GameObject)][(int)GameObjects.Contents_GO] as GameObject);
            GameObject slotGO= Managers.Resource.Instantiate("UI/SceneUI/QuickSlot/QuickSlot_Prefab", parent.transform);
            slotGO.AddComponent<UI_Droppable>();

            slotGO.GetComponent<UI_QuickSlot>().SetSlotNO(i+1);

            _quickSlotList.Add(slotGO.GetComponent<UI_QuickSlot>());
        }
    }
}
