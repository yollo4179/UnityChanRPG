using UnityEngine;
using static EQSManager;

// 7. 분산 테스트 - Querier들 간의 적절한 간격 유지
[CreateAssetMenu(fileName = "EQSSpacingTestSO", menuName = "Scriptable Objects/EQSSpacingTestSO")]
public class EQSSpacingTestSO :ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 30f;
    [SerializeField] private float _minSpacing = 0.5f;

    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        float score = 1.0f;

        foreach (var otherQuerier in context.allQueriers)
        {
            if (otherQuerier == querier) continue;//나 뺴고

            EQSQuerierBehaviour_Battle queryBehaviour = otherQuerier.EQSQuerierBehabiour; 
            if (null !=queryBehaviour) //누가 예약했다.
            {
                //EQSManager.EQSPoint nowReservedPointByOtherQuerier = queryBehaviour.ReservedPoint;
                //if (null != nowReservedPointByOtherQuerier)
                {
                    float distance = Vector3.Distance(point.pos, otherQuerier.transform.position);

                    //float distance = Vector3.Distance(point.pos, nowReservedPointByOtherQuerier.pos);
                    if (distance < _minSpacing)
                    {
                        score *= (distance / _minSpacing);
                    }

                }

            }

            
        }

        return Mathf.Clamp01(score);
    }
}