using UnityEngine;

[CreateAssetMenu(fileName = "EffectData", menuName = "Scriptable Objects/EffectData")]
public class EffectData : ScriptableObject
{
    [SerializeField] Vector3 loacalPosition;
    [SerializeField] Vector3 loacelRotation;
    [SerializeField] Vector3 localScale;
    [SerializeField] GameObject effectPrefab;
}
