using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WolfGetHit", story: "agent get hit and chase after getting hit", category: "Action", id: "dc6462439b234c11a3abe559a7dfacb1")]
public partial class WolfGetHitAction : Action
{
    [SerializeReference]public  BlackboardVariable<GameObject> _self;
    [SerializeReference]public  BlackboardVariable<eEnemyState> _enemyStatus;

    NavMeshAgent _agent;
    Animator _animator;
    StatusScript _status;
    int _hitAnimFullpathHash;
    protected override Status OnStart()
    {
        _agent = _self.Value.GetComponent<NavMeshAgent>();
        _animator = _self.Value.GetComponent<Animator>();
        _animator.SetTrigger("OnHit");
        _hitAnimFullpathHash  = Animator.StringToHash("Base Layer.Hit");
        _status = _self.Value.GetComponent<StatusScript>(); 
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (eEnemyState.DEAD==_enemyStatus.Value)
        {
            return Status.Success; //업데이트 중에 죽었다. -> 브랜치 끊고 상태 전환
        }
        if (_agent.velocity.sqrMagnitude >=0.1f)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        if (!_animator.IsInTransition(0)&&
            _animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _hitAnimFullpathHash
            )
        {
            _animator.SetTrigger("OnHit");
        }

        if (_hitAnimFullpathHash ==_animator.GetCurrentAnimatorStateInfo(0).fullPathHash 
            && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _status.GotHit = false;
        switch (_enemyStatus.Value)
        {
            case eEnemyState.DEAD:
            {
                _enemyStatus.Value = eEnemyState.DEAD; 
                break; 
            }
             default:
            {
                 _enemyStatus.Value = eEnemyState.CHASE; 
                 break; 
            }
        }
       
    }
}

