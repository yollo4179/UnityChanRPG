using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Idle", story: "Idle : stop and Set Animator Speed Value , lerping", category: "Action", id: "dad3448d0e53551d8cf7e7886134f2f3")]
public partial class IdleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;

    [SerializeReference] public BlackboardVariable<float> _idleTimeRangeStart;
    [SerializeReference] public BlackboardVariable<float> _idleTimeRangeEnd;

    StatusScript _status;
    Animator _animator;
    float _targetSpeed = 0;

    float _startTime= 0;
    float _idleDuration = 0;
    protected override Status OnStart()
    {   
        _animator = _self.Value.GetComponent<Animator>();
        _animator.SetTrigger("OnWander");
        _status= _self.Value.GetComponent<StatusScript>();

        _idleDuration= UnityEngine.Random.Range(_idleTimeRangeStart.Value, _idleTimeRangeEnd.Value);
        _startTime =Time.time; 

        return Status.Running;
    }

    protected override Status OnUpdate()
    {


        if (eEnemyState.DEAD==_enemyState)
        {
            return Status.Success; //업데이트 중에 죽었다. -> 브랜치 끊고 상태 전환
        }
        if(true == _status.GotHit)
        {
            _enemyState.Value = eEnemyState.HIT;
            return Status.Success; //맞았을 때
        }
        /*Anim Lerp*/
        float curSpeed = _animator.GetFloat("Speed");
        float speed = Mathf.Lerp(curSpeed, _targetSpeed, Time.deltaTime * 5);
        _animator.SetFloat("Speed", speed);
        if(Mathf.Abs (_targetSpeed - speed) <0.01f)
        {
            if (_idleDuration< Time.time -_startTime)
            {
                return Status.Success; //대기 시간이 다 됐을 때
            }
        }
        

        return Status.Running;
    }

    protected override void OnEnd()
    {
        switch (_enemyState.Value)
        {
            case eEnemyState.DEAD:
                {
                    _enemyState.Value = eEnemyState.DEAD;
                    break;
                }
            case eEnemyState.HIT:
                {
                    _enemyState.Value = eEnemyState.HIT;
                    break;
                }
            default:
                {
                    _enemyState.Value = eEnemyState.WANDER;
                    break;
                }
        }
    }
}

