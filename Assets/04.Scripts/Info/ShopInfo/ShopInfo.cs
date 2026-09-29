
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using System.IO;
public class ShopInfo 
{
    

    public  List<ItemInfo> InfoList;
    
    public void Init() {
        InfoList = new List<ItemInfo>(); 
    }

    /*Shop Npc 가 가지고 있는것이 맞나?  sHOPiNFO Manager를 만들어서 상점마다 ID를 부여할까*/
    //public void LoadItemsFromJson(string jsonPath){
    //    string Json = File.ReadAllText(jsonPath);
    //    m_ItemInfoList = JsonConvert.DeserializeObject<List<ItemInfo>>(jsonPath);
    //    if (m_ItemInfoList == null)
    //    {
    //        Debug.LogError($"<color=red>상점 아이템 리스트를 불러오는데 실패했습니다. 경로: {jsonPath}</color>");
    //        return;
    //    }
    //}



}
