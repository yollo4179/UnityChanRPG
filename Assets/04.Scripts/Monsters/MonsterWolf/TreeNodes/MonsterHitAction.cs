using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterHitAction", story: "MonsterHitAction", category: "Action", id: "4da0adaa40eb19f4333b60826c5ed6d6")]
public partial class MonsterHitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyStatus;

 
    Animator _animator;
    StatusScript _status;
    int _hitAnimFullpathHash;

    bool _hasInitialized = false; 
    protected override Status OnStart()
    {
        //every time
        if(_animator) _animator.SetTrigger("OnHit");

        if (true == _hasInitialized) return Status.Running; 
        _hasInitialized =true; 

        //once
        _animator = _self.Value.GetComponent<Animator>();
        _status = _self.Value.GetComponent<StatusScript>();
        _hitAnimFullpathHash  = Animator.StringToHash("Base Layer.hit");
        _animator.SetTrigger("OnHit");
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        if (eEnemyState.DEAD==_enemyStatus.Value)
        {
            return Status.Success; //업데이트 중에 죽었다. -> 브랜치 끊고 상태 전환
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


