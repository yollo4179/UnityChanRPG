using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterBattleAction", story: "Monster in battleAction", category: "Action", id: "6bbaa83b27fbe864ff145c5d763dacde")]
public partial class MonsterBattleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;

    public MonsterBattleScript _battleScript;
    StatusScript _status;
    

    #region  ChaseBehavior From  Self 
    Animator _animator;
    NavMeshAgent _agent;
    int _hash;

    bool isFirst = true;
    #endregion
    protected override Status OnStart()
    {
        _battleScript = _self.Value.GetComponent<MonsterBattleScript>();

        _animator  = _self.Value.GetComponentInChildren<Animator>();
        _animator.SetTrigger("OnBattle");
        _agent  = _self.Value.GetComponent<NavMeshAgent>();
        _agent.enabled = false; 
        _status = _self.Value.GetComponent<StatusScript>();
        _hash = Animator.StringToHash("Base Layer.BattleLocomotion");
        isFirst = true;
        return Status.Running;

    }

    protected override Status OnUpdate()
    {
      

        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash!= _hash

            /*&&!_animator.IsInTransition(0)*/)
        {
            _animator.SetTrigger("OnBattle");
            return Status.Running;

        }
        if (eEnemyState.DEAD ==_enemyState)
        {
            return Status.Success;
        }
        if (true == _status.GotHit)
        {
            _enemyState.Value = eEnemyState.HIT;
            return Status.Success; //맞았을 때
        }

        Debug.Assert(null != _battleScript, "ChaseBehavior is null");

        _battleScript?.UpdateBattle( isFirst);

        isFirst=false;

        if (eEnemyState.ATTACK ==_enemyState)
        {
            return Status.Success;
        }
        if (eEnemyState.FLY ==_enemyState)
        {
            return Status.Success;
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
            case eEnemyState.ATTACK:
                {
                    _enemyState.Value = eEnemyState.ATTACK;
                    break;
                }
            case eEnemyState.FLY:
                {
                    _enemyState.Value = eEnemyState.FLY;
                    break;
                }
            default:
                {
                    _enemyState.Value = eEnemyState.CHASE;
                    break;
                }
        }
    }
}

