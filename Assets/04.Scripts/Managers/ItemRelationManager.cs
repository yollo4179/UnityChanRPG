using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class ItemRelationManager {
    public class ItemEnumGroup
    {
        public ItemEnumGroup(eITEMTYPE Type, eATLAS Atlas)
        {
            this.ItemType =Type;
            this.Atlas = Atlas;
        }
        public eITEMTYPE ItemType;
        public eATLAS Atlas;
    }



    private static ItemRelationManager m_Instance=null;

    /*Awake에서 호출하지 말자*/
   public static ItemRelationManager GetInstance()
    {
        if(null == m_Instance)
        {
            m_Instance = new ItemRelationManager();
            m_Instance.MappingItemAtlas();
        }
        return m_Instance;
    }
   private Dictionary<eITEMTYPE, ItemEnumGroup> g_ItemEnumGroup;
   public ItemEnumGroup GetItemEnumGroup(eITEMTYPE Type)
   {
       return this.g_ItemEnumGroup[Type];
   }


    ItemEnumGroup GetAtlasCodeByItemType(eITEMTYPE Type)
    {
        if (this.g_ItemEnumGroup.ContainsKey(Type))
        {
            return this.g_ItemEnumGroup[Type];
        }
        Debug.LogError($"<color=#ff0000>Item Type not found in atlas mapping:{Type}</color>");
        return null;
    }
    public void MappingItemAtlas()
    {

        this.g_ItemEnumGroup = new Dictionary<eITEMTYPE, ItemEnumGroup>();
        this.g_ItemEnumGroup.Add(eITEMTYPE.INGREDIENT, new ItemEnumGroup(eITEMTYPE.INGREDIENT  ,eATLAS.ATLAS_ITEMIST_INGREDIENT));
        this.g_ItemEnumGroup.Add(eITEMTYPE.CONSUMABLE, new ItemEnumGroup(eITEMTYPE.CONSUMABLE ,eATLAS.ATLAS_ITEMLIST_CONSUMABLE));
        this.g_ItemEnumGroup.Add(eITEMTYPE.EQUIPMENT,  new ItemEnumGroup(eITEMTYPE.EQUIPMENT   ,eATLAS.ATLAS_ITEMLIST_EQUIPMENT));
    }

}
