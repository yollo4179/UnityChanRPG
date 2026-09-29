using System.Collections.Generic;
using UnityEngine;

public class PrefabManager
{

    private  PrefabItemCatalog _itemCatalog;
    public Dictionary<string, Poolable> _prefabItems;
    public Dictionary<int ,PrefabItemInfo> _prefabs;

    public void Init()
    {

        _prefabItems = new Dictionary<string, Poolable>();
        _prefabs = new Dictionary<int , PrefabItemInfo>();

        _itemCatalog = Resources.Load<PrefabItemCatalog>("Data/ScriptableObjects/ItemPrefabs/PrefabItemCatalog");
        
        /*Register Pooling */
        foreach (var prefabItem in _itemCatalog._prefabItems)
        {
            Managers.Pool.CreatePool(prefabItem.prefabItem, false, 1);
            _prefabs.Add(prefabItem.itemID, prefabItem);
        }
        _prefabItems = new Dictionary<string, Poolable>();
    }
    public void  ActivatePrefab(int itemID ,Transform parent, string instanceID ="BASE")
    {
       
        if (false ==_prefabs.ContainsKey(itemID)) return;

        if (_prefabItems.ContainsKey("BASE")) InActivatePrefab();//기본 무기 있으면 장착 해제

        Poolable handle = Managers.Pool.LendPoolableTo(_prefabs[itemID].prefabItem.name,parent);
        handle.transform.localScale = _prefabs[itemID].Scale;
        _prefabItems.Add(instanceID, handle);

    }
    public void InActivatePrefab (string instanceID= "BASE")
    {
        if (false ==  _prefabItems.ContainsKey(instanceID)) return;
        Managers.Pool.GetBack(_prefabItems[instanceID]); 
        _prefabItems.Remove(instanceID);
    }

}
