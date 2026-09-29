using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "EQSBlockTargetTest", menuName = "Scriptable Objects/EQSBlockTargetTest")]
public class EQSBlockTargetTest : ScriptableObject, IEQSTest
{
    
    [SerializeField] private float _weight = 50f;
    public float Weight { get=> _weight; set => _weight =value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {

        Vector3 targetPos =  context.target.transform.position;
        Vector3 myPos = point.pos;

        Vector3 targetLook = context.target.transform.forward;
        
        Vector3 Dir2PointFromTarget = (myPos - targetPos).normalized;  //먼저 가로막도록
        float dot= Vector3.Dot(targetLook, Dir2PointFromTarget);
        float normalize = (dot + 1) * 0.5f; // -1~1 -> 0~1


        return normalize; 

    }
}
