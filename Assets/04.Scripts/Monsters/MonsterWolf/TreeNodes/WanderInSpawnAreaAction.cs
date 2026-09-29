using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WanderInSpawnArea", story: "[Self] wanders toward one of the points on the spawn area based on their weight", category: "Action", id: "a5e56fca26eae8c94d67ce69c221ac64")]
public partial class WanderInSpawnAreaAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;
    /*SpawnTile을 가지고 있어야 한다 (컴포넌트로) ->EQSPoints를 가지고 와야함*/

    StatusScript _status;
    #region /*런타임 주입*/
    [SerializeReference] public BlackboardVariable<GameObject> Spawner;//Manager
    [SerializeReference] public BlackboardVariable<int> CellIndex;
    [SerializeReference] public BlackboardVariable<bool> isTargetDetected;

    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;
    #endregion
    #region 런타임 주입 이후 , 오브젝트로부터 얻어온 것들
    SpawnTile _SpawnTile;
    EQSPoint _goalPoint;
    NavMeshAgent _agent;
    #endregion
    #region Self로부터 얻어온 컴폰너트 
    EQSQuerierBehavior_WanderAroundCell _querierBehavior;
    Animator _animator;  float _speed = 0; 
    #endregion
    public float _maxWanderingDuration =5f;
    public float _currentWanderingTime=0f; 

    protected override Status OnStart()
    {
        _agent  = _self.Value.GetComponent<NavMeshAgent>();
        _agent.speed = 1.5f;
        _querierBehavior = _self.Value.GetComponent<EQSQuerierBehavior_WanderAroundCell>();
        _animator = _self.Value.GetComponentInChildren<Animator>();
        _animator.SetTrigger("OnWander");
        //Self.Value
        if (null != Spawner)
        {
            _SpawnTile= Spawner.Value.GetComponent<EnemySpawner>().GetSpawnTileByIndex(CellIndex);
        }
      
        /*점 체킹*/
        _goalPoint = _querierBehavior.GetBestPoint();
        if (null != _goalPoint) _agent.SetDestination(_goalPoint.pos);
        _currentWanderingTime =Time.time;

        _status= _self.Value.GetComponent<StatusScript>();

        return Status.Running;
    }
   
    protected override Status OnUpdate()
    {
        /*테스트 수행 후 Best 옵션 설정*/
        if (eEnemyState.DEAD==_enemyState)
        {
            return Status.Success; //업데이트 중에 죽었다. -> 브랜치 끊고 상태 전환
        }
        if (true == _status.GotHit)
        {
            _enemyState.Value = eEnemyState.HIT;
            return Status.Success; //맞았을 때
        }
        /*Best 옵션 구한 후에 Set Destination 설정*/
        if (null != _goalPoint)
        {
            _speed = Mathf.Lerp(_animator.GetFloat("Speed"), _agent.velocity.magnitude, Time.deltaTime * 5);
            _animator.SetFloat("Speed", _speed );

            if (DoArrivalTest() ||DoDurationTest()||DoDetectionTest())
            {
                _agent.velocity=Vector3.zero;
                _agent.ResetPath();
           
                return Status.Success;
            }
        }
        return Status.Running;
      
    }
    protected override void OnEnd()
    {
        _agent.speed=3;
      

    }
    bool DoArrivalTest()
    {
        return (_goalPoint.pos - _self.Value.transform.position).sqrMagnitude <(0.01f);
    }
    bool DoDurationTest()
    {
        return _maxWanderingDuration < Time.time -_currentWanderingTime;
    }
    bool DoDetectionTest()
    {
        return isTargetDetected.Value;
    }
}

