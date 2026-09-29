using System.Collections.Generic;
#if UNITY_ANDROID
using Unity.Android.Gradle.Manifest;
#endif
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static EQSManager;
public interface IEQSTest
{
    float Execute(EQSPoint point, EQSQuerier querier, EQSContext context);
    float Weight { get; set; }
}
//쿼리어가 하나씩 가진다. 
public class EQSContext
{
    public Transform target { get; set; }
    public List<EQSQuerier> allQueriers { get; set; }
    public EQSPoints points { get; set; }
    public int currentRingIndex { get; set; }
    public int currentPointIndex { get; set; }
}






// 6. 후진 테스트 - 후방 이동 옵션
[System.Serializable]public class RetreatTest : IEQSTest
{
    [SerializeField] private float _weight = 0.8f;
    [SerializeField] private bool _enabled = false;
    [SerializeField] private float _retreatTriggerDistance = 3.0f;

    public float Weight { get => _weight; set => _weight = value; }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        if (!_enabled) return 0.5f;

        float currentDistance = Vector3.Distance(querier.transform.position, context.target.position);

        // 너무 가까우면 후진 선호
        if (currentDistance < _retreatTriggerDistance)
        {
            float newDistance = Vector3.Distance(point.pos, context.target.position);
            if (newDistance > currentDistance)
            {
                return 1.0f;
            }
            return 0.2f;
        }

        return 0.5f;
    }
}

