using System;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Unity.VisualScripting;
public class InventoryManager
{
    /*인벤토리 인포*/
    private InventoryInfo _invenInfo;
    public InventoryInfo InvenInfo { get => _invenInfo; set => _invenInfo=value; } //처음 로그인할때 채워줌
   // public Dictionary<int, int> _dicItemID2Idx = new Dictionary<int, int>(); //판매 삭제 시 주의

    

    /*Debug*/
    static private string _invenInfoPath;
    static public string InvenInfoPath { get => _invenInfoPath; }
    public void Init()
    {
        _invenInfoPath = Application.persistentDataPath + "/InvenInfo.json";
        InitInventoryInfo();
    }

    public int AmountItem(int itemID)
    {
        int amount = 0;
        List<ItemInfo> listItemInfo = _invenInfo.InfoList.FindAll(x => x.ID ==itemID);
        foreach(var item in listItemInfo)
        {
            amount +=item.Amount;
        }
        return amount;
    }
    public void RefreshInventory()
    {
        InvenGridScrollView invenGridScrollView = Managers.UI.GetCachedUIByName("InventoryPannel_Canvas_Prefab")
            .GetComponentInChildren<InvenGridScrollView>();
        invenGridScrollView.Refresh();


    }
    public bool TryMoveToEquipment(ItemInfo itemInfo)
    {
        if (!InvenInfo.InfoList.Contains(itemInfo)) return false;
        if( eITEMTYPE.EQUIPMENT != itemInfo.Type) return false; 
        InstanceItemInfo instanceItemInfo = (itemInfo as InstanceItemInfo);
        if (null ==instanceItemInfo) return false;
        if (Managers.Equipment.GetEquipmentItemList().Contains(itemInfo))return false ;
        
        InvenInfo.InfoList.Remove(itemInfo);
        instanceItemInfo.IsEquipped =true;
        Managers.Equipment.TryAddEquipment(itemInfo );


        RefreshInventory();
        return true;

    }
    public void InitInventoryInfo()
    {
        /*신규 유저 인가요?*/
        string szInvenInfoPath = InvenInfoPath;
        Debug.LogFormat("<color=#22ff00> 파일 위치{0}</color>", szInvenInfoPath);
        InventoryInfo invenInfo = null;
        if (!File.Exists(szInvenInfoPath))
        {
            /*신규유저 입니다.*/
            /*To do*/
            /*1. 파일 생성*/
            Debug.Log("<color=yellow>신규 유저입니다.</color>");
            invenInfo = new InventoryInfo();
            invenInfo.Init();
            string json = JsonConvert.SerializeObject(invenInfo);
            Debug.Log(json);
            File.WriteAllText(szInvenInfoPath, json);
        }
        else
        {
            /*신규 유저가 아닙니다.*/
            Debug.Log("<color=yellow>기존 유저입니다.</color>");
            /*To do*/
            /*직렬화*/
            string json = File.ReadAllText(szInvenInfoPath);
            invenInfo = JsonConvert.DeserializeObject<InventoryInfo>(json);
            Debug.Log(json);
            Debug.Log("<color=yellow>Deserialization Completed </color>");
        }
        /*인벤토리 인포 초기화*/
        InvenInfo = invenInfo;
        Debug.LogFormat("InfoManager.m_Instance.m_InvenInfo{0}", InvenInfo);
        Debug.LogFormat("<color=##ff0000>InfoManager.m_Instance.m_InvenInfo.InfoList.Count{0}</color>", InvenInfo.InfoList.Count);

        //for (int idx = 0; idx<InvenInfo.InfoList.Count; ++idx)
        //{
        //    _dicItemID2Idx.Add(InvenInfo.InfoList[idx].ID, idx);
        //}//아이템 삭제시 주의

    }
    public void SaveInventoryInfo()
    {
        /*저장한다 내 데이터*/
        /*Serialize*/

        try
        {
            var Json = JsonConvert.SerializeObject(_invenInfo);
            Debug.Log(Json);
            File.WriteAllText(_invenInfoPath, Json);
            Debug.Log($"<color=$00ff00> 저장 완료 </color>");
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red>저장 실패 {e.Message}</color>");
        }
        finally
        {

        }

    }
    public void GetInvenInfoItemAmount(int ItemID, out int Amount)
    {
        Amount = -1;
        if (null == Managers.Data.GetItemData(ItemID)) return;


        ItemInfo FoundItem = InvenInfo.InfoList.Find((x) => ItemID ==x.ID);
        if (null != FoundItem)
            Amount = FoundItem.Amount;
    }
    public void TryAddItem(ItemInfo itemInfo)
    {
        if (InvenInfo.InfoList.Contains(itemInfo)) return;
        InvenInfo.InfoList.Add(itemInfo);
        RefreshInventory();
    }
    public void TryAddItem(eITEMTYPE itemType, int itemID, int amount)
    {
        switch (itemType){
            case eITEMTYPE.EQUIPMENT:
                {
                    for (int i = 0; i<amount; ++i)
                    {
                        ItemData itemData = Managers.Data.GetItemData(itemID);
                        InstanceItemInfo newItem = new InstanceItemInfo();
                        newItem.SetInfo(itemID, itemType, 1);
                        newItem.SetInstanceInfo($"i-{Guid.NewGuid():N}", itemData.Level, itemData.Rarity, itemData.MaxReinforce);
                        InvenInfo.InfoList.Add(newItem);
                    }
                    break;
                }
            default: 
            {
               ItemInfo itemInfo= InvenInfo.InfoList.FirstOrDefault((item) => { return item.ID ==itemID; });

                    if (null ==itemInfo)//처음 보는 아이템
                    {
                        ItemInfo newItem = new ItemInfo();
                        newItem.SetInfo(itemID, itemType, amount);
                        InvenInfo.InfoList.Add(newItem);
                    }
                    else
                    {
                        Debug.Log($"<color=#00ff00>새로운 아이템 추가 {Managers.Data.GetItemData(itemID).ItemName} </color>");
                        itemInfo.Amount += amount;
                    }
                    break;
            }
        }

        RefreshInventory();
    }
    public ItemInfo TryRemoveItem(ItemInfo itemInfo,int amount)
    {
        if (eITEMTYPE.EQUIPMENT==itemInfo.Type)
        {
            InvenInfo.InfoList.Remove(itemInfo); 
         
        }
        else
        {
            itemInfo.Amount-=amount;

            if (0 >= itemInfo.Amount ) {
                InvenInfo.InfoList.Remove(itemInfo);
            }
        }
      
        RefreshInventory();

        return itemInfo;
    }
    //public int TryRemoveItem(eITEMTYPE itemType, int itemID, int amount)
    //{
        

    //    ItemInfo itemInfo = InvenInfo.InfoList.FirstOrDefault((item) => { return item.ID ==itemID; });
    //    if (null == itemInfo) return 0;

    //    int result = amount;
        
    //    result = itemInfo.Amount -= amount;

    //    if (0 > result)
    //    {
    //        result = amount + result;
    //    }
    //    else if (0 == result)
    //    {
    //        result = amount;
    //    }
    //    else
    //    {
    //        result= amount;
    //        return result;
    //    }
    //    /*정보 다시 변경*/
    //    InvenInfo.InfoList.Remove(itemInfo); 
        
    //    //for (int i = 0; i<InvenInfo.InfoList.Count; ++i)
    //    //{
    //    //    _dicItemID2Idx[InvenInfo.InfoList[i].ID] = i;
    //    //}
    //    return result;
    //}
    public void SaveInfo()
    {

        try
        {
            string json = JsonConvert.SerializeObject(_invenInfo,Formatting.Indented);
            File.WriteAllText(_invenInfoPath, json);
            Debug.Log($"<color=#00ff00>저장 완료 : {_invenInfoPath} 내용{json}</color>");

        }
        catch (Exception e)
        {
            Debug.Log($"<color=#ff0000>{e} </color>");
        }
        finally { }
    }

    public InventoryInfo LoadInfo()
    {
        try
        {
            var json = File.ReadAllText(_invenInfoPath);
            Debug.Log($"<color=#00ff00>{json}</color>");
            return JsonConvert.DeserializeObject<InventoryInfo>(json);


        }
        catch (Exception e)
        {
            Debug.Log($"<color=ff0000>{e}</color>");
        }
        finally
        {

        };
        return default(InventoryInfo);
    }
}
