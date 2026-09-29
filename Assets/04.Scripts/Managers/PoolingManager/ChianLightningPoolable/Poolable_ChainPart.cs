using DigitalRuby.LightningBolt;
using UnityEngine;

public class Poolable_SkillChainLightning : Poolable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    LineRendererController _lineRController ;
    private void Awake()
    {
        _lineRController = GetComponent<LineRendererController>();
    }
    public override void Init()
    {
       
        GetComponent<LineRendererController>()?.SetPosition(Vector3.zero, Vector3.zero);
        GetComponentInChildren<LightningBoltScript>().StartObject=null ;
        GetComponentInChildren<LightningBoltScript>().EndObject=null ;
    }
}
