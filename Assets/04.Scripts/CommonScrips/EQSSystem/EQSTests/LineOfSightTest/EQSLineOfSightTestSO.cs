using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static EQSManager;
#if UNITY_EDITOR
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
#endif

// 3. 시야각 테스트 - 타겟을 바라보기 좋은 위치
[CreateAssetMenu(fileName = "EQSLineOfSightTestSO", menuName = "Scriptable Objects/EQSLineOfSightTestSO")]
public class EQSLineOfSightTestSO : ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 30f;
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private AnimationCurve _scoreCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] private float _radius = 1.5f;


    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        _obstacleLayer = LayerMask.GetMask("Enemies", "Player");

        Vector3 dirToTarget = (context.target.position - point.pos).normalized;
        dirToTarget.y=0;
        // 장애물 체크
        //if (Physics.Raycast(point.pos, dirToTarget,
        //    Vector3.Distance(point.pos, context.target.position), _obstacleLayer))
        //{
        //    return 0.0f;
        //}
       
       // if(point.ringIndex!=0)
        if (Physics.SphereCast(
             point.pos+Vector3.up*0.1f ,//Origin
             _radius,                        //Radius 
             dirToTarget,                //Dir
             out RaycastHit hit,          //RaycastHit
             1f,                       //RayDistance
            _obstacleLayer,
            QueryTriggerInteraction.Ignore
            ))
        { 
            return 0f;
        }

        // 타겟을 바라보는 각도 계산
        Vector3 querierForward = querier.transform.forward;
        float dotProduct = Vector3.Dot(querierForward, dirToTarget);

        return 1f;//_scoreCurve.Evaluate((dotProduct + 1f) * 0.5f); // -1~1을 0~1로 정규화 
    }
  
}