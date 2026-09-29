using UnityEngine;
using System.Collections.Generic;
public enum ELEMENT
{
    BASE,
    FIRE,
    ICE,
}
[CreateAssetMenu(fileName = "ReinforceEffect", menuName = "Scriptable Objects/ReinforceEffect")]
public class ReinforceEffect : ScriptableObject
{
    [SerializeField] public List<ReinforceInfo> ReinforceEffectPrefabs;
    [SerializeField] public ELEMENT Element;
}
[System.Serializable] public class ReinforceInfo
{
    [SerializeField]                    public GameObject EffectPrefab;
    [SerializeField, TextArea(5, 1)]    public string Description;
    [SerializeField]                    public int Level;
  
    

}