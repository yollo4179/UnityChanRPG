using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "Chase EQSItem areound target , Waiting for Attack after occupancy", category: "Action", id: "d16981a9249636ed278c1050a8cfa492")]
public partial class ChaseAction : Action
{
    [SerializeReference]  public BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;  
    StatusScript _status;
    #region  ChaseBehavior From  Self 
    EQSQuerierBehaviour_Battle _chaseBehabior;
    EQSQuerier _eqsQuerier;
    Animator _animator; 
    NavMeshAgent _agent;
    #endregion

    protected override Status OnStart()
    {
        _chaseBehabior = _self.Value.GetComponent<EQSQuerierBehaviour_Battle>();
        _eqsQuerier =_self.Value.GetComponent<EQSQuerier>();
        _eqsQuerier.IsChasing = true;
        EQSManager.GetInstance().AddChasingQuerier(_eqsQuerier);

        _animator  = _self.Value.GetComponentInChildren<Animator>();
        _animator.SetTrigger("OnBattle");
        _agent  = _self.Value.GetComponent<NavMeshAgent>();

        _status= _self.Value.GetComponent<StatusScript>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        int hash =Animator.StringToHash("Base Layer.BattleLocomotion");
        if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash!= hash)
            _animator.SetTrigger("OnBattle");

        if (eEnemyState.DEAD ==_enemyState)
            {
                return Status.Success;
            }
            if (true == _status.GotHit)
            {
                _enemyState.Value = eEnemyState.HIT;
                return Status.Success; //맞았을 때
            }

            Debug.Assert(null != _chaseBehabior, "ChaseBehavior is null");

            _chaseBehabior?.BattleBehavior();
          
            
        float curSpeed = Mathf.Lerp(_animator.GetFloat("Speed"), _agent.velocity.magnitude, 6*Time.deltaTime);
        _animator.SetFloat("Speed", curSpeed);
        if (eEnemyState.ATTACK ==_enemyState)
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
    
            default:
            {
                _enemyState.Value = eEnemyState.CHASE;
                break;
            }
        }
    }
}

