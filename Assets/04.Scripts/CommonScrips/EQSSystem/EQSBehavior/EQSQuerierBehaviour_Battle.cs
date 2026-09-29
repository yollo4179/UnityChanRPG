
using System.Drawing;
using Unity.AI.Navigation;

#if UNITY_ANDROID
using Unity.Android.Gradle.Manifest;
#endif

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AppUI.Core;
using System;
using Unity.Behavior;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.InputSystem.Processors;


public class EQSQuerierBehaviour_Battle : MonoBehaviour
{
    [Header("EQS Query Setting")]
    //[SerializeField] 
    private EQSQuery _query;
    [SerializeField] private float _queryInterval = 0.2f; //질의 주기 
    [SerializeField] private float _reservationTimeOut = 3f;
    [SerializeField] private bool _isBehaviorTreeOn = false;
    /*Movement로 따로 묶어도 될거 ㅅ같기도 하고 */
    //[Header("Movement")]
    //[SerializeField]
    private NavMeshAgent _agent;


    [SerializeField] private float _stoppingDistance = 0.5f;
    //[SerializeField] private NavMeshModifierVolume _navModiVolume;
    //[SerializeField] private NavMeshUtil _navMeshUtil;


    [Header("Combat Behavior")]
    [SerializeField] private eLOCOMOTION_MODE _behaviorMode;
    //[SerializeField] private float _attackRange = 5f;

    [Header("NavMeshZoneID Fileter")]
    [SerializeField] private eNavMeshZoneID _ZoneID;

    /* [SerializeField]*/
    private Animator _animator;
    private float _NowSpeed = 0;
    private EQSQuerier _querier;
    private EQSPoint _reservedPoint;
    public EQSPoint ReservedPoint { get => _reservedPoint; }
    private EQSPoint _goalPoint;
    private EQSTarget _target;
    private float _lastQueryTime;
    private float _lastReservationTime;


    /*ContextBasedSteering*/
    float[] interestMap;
    float[] dangerMap;
    Vector3[] contextMap;
    bool MoveBackward = false;
    Vector3 backwardTarget;
    int iSeg = 32;
    List<SphereDebug> _arrDebugCol = new();


    Vector3 _escapePos = Vector3.zero; 

    EQSBattleEscapeHelper _escapeHelper;
    bool _nowEscape = false;
    // 벽 따라가기 관련 파라미터
    float wallFollowWeight = 0.8f;      // 벽 따라가기 가중치
    float targetPullWeight = 0.4f;      // 목표 끌어당기는 힘 (약하게)
    float wallDistance = 0.6f;          // 벽과 유지할 거리
    float lookAheadDistance = 1.5f;     // 앞쪽 탐지 거리
    bool isWallFollowing = false;

    // 포위망 감지 및 탈출 파라미터
    float encirclementRadius = 2.0f;           // 포위망 감지 반경
    float escapeForceMultiplier = 1.5f;        // 탈출력 배수
    float concaveDetectionAngle = 90f;        // 오목한 부분 감지 각도
    float breakoutBonus = 0.8f;                // 돌파 지점 보너스
    bool isEncircled = false;
    Vector3 escapeDirection = Vector3.zero;

    // 포위망 상태 변수
    List<Vector3> enemyPositions = new List<Vector3>();
    Dictionary<int, float> directionHistory = new Dictionary<int, float>(); // 방향별 이동 이력
  //  float stuckTimer = 0f;
   // Vector3 lastPosition = Vector3.zero;

    //대칭 깨기 
    // 이전 프레임의 방향 저장 (히스테리시스용)
    private Vector3 previousSteeringDirection = Vector3.zero;
    private float hysteresisWeight = 0.3f;

    // 교착 상태 감지용
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private float stuckThreshold = 0.5f; // 움직임이 이 값보다 작으면 stuck으로 판단
    private float stuckTimeThreshold = 0.5f; // 이 시간 이상 stuck이면 특별 처리

    // 랜덤 노이즈 (대칭 깨기용)
    private float noiseStrength = 0.1f;
    private float noiseTimer = 0f;


    /*Check Attack Condition*/
    BehaviorGraphAgent _behaviorAgent;
   


    #region Action
    /*Strafing*/
    StrafingControl _strafingControl =new();
    IMonsterAttackPrerequisiteChecker _monsterAttack; 
    #endregion


    #region InnerClasses
    private enum eLOCOMOTION_MODE
    {
            Surround, 
            Strafe_L,
            Strafe_R,
            Retreat,
            Aggressive
    }
    class SphereDebug
    {
        public Vector3 pos;
        public float radius;
        public UnityEngine.Color color;
        public float alpha;
        public SphereDebug(Vector3 p, float r, UnityEngine.Color c, double Alpha = 0.05) { pos = p; radius = r; color = c; alpha=0.05f; }
    }
    /* Now Detecting Mode*/
    [System.Serializable] private class StrafingControl
    {
        bool _isStafing = false; public bool IsStafing { get => _isStafing; set => _isStafing = value; }
        public float _strafingDuration = 5;
        public float _strafingCoolTime = 5;
        public float _currentTime;
        public float _weight =50;
        public bool _isInitDone = false; 
    }

    #endregion





    private void Start()
    {
        _agent =GetComponent<NavMeshAgent>();
        _querier = GetComponent<EQSQuerier>(); //Owner
        _animator =GetComponentInChildren<Animator>();
        _query = GetComponent<EQSQuery>();

        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _monsterAttack = GetComponent<IMonsterAttackPrerequisiteChecker>();

        _escapeHelper = GetComponent<EQSBattleEscapeHelper>();
       
    }
    private void Update() //InLocomotionState;
    {
        //if(_isBehaviorTreeOn && true ==_querier.IsChasing)
           // BattleBehavior();
    }
    #region 스트레이프
    private void OnStrafe()
    {
        
        if( (null!=_goalPoint && 0 == _goalPoint.ringIndex) || (null!=_querier.OccupingPoint && 0 == _querier.OccupingPoint.ringIndex) )
        {
            _query.GetTestConfigByType(typeof(EQSStrafeTestSO), out var testConfig);
            testConfig.GetTest().Weight = testConfig.GetOriginTest().Weight;
            _query.RestoreBaseTestWeights();
           
            return;  
        }

        switch(_strafingControl.IsStafing)
        {
            case false:
            {
                    if (_strafingControl._strafingCoolTime < Time.time - _strafingControl._currentTime)
                    {
                        _strafingControl.IsStafing = true;
                        _strafingControl._currentTime = Time.time;
                    }
                    break;
            }
            case true:
            {
                    if (false ==_strafingControl._isInitDone)
                    {
                        _query.GetTestConfigByType(typeof(EQSStrafeTestSO), out var testConfig);
                        testConfig.GetTest().Weight = _strafingControl._weight;
                        (testConfig.GetTest() as EQSStrafeTestSO).Side = UnityEngine.Random.Range(-1f, 1f);
                        
                        _query.LowerBaseTestWeights(1000f);//Dbz조심

                        _strafingControl._currentTime = Time.time;
                        _strafingControl._isInitDone =true;

                        _strafingControl._strafingDuration =UnityEngine.Random.Range(6f, 10f);
                    }

                    if (_strafingControl._strafingDuration <Time.time - _strafingControl._currentTime)
                    {
                        _query.GetTestConfigByType(typeof(EQSStrafeTestSO), out var testConfig);
                        testConfig.GetTest().Weight = testConfig.GetOriginTest().Weight;
                      
                        _query.RestoreBaseTestWeights();

                        _strafingControl._currentTime = Time.time;

                        _strafingControl.IsStafing = false;
                        _strafingControl._isInitDone =false;  
                    }

                    break;
            }
        }

    }
    #endregion
    #region Behaviors
    public void BattleBehavior()
    {
       
        
        


        /*1초마다 새로운 쿼리 제시*/
        if (Time.time - _lastQueryTime > _queryInterval )
        {
            // ReleaseFloackArea(); //이동 일단
            QueryNewPoint();
            _lastQueryTime = Time.time;
        }
        // 예약한 후에 3초 지나고 예약 타임아웃 체크
        if (_reservedPoint != null && Time.time - _lastReservationTime > _reservationTimeOut)
        {
            ReleaseReservedPoint(); //소프트락 해제
        }
        if (_goalPoint != null)
        {

            // 도착 체크
            if (Vector3.Distance(transform.position, _goalPoint.pos) < _stoppingDistance)
            {
                OccupyPoint(_goalPoint);
                //_nowEscape =false;
            }
        }
        if (_querier ==_querier.OccupingPoint?.occupant && null!=EQSManager.GetInstance().GetTarget())
        {

            Vector3 lookDir =VectorUtil.PlatVector( EQSManager.GetInstance().GetTarget().transform.position- transform.position );
            var targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime);
        }



        /*점유 해제는 언제하지 Querier의 위치와 Position의 위치가 꽤 멀어졌을때*/
        if (!_isBehaviorTreeOn)
        {
            float TargetSpeed = _agent.velocity.magnitude;
            _NowSpeed  = Mathf.Lerp(_animator.GetFloat("MovementSpeed"), TargetSpeed, 5f*Time.deltaTime);
            _animator.SetFloat("MovementSpeed", _NowSpeed);
        }
       
        DoContextBassedSteering();
        OnStrafe();
        CheckAttackCondition(); 
        
    }
    #endregion


    #region ContextBasedSteering

    public Vector3 CalculateSteering(EQSPoint  goalPoint)
    {
        _arrDebugCol.Clear();

        int iSeg = 32;
        float length = 0.8f;
        LayerMask layerMask = LayerMask.GetMask("Enemies", "Player");
        Vector3 myPos = transform.position + Vector3.up * 0.3f;

        Vector3[] contextMap = new Vector3[iSeg];
        float[] interestMap = new float[iSeg];
        float[] dangerMap = new float[iSeg];
        Vector3 steeringVector = Vector3.zero;

        float radius = 0.2f;
        bool bAvoidance = false;

        // 교착 상태 감지
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        if (moveDistance < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = transform.position;

        if (goalPoint != null)
        {
            Vector3 desiredVector = (goalPoint.pos - transform.position).normalized;

            // Context Map 초기화
            for (int i = 0; i < iSeg; ++i)
            {
                float theta = (360f / iSeg) * Mathf.Deg2Rad * i;
                Vector3 vDir = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)).normalized;
                contextMap[i] = vDir;
            }

            // Interest와 Danger 계산
            for (int i = 0; i < iSeg; ++i)
            {
                // Interest Map (목표 방향 선호도)
                interestMap[i] = Mathf.Clamp01(0.5f + 0.5f * Vector3.Dot(contextMap[i], desiredVector));
                dangerMap[i] = 0;

                // Danger Map (장애물 회피)
                if (Physics.SphereCast(myPos, radius, contextMap[i], out var hit, length, layerMask, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.gameObject != this.gameObject)
                    {
                        // 거리 기반 위험도 (가까울수록 높음)
                        float distanceRatio = 1f - (hit.distance / length);
                        dangerMap[i] = Mathf.Max(0.5f, distanceRatio);
                        bAvoidance = true;
                    }
                }

                // 방법 1: 가중치 블렌딩 (더 부드러운 전환)
                float safetyWeight = 1f - dangerMap[i];
                interestMap[i] *= safetyWeight;
            }

            // 방법 2: 교착 상태 해결 - 대칭 깨기
            if (stuckTimer > stuckTimeThreshold)
            {
                ApplySymmetryBreaking(interestMap, contextMap, desiredVector);
            }

            // 방법 3: 가우시안 블러 적용 (부드러운 전환)
            ApplyGaussianBlur(interestMap, iSeg);

            // 방법 4: 2차 관심 맵 (Secondary Interest) - 우회 경로 찾기
            if (bAvoidance)
            {
                ApplySecondaryInterest(interestMap, dangerMap, contextMap, desiredVector, iSeg);
            }

            // 최종 Steering Vector 계산
            float maxInterest = 0f;
            int bestDirection = -1;

            // 방법 5: 슬롯 기반 선택 (연속된 안전한 방향 찾기)
            int[] safeSlots = FindConsecutiveSafeSlots(dangerMap, iSeg);

            for (int i = 0; i < iSeg; ++i)
            {
                // 안전한 슬롯에 보너스 부여
                if (IsInSafeSlot(i, safeSlots))
                {
                    interestMap[i] *= 1.2f; // 안전한 경로 선호
                }

                if (interestMap[i] > maxInterest)
                {
                    maxInterest = interestMap[i];
                    bestDirection = i;
                }

                // 가중치가 있는 방향만 합산
                if (interestMap[i] > 0.1f)
                {
                    steeringVector += interestMap[i] * contextMap[i];
                }

                //// 디버깅 시각화
                //UnityEngine.Color color = dangerMap[i] > 0 ? UnityEngine.Color.red : UnityEngine.Color.green;
                //Debug.DrawRay(myPos, contextMap[i] * interestMap[i], color);
            }

            // 방법 6: 히스테리시스 (이전 방향 고려)
            if (previousSteeringDirection != Vector3.zero && steeringVector.magnitude > 0.1f)
            {
                steeringVector = Vector3.Lerp(steeringVector.normalized, previousSteeringDirection, hysteresisWeight);
            }

            // 방법 7: 노이즈 추가 (완전한 교착 상태 방지)
            if (stuckTimer > stuckTimeThreshold * 2f)
            {
                noiseTimer += Time.deltaTime;
                float noiseAngle = Mathf.Sin(noiseTimer * 10f) * noiseStrength;
                steeringVector = Quaternion.Euler(0, noiseAngle * 180f, 0) * steeringVector;
            }

            previousSteeringDirection = steeringVector.normalized;
        }

        return steeringVector.normalized;
    }
    public void DoContextBassedSteering()
    {
        _arrDebugCol.Clear();
        int iSeg = 32;
        float length = 0.8f;
        LayerMask layerMask = LayerMask.GetMask("Enemies", "Player");
        Vector3 myPos = transform.position + Vector3.up*0.3f;
        contextMap = new Vector3[iSeg];
        interestMap = new float[iSeg];
        float[] dangerMap = new float[iSeg];
        Vector3 steeringVector = Vector3.zero;

        float radius = 0.6f; //3.14*2*1.5(Radius InCircle) /8 =1.15
        bool bAvoidance = false;

        /*Test*/
        float allowanceAngleAsAvoidance = 30f;
        int cntBlocked = 0;
        int cntTot = 0;
        Vector3[] escapeVector = new Vector3[3];


        // 교착 상태 감지
        float moveDistance = Vector3.Distance(transform.position, lastPosition);
        if (moveDistance < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = transform.position;


        if (null != _goalPoint)
        {
            Vector3 desiredVector = (_goalPoint.pos - transform.position).normalized;
            for (int i = 0; i<iSeg; ++i)
            {   /*Init*/
                float theta = (360f/iSeg) *Mathf.Deg2Rad *i;
                Vector3 vDir = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)).normalized;
                contextMap[i] = vDir;
            }

            for (int i = 0; i<iSeg; ++i)
            {

                dangerMap[i] =0;
                interestMap[i]  = Mathf.Clamp01(0.5f + 0.5f * Vector3.Dot(contextMap[i], desiredVector));
                UnityEngine.Color color = UnityEngine.Color.green;
                bool checkSum = false;
                if (Vector3.Angle(contextMap[i].normalized, desiredVector.normalized) < allowanceAngleAsAvoidance)
                {
                    ++cntTot;
                    checkSum=true;
                }

                if (Physics.SphereCast(
                    myPos,
                    radius,
                    contextMap[i],
                    out var hit,
                    length,
                    layerMask,
                    QueryTriggerInteraction.Ignore))
                {


                    if (hit.collider.gameObject != this.gameObject)
                    {
                       

                        if (checkSum) ++cntBlocked;

                        float distanceRatio = hit.distance / length;
                        dangerMap[i] = Mathf.Lerp(1.0f, 0.2f, distanceRatio);
                        color = UnityEngine.Color.red;
                        bAvoidance = true;

                    }
                }
               
                //위험한 만큼 가중치를 줄인다. (위험도가 클 수록 가중치가 낮도록)
                interestMap[i] = Mathf.Clamp01(interestMap[i] * (1 - dangerMap[i]));
                if (0 < dangerMap[i])
                {
                    color = UnityEngine.Color.red;
                    bAvoidance =true;
                    Debug.DrawRay(myPos, contextMap[i], color);
                }
                Vector3 contextVector = interestMap[i] * contextMap[i];
                {
                    if (!bAvoidance)
                        _arrDebugCol.Add(new SphereDebug(myPos + contextVector, radius, color));
                    else
                        _arrDebugCol.Add(new SphereDebug(myPos + contextMap[i] * length, radius, color));
                    Debug.DrawRay(myPos, contextVector, color);
                }
            }

        }
      
        // 방법 4: 2차 관심 맵 (Secondary Interest) - 우회 경로 찾기
        if (bAvoidance)
        {
            

            // 방법 5: 슬롯 기반 선택 (연속된 안전한 방향 찾기)
            int[] safeSlots = FindConsecutiveSafeSlots(dangerMap, iSeg);
            //// 최종 Steering Vector 계산
            float maxInterest = 0f;
            int bestDirection = -1;
            if (stuckTimer > stuckTimeThreshold)
            {
                for (int i = 0; i < iSeg; ++i)
                {
                    //// 안전한 슬롯에 보너스 부여
                    if (IsInSafeSlot(i, safeSlots))
                    {
                        interestMap[i] *= interestMap[i]*1.2f; // 안전한 경로 선호
                    }

                    if (interestMap[i] > maxInterest)
                    {
                        maxInterest = interestMap[i];
                        bestDirection = i;
                    }

                    //// 가중치가 있는 방향만 합산
                    //if (interestMap[i] > 0.1f)
                    Vector3 contrastVector = Vector3.zero;
                    for (int k = 0; k<iSeg; ++k)
                    {

                        contrastVector+= interestMap[i]*contextMap[i];
                    }
                    
                    {
                        Vector3 toGoal = _goalPoint.pos -transform.position;
                        if (stuckTimer>stuckThreshold)
                        {
                            if (IsTargetDirectionBlocked(transform.position, toGoal, out Vector3 wallNormal, out float hitDistance))
                            {
                                interestMap[i] = CalculateWallFollowingInterest(contextMap[i], toGoal, wallNormal);
                                _nowEscape =true;
                            }
                            steeringVector += interestMap[i] * contextMap[i];
                            
                        }
                    }

                }
            }
            if(steeringVector.magnitude < 0.1f && bestDirection>=0) 
            steeringVector+=(interestMap[bestDirection] * contextMap[bestDirection]);
            if(true ==_nowEscape)
            {

                _escapePos = transform.position + steeringVector.normalized * 6f;
                NavMesh.SamplePosition(_escapePos, out NavMeshHit hit, 8f, NavMesh.AllAreas);
                _escapePos  = hit.position;
                _agent.SetDestination(_escapePos);

               
                return;
            }

        }
        else
        {
            for (int i = 0; i<iSeg; ++i)
            {

                steeringVector += interestMap[i]*contextMap[i];
            }
        }



        // 방법 6: 히스테리시스 (이전 방향 고려)
        //if (previousSteeringDirection != Vector3.zero && steeringVector.magnitude > 0.1f )
        //{
        //    steeringVector = Vector3.Lerp( steeringVector.normalized, previousSteeringDirection, hysteresisWeight);
        //}


        // 방법 7: 노이즈 추가 (완전한 교착 상태 방지)
        if (stuckTimer > stuckTimeThreshold * 2f)
        {
            noiseTimer += Time.deltaTime;
            float noiseAngle = Mathf.Sin(noiseTimer * 10f) * noiseStrength;
            steeringVector = Quaternion.Euler(0, noiseAngle * 180f, 0) * steeringVector;
        }

        previousSteeringDirection = steeringVector.normalized;


        Debug.DrawRay(myPos, steeringVector.normalized*5f, UnityEngine.Color.cyan);

    

        {
            Vector3 vCBS = steeringVector.normalized;
            Vector3 vNav = VectorUtil.PlatVector(_agent.steeringTarget - _agent.nextPosition).normalized; // NavMesh 경로 진행 방향
                                                                                                          // 가중 합성
            float wNav = 0.1f, wCBS = 0.9f; // 상황에 맞게 조절
            Vector3 v = (wNav * vNav + wCBS * vCBS);
            if (v.sqrMagnitude > 1e-6f) v = vCBS.normalized * _agent.speed;
            if (bAvoidance)
            {
                _agent.ResetPath();
                _agent.velocity = v;
            }
        }



    }
    #endregion
    #region  another way

    // 대칭 깨기 함수
    private void ApplySymmetryBreaking(float[] interestMap, Vector3[] contextMap, Vector3 desiredVector)
    {
        // 좌우 방향 중 하나를 미세하게 선호
        float sidePreference = (Time.time % 2f < 1f) ? 0.1f : -0.1f;
        Vector3 rightVector = Vector3.Cross(Vector3.up, desiredVector); //비대칭 백터

        for (int i = 0; i < interestMap.Length; i++)
        {
            float dotRight = Vector3.Dot(contextMap[i], rightVector);
            interestMap[i] += dotRight * sidePreference; //비대칭 적용 (뺴거나 더하거나)
        }
    }

    // 가우시안 블러 (부드러운 전환)
    private void ApplyGaussianBlur(float[] map, int size)
    {
        float[] blurred = new float[size];
        float kernel = 0.25f; // 블러 강도

        //wrap 
        for (int i = 0; i < size; i++)
        {
            float sum = map[i] * (1f - 2f * kernel); //0.5
            int prev = (i - 1 + size) % size; //왼
            int next = (i + 1) % size;        //오 
            sum += map[prev] * kernel; //0.25
            sum += map[next] * kernel; //0.25
            blurred[i] = sum;
        }

        System.Array.Copy(blurred, map, size);
    }

    // 2차 관심 맵 (우회 경로)
    private void ApplySecondaryInterest(float[] interestMap, float[] dangerMap, Vector3[] contextMap, Vector3 desiredVector, int size)
    {
        // 목표 방향이 막혔을 때, 가장 가까운 안전한 방향 찾기
        float desiredAngle = Mathf.Atan2(desiredVector.z, desiredVector.x) * Mathf.Rad2Deg;
        if (desiredAngle<0) desiredAngle +=360;
            int desiredIndex = Mathf.RoundToInt(desiredAngle / (360f / size)) % size; // 앵글이 어느 인덱스에 속하는지 확인  

        if (dangerMap[desiredIndex] > 0.5f) // 목표 방향이 막힌 경우 (우회로가 막혔다 )
        {
            // 좌우로 탐색하여 가장 가까운 안전한 방향 찾기
            for (int offset = 1; offset < size / 2; offset++)
            {
                int leftIndex = (desiredIndex - offset + size) % size;// 왼쪽 찾기
                int rightIndex = (desiredIndex + offset) % size;      // 오른쪽 찾기

                // 안전한 방향에 보너스 부여
                if (dangerMap[leftIndex] < 0.1f)
                {
                    interestMap[leftIndex] += 0.3f;
                    break;
                }
                if (dangerMap[rightIndex] < 0.1f)
                {
                    interestMap[rightIndex] += 0.3f;
                    break;
                }
            }
        }
    }
    // 연속된 안전한 슬롯 찾기
    private int[] FindConsecutiveSafeSlots(float[] dangerMap, int size)
    {
       List<int> safeSlots = Enumerable.Repeat(0, size).ToList();
        safeSlots.Capacity = size;
        int consecutiveCount = 0;
        int startIndex = -1;

        int iSafeAreaLimit = 3; 

        for (int i = 0; i < size + iSafeAreaLimit; i++) //(원형 처리)
        {
            int index = i % size;

            if (!(dangerMap[index] > 0f))
            {
                if (startIndex == -1) startIndex = index;
                consecutiveCount++;
            }
            else
            {
                if (consecutiveCount >= iSafeAreaLimit ) // 3개 이상 연속된 안전 구역
                {
                    for (int j = 0; j < consecutiveCount; j++)
                    {
                        safeSlots[(startIndex + j) % size] =1;
                    }
                }
                consecutiveCount = 0;
                startIndex = -1;
            }
        }

        return safeSlots.ToArray();
    }
    // 안전한 슬롯에 포함되는지 확인
    private bool IsInSafeSlot(int index, int[] safeSlots)
    {
        if (index<0) return false;
        if (safeSlots.Length<=index) return false; 

        index = index % safeSlots.Length;

       if( 1==safeSlots[index]) return true;

        return false;
    }
    #endregion


    #region Follwing Wall
    // 목표 방향이 막혔는지 확인하는 함수
    bool IsTargetDirectionBlocked(Vector3 startPos, Vector3 targetDirection, out Vector3 wallNormal, out float hitDistance)
    {
        wallNormal = Vector3.zero;
        hitDistance = 0f;

        float radius = 0.2f;//0.6

        float checkDist =  0.3f*lookAheadDistance*2 *1.5f;//3f
        UnityEngine.Color debugCol = UnityEngine.Color.yellow;
        // 목표 방향으로 레이캐스트
        if (Physics.SphereCast(startPos, radius, targetDirection.normalized, out RaycastHit hit, checkDist, LayerMask.GetMask("Enemies")))
        {
            //if (hit.collider.gameObject != this.gameObject)
            {
                wallNormal = hit.normal;
                hitDistance = hit.distance;

                // 벽이 너무 가까우면 막힌 것으로 판정
                if (hitDistance < wallDistance * 2f)
                {
                    Debug.DrawRay(startPos, wallNormal * hit.distance*10f, UnityEngine.Color.purple, 0.1f);
                    debugCol = UnityEngine.Color.magenta;
                    _arrDebugCol.Add(new SphereDebug(startPos + targetDirection.normalized* checkDist, radius, debugCol, 0.5f));
                    return true;
                }
            }
           
        }

        else if (Physics.SphereCast(startPos, radius*1.2f, transform.forward.normalized, out hit, checkDist*2f, LayerMask.GetMask("Enemies")))
        {
            //if (hit.collider.gameObject != this.gameObject)
            {
                wallNormal = hit.normal;
                hitDistance = hit.distance;

                // 벽이 너무 가까우면 막힌 것으로 판정
                if (hitDistance < wallDistance * 2f)
                {
                    Debug.DrawRay(startPos, wallNormal * hit.distance*10f, UnityEngine.Color.brown, 0.1f);
                    debugCol = UnityEngine.Color.brown;
                    _arrDebugCol.Add(new SphereDebug(startPos + targetDirection.normalized* checkDist*2f, radius, debugCol, 0.5f));
                    return true;
                }
            }

        }
        _arrDebugCol.Add(new SphereDebug(startPos + transform.forward.normalized* checkDist, radius, debugCol, 0.5f));


        return false;
    }

    // 벽 따라가기 Interest 계산
    float CalculateWallFollowingInterest(Vector3 direction, Vector3 targetDirection, Vector3 wallNormal)
    {

        Vector3 bestTangent = GetWallFollowingDirection(wallNormal,targetDirection);

        // 각 방향에 대한 관심도 계산
        float wallFollowInterest = Mathf.Clamp01(0.5f + 0.5f * Vector3.Dot(direction, bestTangent)) * wallFollowWeight;
        float targetPullInterest = Mathf.Clamp01(0.5f + 0.5f * Vector3.Dot(direction, targetDirection)) * targetPullWeight;

        // 벽에서 너무 멀어지지 않도록 하는 관심도
        float wallProximityInterest = 0.2f;
        if (Vector3.Dot(direction, -wallNormal) > 0.7f) // 벽 쪽으로 향하는 방향
        {
            wallProximityInterest = 0.2f;
        }

        return Mathf.Clamp01(wallFollowInterest + targetPullInterest + wallProximityInterest);
    }

    // 강제 벽 따라가기 방향 계산
    Vector3 GetWallFollowingDirection(Vector3 wallNormal, Vector3 targetDirection)
    {
        // 벽의 접선 방향들
        Vector3 wallTangent1 = Vector3.Cross(wallNormal, Vector3.up).normalized;
        Vector3 wallTangent2 = -wallTangent1;

        //목표 방향과 더 가까운 접선 방향 선택
        Vector3 bestDirection = Vector3.Dot(wallTangent1, targetDirection) < Vector3.Dot(wallTangent2, targetDirection)
            ? Quaternion.Euler(0, -10, 0)*wallTangent1 : Quaternion.Euler(0,10,0)*wallTangent2;

        Vector3 upOffset = Vector3.up * 0.3f;

        float[] DistanceTan = { 5f, 5f }; 

        var hit = Physics.RaycastAll(transform.position+upOffset, wallTangent1, 5f, LayerMask.GetMask("Enemies","Player"), QueryTriggerInteraction.Ignore);
        
        foreach (var h in hit)
        {
            if (h.collider.gameObject != this.gameObject)
            {
                if (DistanceTan[0]>h.distance)
                    DistanceTan[0] = h.distance;
                //break; 
            }
        }
            
        
        hit = Physics.RaycastAll(transform.position+upOffset, wallTangent2, 5f, LayerMask.GetMask("Enemies", "Player"), QueryTriggerInteraction.Ignore);

        foreach (var h in hit)
        {
            if (h.collider.gameObject != this.gameObject)
            {
                if (DistanceTan[1]>h.distance)
                    DistanceTan[1] = h.distance;
                //break;
            }
        }
        bestDirection = DistanceTan[0] < DistanceTan[1] ? wallTangent2 : wallTangent1; //장애물이 없는 곳 위주로 달린다(잡선이지만). 
     

        Debug.DrawRay(transform.position+upOffset, bestDirection.normalized*5f, UnityEngine.Color.yellow, 0.1f);
        return bestDirection;

    }
    #endregion

    #region Concave Case
    // 갇힌 상태 감지 (핑퐁 현상)
    bool DetectStuckState()
    {
        Vector3 currentPosition = transform.position;

        if (Vector3.Distance(currentPosition, lastPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = currentPosition;
        }

        return stuckTimer > 2f; // 2초간 거의 움직이지 않으면 갇힌 것으로 판정
    }
    #endregion

#region Check Attack Condition
        public void CheckAttackCondition()
    {
        if (null == _monsterAttack) return;

        _monsterAttack.UpdateAttack();
        
    }
    #endregion


    private void ReleaseGoalPoint()
    {
        if(null != _goalPoint)
        {
            if (_querier == _goalPoint.occupant)
                _goalPoint.occupant =null;

            if (_querier == _goalPoint.reserver)
                _goalPoint.reserver =null;
        }
    }
 
    private void ReleaseReservedPoint()
    {
        if(null != _reservedPoint)
        {
            _reservedPoint.reserver = null;
            _reservedPoint =null; 
        }
    }
    private void ReleaseOccupyingPoint()
    {
        if(null != _querier.OccupingPoint)
        {
            if(true ==_querier.OccupingPoint.isOccupied)
            {
                if( _querier.OccupingPoint.occupant ==_querier)
                {
                    _querier.OccupingPoint.occupant =null;
                }
            }
        }
    }
    private void ReservePoint(EQSPoint point)
    {
        /*Clear*/
        if(null!= _reservedPoint && _reservedPoint != point )
        {
            _reservedPoint.reserver = null; //나 리저브 푼다.
        }
        /*Do*/
        point.reserver = _querier;
        _reservedPoint=  point;
        /*이거 중요 */
        _lastReservationTime =Time.time; 

    }
    private void OccupyPoint(EQSPoint point)
    {
        if (_querier == point.occupant) return; 

            /*Clear*/
        ReleaseOccupyingPoint();
        if (null!= _querier?.OccupingPoint)
        {
            _querier.OccupingPoint.occupant =null;
        }
        /*Do*/
       
        point.occupant= _querier;
        point.reserver = null; 
        _querier.OccupingPoint = point;
        _reservedPoint = null;
        _goalPoint = null;

       
    }
    /* 주기적으로 BestPoint를 Test하고 얻어와 Goal 갱신 */
    public void QueryNewPoint()
    {
        if (false==gameObject.activeSelf) return;
        
        var context = new EQSContext
        {
            target = EQSManager.GetInstance().GetTarget().transform,
            allQueriers = EQSManager.GetInstance().GetAllQueriers(),
            points = EQSManager.GetInstance().GetPoints()
        };
        var bestPoint =  _query.ExecuteQuery(_querier, context.points,context);

        if (null== bestPoint) return;

        if ((int)_ZoneID !=NavMeshZoneFilter.GetZoneIDAt(bestPoint.pos))
        {
            ReleaseOccupyingPoint();
            ReleaseGoalPoint();
            ReleaseReservedPoint(); return;

            /*Patrol 상태로 전환-> Spawner에게 SetDestination 호츌하라고 함 Spawner 가 Dictionary로 ID를 가지고 있음,enemyID ==IDX 딕셔너리로 관리 (에네미PreFab-> GetID-> Dictionary 등록)*/
        }

        ReleaseOccupyingPoint();
        if (bestPoint!= _goalPoint )
        {
            
            ReleaseGoalPoint();
            ReservePoint(bestPoint);
            _goalPoint = bestPoint;
        }
       
        MoveToGoal();
    }

    private void MoveToGoal()
    {
        
            
        if (null == _goalPoint) return;
       
        _agent.SetDestination(_goalPoint.pos);
    }
    #region 디버그
    private void OnDrawGizmos()
    {
        /*그려야 할 것 GoalPointPos*/
        if(null!= _goalPoint)
        {
            Gizmos.color= new UnityEngine.Color(0,0,1,1);
            Gizmos.DrawSphere( _goalPoint.pos,0.1f);
        }
        /*Goal까지의 경로*/
        /*s나 위치*/
        


        /*예약 Point*/
        if (null!= _reservedPoint)
        {
            Gizmos.color= new UnityEngine.Color(1, 0, 1, 1f);//purple
            Gizmos.DrawSphere(_reservedPoint.pos, 0.1f);
        }

        if (null == _querier) return;
        Vector3 MyPos = _querier.transform.position;
        NavMeshPath _path = _agent.path;
        if (_path.corners.Length>0)
        {
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawLine(MyPos, _path.corners[0]);
            for (int i = 0; i<_path.corners.Length -1; ++i)
            {
                Gizmos.DrawLine(_path.corners[i], _path.corners[i+1]);
            }
        }

        foreach(var sphere in _arrDebugCol)
        {
            Gizmos.color = sphere.color;
            Gizmos.color = new UnityEngine.Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, sphere.alpha);
            Gizmos.DrawSphere(sphere.pos, sphere.radius);
        }



    }
    #endregion
}
