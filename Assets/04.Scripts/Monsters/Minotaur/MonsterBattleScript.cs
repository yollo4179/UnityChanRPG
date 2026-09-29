
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;



public class MonsterBattleScript : MonoBehaviour
{
    public class DebugSet
    {
        public Ray ray;
        public bool isInterested;
    }
    DebugSet[] _debugLines;
    
    [SerializeField] int _numRays = 16;
    [SerializeField] float  _lengthRay = 5f;
    [SerializeField] float _baseHeight= 1f;
    [SerializeField] float _capsuleR = 0.5f;
    protected Transform _target;
    protected Vector3 _offset  = Vector3.zero;
    protected LayerMask _obstacleMask; 

    private float[] _interestMap;
    private float[] _interestMap_L;
    private float[] _interestMap_R;
    private float[] _dangerMap;
    [SerializeField] protected float _rangeOffset = 5f;
    [SerializeField] protected float _monsterSpeed = 3f;
    protected float _baseSpeed;
    protected NavMeshAgent _agent;
    protected Animator _animator; 
    protected Vector3 _nowLookDir= Vector3.zero;
     

    protected float _animatorVal_H = 0f;
    protected float _animatorVal_V = 0f;

    protected MonsterAttackPrerequisiteChecker _attackChecker;
    public enum eNowState
    {
        MoveB,
        Strafe,
        MoveF,
        Stop,
    }
    public enum eStrafeDirection
    { 
        strafe_Right, 
        strafe_Left,
        strafe_End
    }
    eStrafeDirection _strafeDir = eStrafeDirection.strafe_Left ; 

    eNowState _fromState;
    eNowState _toState;
    protected eNowState _nowState;
    private float _blendStartTime = 0f;
    bool _isBlending = false;
    float _lerpTime = 0.2f;
    public virtual void Awake()
    {
        _debugLines = new DebugSet[_numRays];
        for (int i=0; i<_numRays; ++i) {
            _debugLines[i] = new DebugSet();
        }
        _interestMap = new float[_numRays];
        _interestMap_L = new float[_numRays];
        _interestMap_R = new float[_numRays];

        _dangerMap = new float[_numRays];

        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _offset.y = _baseHeight;

        _obstacleMask= LayerMask.GetMask("Enemies", "Obstacles");

        _nowLookDir = transform.forward;
        _animator  = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        //_agent.enabled = false;
        _attackChecker = GetComponent<MonsterAttackPrerequisiteChecker>();
        _baseSpeed = _monsterSpeed;
    }

    public virtual void UpdateBattle(bool isFirst)
    {

        Vector3 toTarget = VectorUtil.PlatVector(_target.transform.position - transform.position);
        float lengthToTargetOnXZ = toTarget.magnitude;
        toTarget.Normalize();
        Vector3 targetDir = CalculateInterests(lengthToTargetOnXZ, toTarget, isFirst);

        _nowLookDir = Vector3.RotateTowards(_nowLookDir, toTarget, Time.deltaTime*3f, Time.deltaTime*5f);
        UpdateMovement(targetDir, _nowLookDir);
        ChooseAnimation(toTarget);

        _attackChecker.CheckAttackCondition();
    }
    public Vector3 CalculateInterests(float lengthToTargetOnXZ, Vector3 toTarget,bool isFirst = true)
    {

        Vector3 interestSum = Vector3.zero;


        for (int i = 0; i< _numRays; i++)
        {

            Vector3 axis = transform.forward.normalized;
            axis = Quaternion.Euler(0, i * 360f/_numRays, 0)* axis;
            axis=axis.normalized;

            Ray ray = new Ray();
            ray.origin = transform.position +_offset;
            ray.direction = axis;

            float dotProduct = Vector3.Dot(toTarget, axis);

            if (isFirst)
            {
                _nowState= _toState =_fromState;
                _blendStartTime = 0f;
                _isBlending =false;
            }            
            
            if (_isBlending)
            {
                _blendStartTime = Mathf.Lerp(_blendStartTime, _lerpTime, Time.deltaTime);

                if (Mathf.Abs(_lerpTime-_blendStartTime)<0.01f)
                {
                    _blendStartTime = 0f;
                    _isBlending =false;
                    _nowState = _toState;
                }
            }
            else {
                if (lengthToTargetOnXZ<_rangeOffset)
                {
                    _animatorVal_V = -1;

                    _interestMap[i]= 0.5f*(1-dotProduct); //뒤로 
                    _toState = eNowState.MoveB;
                }
                else if (_rangeOffset  <= lengthToTargetOnXZ  &&lengthToTargetOnXZ<=_rangeOffset+2f)
                {
                    _interestMap[i] = 2*(1 - Mathf.Abs(dotProduct));//뱅글
                    _toState = eNowState.Strafe;
                }
                else
                {
                    _interestMap[i] =  0.5f*(dotProduct+1f); //쫒
                    _toState = eNowState.MoveF;
                }
                if (_fromState !=_toState)
                {
                    _isBlending  = true;

                }
                _fromState = _toState;
            }

            _dangerMap[i] = 0f;
            if (Physics.Raycast(ray, out RaycastHit hit, _lengthRay, _obstacleMask, QueryTriggerInteraction.Ignore))
            {
                _interestMap[i] = (hit.distance-_capsuleR)/(_lengthRay-_capsuleR);
                _dangerMap[i]=1f;
            }

            ray.direction =_interestMap[i]*axis;
            _debugLines[i].ray = ray;
            _debugLines[i].isInterested =(0>=_dangerMap[i] );
            _interestMap[i] =(0<_interestMap[i]) ? _interestMap[i] : 0f;


            interestSum += _interestMap[i] *axis;
        }
        _monsterSpeed =_baseSpeed;
        if (_nowState == eNowState.Strafe)
        {
            interestSum = Strafe(toTarget).normalized;
            _monsterSpeed =_baseSpeed*0.3f;
        }
        if (_nowState == eNowState.MoveB)
        {

            _monsterSpeed =_baseSpeed*0.5f;
        }
        return interestSum.normalized;
    }
    public Vector3 Strafe(Vector3 toTarget)
    {

        Vector3 leftGo = Vector3.zero;
        Vector3 rightGo = Vector3.zero;
        if (_nowState == eNowState.Strafe)
        {
            Vector3 rightDir = Quaternion.Euler(0, 90, 0) * toTarget.normalized;
            Vector3 leftDir = Quaternion.Euler(0, -90, 0) * toTarget.normalized;

            float bestRight = 0f;
            float bestLeft = 0f;
            float bestBoth = 0f;

            for (int i = 0; i < _numRays; i++)
            {
                Vector3 axis = Quaternion.Euler(0, i * 360f/_numRays, 0) * transform.forward;
                axis.Normalize();

                float w = Mathf.Max(0f, _interestMap[i]);         // 위험 반영 후의 실제 가중치
                if (w > bestBoth) bestBoth = w;

                // axis가 오른쪽/왼쪽 반평면에 있는지 판정
                float sideRight = (1+Vector3.Dot(axis, rightDir));
                float sideLeft = (1+Vector3.Dot(axis, leftDir));

                _interestMap_L[i]= 0;
                _interestMap_R[i]= 0;

                if (sideRight > 0.5f && w > bestRight) { bestRight = w; _interestMap_R[i]=w; }
                if (sideLeft  > 0.5f && w > bestLeft) { bestLeft  = w; _interestMap_L[i]=w; }

                leftGo +=_interestMap_L[i]*axis;
                rightGo +=_interestMap_R[i]*axis;
            }

            float diffIfRight = bestBoth - bestRight;
            float diffIfLeft = bestBoth - bestLeft;

            const float SWITCH_THRESH = 0.75f; 
                                              
            if (_strafeDir == eStrafeDirection.strafe_Right)
            {
                // 반대쪽(Left)이 현저히 이득이면 전환
                if (diffIfLeft >= SWITCH_THRESH)
                {
                    _strafeDir = eStrafeDirection.strafe_Left;
                   // _sideLockUntilTime = Time.time + 0.2f; // (선택) 쿨다운으로 재뒤집힘 방지

                }
            }
            else // Left 잠금 중
            {
                if (diffIfRight>= SWITCH_THRESH)
                {
                    _strafeDir = eStrafeDirection.strafe_Right;
                  //  _sideLockUntilTime = Time.time + 0.2f;
                }
            }
            switch (_strafeDir)
            {
                case eStrafeDirection.strafe_Right:
                    return leftDir;//leftGo;

                case eStrafeDirection.strafe_Left:
                    return rightDir;//rightGo;


            }
        }
        

        return Vector3.zero;
    }
    public virtual void ChooseAnimation(Vector3 toTargetDir)
    {
        _animatorVal_H = 0f;
        _animatorVal_V = 0f;
        switch (_nowState)
        {
            case eNowState.MoveB: {
                    _animatorVal_V= -1; 
                    break;
                }
            case eNowState.MoveF: {
                    _animatorVal_V= 1;

                    break; 
                }
            case eNowState.Strafe: {
                    Vector3 dir = Vector3.Cross(transform.forward, toTargetDir);
                    _animatorVal_H = 1;//왼쪽
                    if (0<dir.y)//오른쪽 
                    {
                        _animatorVal_H = -1; 
                    }
                    break; 
                }
            default:
                { 
                    
                    break; 
                }

        }
        //TODO 해시러 캐싱

        float _target_H = Mathf.Lerp(_animator.GetFloat("Horizontal"), _animatorVal_H,Time.deltaTime*3f);
        float _target_V = Mathf.Lerp(_animator.GetFloat("Speed"), _animatorVal_V, Time.deltaTime*3f);
        _animator.SetFloat("Horizontal", _target_H);
        _animator.SetFloat("Speed", _target_V);
    }
    
    public virtual void UpdateMovement(Vector3 direction, Vector3 lookDir)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        {
            // 직접 이동
            transform.position += direction * _monsterSpeed * Time.deltaTime;

            // 부드러운 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }   
        
    }
    
    private void OnDrawGizmos()
    {
        foreach (var debug in _debugLines)
        {
            Gizmos.color = Color.green;
            if (false == debug.isInterested)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(debug.ray.origin, debug.ray.origin+ debug.ray.direction);

        }


    }
}
