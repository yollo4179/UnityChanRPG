using UnityEngine;

public class UI_World : UI_Base
{
    public override void Init()
    {
        Managers.UI.SetCanvas(gameObject, false);

    }
}
public class UI_WorldPart : UI_Base
{
   protected Transform _targetTransform;
    protected Vector3 _offset;
   protected Poolable _poolable;
    public void SetTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }
    public Vector3 Offset => _offset;
    
     
    
    public Transform TargetTransform=>_targetTransform;

    public override void Init()
    {
    }
}
