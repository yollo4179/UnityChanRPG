using UnityEngine;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class DataManager
{

    

    public void Init()
    {
        LoadItemData();
    }

    private Dictionary<int, ItemData> m_ItemData;



    public void LoadItemData()
    {
        /*ItemData*/
        /*Consumable*/
        TextAsset asset = Resources.Load<TextAsset>("Data/Json/ConsumableItemData");
        var json = asset.text;
        Debug.Log(json);
        ItemData[] arrItemData = JsonConvert.DeserializeObject<ItemData[]>(json);
        this.m_ItemData =arrItemData.ToDictionary(x => x.ItemID);
        /*Ingrediant*/
        asset = Resources.Load<TextAsset>("Data/Json/IngrediantItemsData");
        json = asset.text;
        Debug.Log(json);
        arrItemData = JsonConvert.DeserializeObject<ItemData[]>(json);
        foreach (ItemData Data in arrItemData)
        {
            if (!this.m_ItemData.ContainsKey(Data.ItemID))
                this.m_ItemData.Add(Data.ItemID, Data);
        }
        /*Equipment*/
        asset = Resources.Load<TextAsset>("Data/Json/EquipmentItemData");
        json = asset.text;
        Debug.Log(json);
        arrItemData = JsonConvert.DeserializeObject<ItemData[]>(json);

        foreach (ItemData Data in arrItemData)
        {
            if (!this.m_ItemData.ContainsKey(Data.ItemID))
                this.m_ItemData.Add(Data.ItemID, Data);
        }

   
    }
    public ItemData GetRandomIngredientData()
    {
        int InitialValue = 100; 
        var RandomID = Random.Range(0, 15)+InitialValue;
        
        if(this.m_ItemData.ContainsKey(RandomID))
        {
            var randomData = m_ItemData[RandomID];
            return randomData;
        }
        return null; 
    }
    public ItemData GetItemData(int ItemID)
    {
      
       if (m_ItemData.ContainsKey(ItemID))
       {
            return m_ItemData[ItemID];
       }
        Debug.LogError("해당 아이디를 가진 아이템은 없다.");
        return null;
        
    }

}

