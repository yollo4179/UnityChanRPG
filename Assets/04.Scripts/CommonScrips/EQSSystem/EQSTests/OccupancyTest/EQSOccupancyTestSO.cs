using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static EQSManager;
[CreateAssetMenu(fileName = "EQSOccupancyTestSO", menuName = "Scriptable Objects/EQSOccupancyTestSO")]
public class EQSOccupancyTestSO : ScriptableObject, IEQSTest //LineOfSight에서 헷갈려하는 상황에서 적은 비중의OFFSET 추가
{
    [SerializeField] private float _weight = 60f;
    [SerializeField] private float _maxDistance = 1;
    [SerializeField] private AnimationCurve _scoreCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        // 자신이 이미 점유하고 있는 포인트면 높은 점수
        if (point.occupant == querier)
            return 1.0f;

        // 다른 Querier가 점유 중이면 최저 점수
        if (point.occupant != null && point.occupant != querier)
            return float.NegativeInfinity;

        // 예약이 있는 경우 (근데 내가 아님)
        if (point.reserver != null && point.reserver != querier)
            return false ? 0f : float.NegativeInfinity;

      
        Vector3 vector = point.pos - querier.transform.position;
        vector.y=0;
        float distXZ = Vector3.Magnitude(vector);

        float score = distXZ/_maxDistance;

        return _scoreCurve.Evaluate(Mathf.Clamp01(1-score));//예약이 안되어 있을 경우에 경향성 유지(나와 제일) 제일 가까운 점
    }
    /*가까운 포인트 우선*/
}
