using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EQSBattleEscapeHelper : MonoBehaviour
{
    [Header("Refs")]
    NavMeshAgent _agent;

    [Header("Masks")]
    [SerializeField] LayerMask _enememyMask;

    [Header("Escape Config")]
    [SerializeField] int numRay = 24;   // 360° 분할 수
    [SerializeField] float CheckAngle = 30f;  // 전방 부채살 반각
    [SerializeField] float detectRadius = 2.5f; // 혼잡도 집계 반경
    [SerializeField] float lookAhead = 1.2f; // 바깥으로 나갈 기본 거리
    [SerializeField] float sampleMaxDist = 1.0f; // NavMesh.SamplePosition 탐색 반경
    [SerializeField] float clearanceRange = 2.0f; // 전방 여유거리 측정 구간
    [SerializeField] float minClearance = 0.6f; // 이 값 미만이면 막힘
    [SerializeField] float boostDuration = 0.9f; // 부스트 유지 시간
    [SerializeField] float speedMul = 1.2f; // 속도/가속 부스트
    [SerializeField] float accelMul = 1.5f;
    [SerializeField] int escapePriority = 0;    // 낮을수록 우선권↑ (0~99)

    [Header("Yield Corridor (optional)")]
    [SerializeField] bool openYieldCorridor = true; // 통로 양보 활성화 여부
    [SerializeField] float corridorArcDeg = 75f;  // 내 진행 방향 기준 양보 부채살
    [SerializeField] float corridorRadius = 2.0f; // 양보 명령 반경
    // --- 내부 상태 ---
    public bool IsEscaping { get; set; }
    float restoreTime;
    float origSpeed, origAccel;
    int origPriority;
    bool origAutoBraking;
    Collider[] _overlaps =new Collider[16];
    //struct YieldBackup { public NavMeshAgent ag; public int prio; public float spd; public bool stopped; }
    //readonly List<YieldBackup> _yielded = new();

    void Awake()
    {
        if (!_agent) _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!IsEscaping) return;

        // 종료 조건: 시간 만료 or 목적지 도착 or 경로 무효
        if (/*Time.time >= restoreTime ||*/
            (/*!_agent.pathPending &&*/ _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, 0.15f)) ||
            _agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            ExitEscapeMode();
        }
    }

    // ====== 외부에서 호출: CBS 최대 interest 방향(dir) 전달 ======
    public bool EnterEscapeMode(Vector3 preferredDir)
    {
        preferredDir = VectorUtil.PlatVector(preferredDir);
        if (preferredDir.sqrMagnitude < 0.001f) preferredDir = transform.forward;

        // 1) 가장 덜 막힌 방향 고르기
        Vector3 bestDir =Vector3.zero;
        if(!PickLeastCrowdedDirection(preferredDir.normalized,out  bestDir))
        return false;
        // 2) 그 방향으로 1~2m 바깥 탈출점 찾기
        if (!FindEscapePoint(bestDir, out var escapePoint))
            return false; // 실패 시 호출 측에서 다른 폴백 사용

        // 3) 파라미터 부스트 + 목적지 설정
        StartEscape(escapePoint, bestDir);

        // 4) (옵션) 통로 양보 열기
       // if (openYieldCorridor) OpenYieldCorridor(bestDir);

        return true;
    }

    // 가장 덜 막힌 방향 선택(확실함)
    bool PickLeastCrowdedDirection(Vector3 preferred,out Vector3 bestDir)
    {
        // 분할 각
        float step = 360f / numRay;
        int bestIdx = -1;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < numRay; ++i)
        {
            Quaternion rot = Quaternion.AngleAxis(step * i, Vector3.up);
            Vector3 dir = rot * preferred;

            // 혼잡도: 내 위치에서 detectRadius 안, dir ± wedgeHalfAngle 안에 있는 개체 수
            int count = CountNeighborsInWedge(dir, detectRadius, CheckAngle);
            if (0 == count) continue;

            // 전방 여유거리(스피어캐스트): 작을수록 패널티
            float clear = ForwardClearance(dir, clearanceRange);
            float clearPenalty =/* (clear < minClearance) ? 2f :*/ (1f - Mathf.Clamp01(clear / clearanceRange));

            // NavMesh Raycast 막힘 여부 패널티
            //bool navBlocked = NavMesh.Raycast(transform.position, transform.position + dir * clearanceRange,
            //                                 out _, NavMesh.AllAreas);
            //float navPenalty = navBlocked ? 1.5f : 0f;

            float score = count * 1.0f + clearPenalty * 1.2f;//+ navPenalty * 1.5f;

            if (score < bestScore)
            {
                bestScore = score;
                bestIdx = i;
            }
        }
        if(bestIdx == -1)
        {
            bestDir = Vector3.zero;
            return false;
        }
        bestDir =Quaternion.AngleAxis(step * bestIdx, Vector3.up) * preferred;
        return true;
    }

    int CountNeighborsInWedge(Vector3 dir, float radius, float halfAngleDeg)
    {
        int cnt = 0;
        int n = Physics.OverlapSphereNonAlloc(transform.position, radius, _overlaps, _enememyMask,
                                              QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; ++i)
        {
            var c = _overlaps[i];
            EQSQuerier k; 
            if ( (k= c.gameObject.GetComponent<EQSQuerier>()) != null) {

                if(true==k.IsChasing) //움직이는 놈 은 CBS, 차지한 놈 피하고 , 
                continue; 
            
            }
            else
            {
                continue; 
            }
            continue;

            if (!c || c.attachedRigidbody == _agent?.GetComponent<Rigidbody>()) continue;
            Vector3 to = c.transform.position - transform.position; to.y = 0;
            if (to.sqrMagnitude < 0.01f) continue;
            float Deg = -1;
            if ((Deg = Vector3.Angle(dir, to)) <= halfAngleDeg)
                cnt++;
        }
        return cnt;
    }

    float ForwardClearance(Vector3 dir, float range)
    {
        // 내 반경에 가깝게 스피어캐스트
        float r = Mathf.Max(0.05f, _agent.radius * 0.9f);
        if (Physics.SphereCast(transform.position + Vector3.up * 0.2f, r, dir, out var hit, range, _enememyMask,
                               QueryTriggerInteraction.Ignore))
            return hit.distance;
        return range;
    }

    bool FindEscapePoint(Vector3 dir, out Vector3 escapePoint)
    {
        // 기본 거리에서 시작, 실패 시 점차 늘림(확실함)
        float[] tries = { lookAhead, lookAhead + 0.6f, lookAhead + 1.2f };
        foreach (float d in tries)
        {
            Vector3 raw = transform.position + dir.normalized * d;
            if (NavMesh.SamplePosition(raw, out var hit, sampleMaxDist, NavMesh.AllAreas))
            {
                // 경로가 실제로 완주 가능한지 확인
                var path = new NavMeshPath();
                if (_agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    escapePoint = hit.position;
                    return true;
                }
            }
        }
        escapePoint = Vector3.zero;
        return false;
    }

    void StartEscape(Vector3 escapePoint, Vector3 dir)
    {
        if (!_agent) return;

        // 원래 값 백업
        origSpeed       = _agent.speed;
        origAccel       = _agent.acceleration;
        origPriority    = _agent.avoidancePriority;
        origAutoBraking = _agent.autoBraking;

        // 부스트 + 설정
        _agent.isStopped      = false;
        _agent.autoBraking    = false;
        _agent.avoidancePriority = Mathf.Clamp(escapePriority, 0, 99);
        _agent.speed          = origSpeed * speedMul;
        _agent.acceleration   = origAccel * accelMul;

        // 회전이 둔하면 수동 회전 모드로 전환해도 됨(프로젝트 스타일에 따라)
        // agent.updateRotation = false;

        _agent.SetDestination(escapePoint);

        IsEscaping = true;
        restoreTime = Time.time + boostDuration;
    }

    //void OpenYieldCorridor(Vector3 dir)
    //{
    //    _yielded.Clear();

    //    int n = Physics.OverlapSphereNonAlloc(transform.position, corridorRadius, _overlaps,
    //                                          _enememyMask, QueryTriggerInteraction.Ignore);
    //    for (int i = 0; i < n; ++i)
    //    {
    //        var c = _overlaps[i]; if (!c) continue;
    //        Vector3 to = c.transform.position - transform.position; to.y = 0;
    //        if (Vector3.Angle(dir, to) > corridorArcDeg * 0.5f) continue;

    //        var other = c.GetComponentInParent<NavMeshAgent>();
    //        if (!other || other == _agent) continue;

    //        _yielded.Add(new YieldBackup
    //        {
    //            ag = other,
    //            prio = other.avoidancePriority,
    //            spd = other.speed,
    //            stopped = other.isStopped
    //        });

    //        // 양보: 우선순위 낮춤(숫자 큼), 속도↓ 또는 잠깐 정지
    //        other.avoidancePriority = 99;
    //        other.speed *= 0.3f;
    //        // other.isStopped = true; // 필요하면 활성화
    //    }
    //}

    void ExitEscapeMode()
    {
        if (!_agent) return;

        // 복구
        _agent.speed            = origSpeed;
        _agent.acceleration     = origAccel;
        _agent.avoidancePriority= origPriority;
        _agent.autoBraking      = origAutoBraking;
        _agent.updateRotation = true; // 수동 회전으로 바꿨다면 복구

        // 양보 복구
        //foreach (var y in _yielded)
        //{
        //    if (!y.ag) continue;
        //    y.ag.avoidancePriority = y.prio;
        //    y.ag.speed             = y.spd;
        //    y.ag.isStopped         = y.stopped;
        //}
        //_yielded.Clear();

        IsEscaping = false;
    }
}
