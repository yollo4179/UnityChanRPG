using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "EQSStrafeTestSO", menuName = "Scriptable Objects/EQSStrafeTestSO")]

// 5. Strafe 테스트 - 측면 이동 선호
[System.Serializable]
public class EQSStrafeTestSO : ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 1.2f;
    [SerializeField] private bool _enabled = true;
    [SerializeField] private float _strafeAnglePreference = 67.5f; // 90도가 이상적
    private float _side = 1f; public float Side{ set => _side= (value>=0)? 1f : -1f; }
    //Circling
    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        if (!_enabled) return 0.5f;

        Vector3 toTarget = VectorUtil.PlatVector(context.target.position - querier.transform.position).normalized;
        Vector3 tanLine = Quaternion.Euler(0f, 5f*_side, 0f) * new Vector3(-1* toTarget.z, 0, toTarget.x)*_side; 
        
        Vector3 toPoint = VectorUtil.PlatVector(point.pos - querier.transform.position).normalized;

        float score = Mathf.Max(0f, Vector3.Dot(tanLine, toPoint)); // 0..1                                                          // 선명도 조절
        score = Mathf.Pow(score, 1.5f);
        return score;


        //float angle = Vector3.Angle(toTarget, toPoint);//내가 타겟을 바라보는 벡터, 내가 포인트를 바라보는 벡터사이 각도
        //float angleDiff = Mathf.Abs(angle - _strafeAnglePreference);//옆에 있는 포인트가 90도에 가깝다.

        //// 선호 각도에 가까울수록 높은 점수
        //return 1f - (angleDiff / 180f); //Acos 연산이니까 0~180도 사이
    }
}