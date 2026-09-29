using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PrefabItemCatalog", menuName = "Scriptable Objects/PrefabItemCatalog")]
public class PrefabItemCatalog : ScriptableObject
{
    [SerializeField] public List<PrefabItemInfo> _prefabItems;  



}
[System.Serializable] public class PrefabItemInfo //ForEqupment
{
    [SerializeField]  public GameObject prefabItem;
    [SerializeField] public int itemID;
    [SerializeField] public Vector3 Scale;
}
