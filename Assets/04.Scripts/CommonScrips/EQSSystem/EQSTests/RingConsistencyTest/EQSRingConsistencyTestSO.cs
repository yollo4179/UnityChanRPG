using System.Collections.Generic;
using UnityEngine;
using static EQSManager;

// 4. 링 변경 페널티 테스트 - 같은 링 내에서의 이동 제한
[CreateAssetMenu(fileName = "EQSRingConsistencyTestSO", menuName = "Scriptable Objects/EQSRingConsistencyTestSO")]
public class EQSRingConsistencyTestSO : ScriptableObject, IEQSTest
{
    [SerializeField] private float _weight = 1.8f;
    [SerializeField] private float _sameRingPenalty = 0.2f;
    //[SerializeField] private float _ringChangeCooldown = 2.0f;

    private Dictionary<EQSQuerier, RingHistory> _ringHistories = new Dictionary<EQSQuerier, RingHistory>();

    public float Weight { get => _weight; set => _weight = value; }

    private class RingHistory
    {
        public int LastRingIndex;
        public float LastChangeTime;
        public Vector3 LastPosition;
    }

    public float Execute(EQSPoint point, EQSQuerier querier, EQSContext context)
    {
        if (!_ringHistories.ContainsKey(querier))//쿼리어가(자신) 이 없으면 나 등록  
        {
            _ringHistories[querier] = new RingHistory
            {
                LastRingIndex = -1,
                LastChangeTime = Time.time,
                LastPosition = querier.transform.position
            };
        }

        var history = _ringHistories[querier]; //나의 과거 이력

        // 현재 Querier가 있는 링 인덱스 찾기
        int currentQuerierRing = GetQuerierCurrentRing(querier, context); //현재 쿼리어가 차지하고 있는 링
        int targetRing = context.currentRingIndex; //현재 링의 인덱스 

        // 같은 링 내에서의 이동인 경우
        if (currentQuerierRing == targetRing && currentQuerierRing != -1)   //같은 링이다.
        {
            // 같은 링 내에서 너무 많이 이동했는지 체크
            float movedDistance = Vector3.Distance(history.LastPosition, point.pos); //이전위치차이가 일정량 이상이면 낮은 가중치
            if (movedDistance > 0.1f) // 작은 이동은 페널티
            {
                return _sameRingPenalty;
            }
        }

        _ringHistories[querier].LastPosition =point.pos;
        _ringHistories[querier].LastRingIndex =point.ringIndex;

        //// 링 변경 쿨다운 체크
        //if (Time.time - history.LastChangeTime < _ringChangeCooldown &&
        //    currentQuerierRing != targetRing)
        //{
        //    _ringHistories[querier].LastChangeTime = Time.time;

        //    return 0.3f; // 쿨다운 중 링 변경 시도
        //}

        return 1.0f;
    }

    private int GetQuerierCurrentRing(EQSQuerier querier, EQSContext context)
    {
        var point = querier.OccupingPoint;

        int idx = -1;
        return idx  = (null!=point)?point.ringIndex: idx;
    }
}