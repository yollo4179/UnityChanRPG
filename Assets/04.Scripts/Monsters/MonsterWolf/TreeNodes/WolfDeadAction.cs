using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WolfDead", story: "Agent dead after get hit", category: "Action", id: "709e609f89797015899e6aa26a79efbc")]
public partial class WolfDeadAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;
   
    
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;
    [SerializeReference] public BlackboardVariable<float> _deadDuration;
     Animator _animator;
    ShaderEffects _shader; 
    int _deadTriggerHash;

    float _deadTime; 
    [SerializeReference]public float _respawnDuration ;

    private NavMeshAgent _agent;


    #region  ChaseBehavior From  Self 
    EQSQuerierBehaviour_Battle _chaseBehabior;
    EQSQuerier _eqsQuerier;
    #endregion

    StatusScript _status;


    //Todo DamageReciever에서 죽었을때 Dead상태로 처리 콜라이더 모두 끄기 Respawn시 리셋함수 만들기 
    protected override Status OnStart()
    {
        /*Animation 처리*/
        _deadTriggerHash= Animator.StringToHash("OnDead");
        _animator = _self.Value.GetComponent<Animator>();
        _animator.SetTrigger(_deadTriggerHash);
        /*Shader처리*/
        _shader = _self.Value.GetComponent<ShaderEffects>();
        _shader.SetNowMarerial(eShaderEffect.DISOLVE);
        _shader.DoFade(1.3f, -0.3f, 8.5f,eFadeMode.FADE_OUT);

        /*Ready Logic */
        _deadTime = Time.time;
        /**/
        /*From Any State to DeadState */

        /*공격 콜라이더는 예외 처리*/
        Collider[] col = _self.Value.GetComponentsInChildren<Collider>();
        foreach (Collider c in col)
        {
            c.enabled =false;
        
        }
        _agent = _self.Value.GetComponent<NavMeshAgent>();


        _chaseBehabior = _self.Value.GetComponent<EQSQuerierBehaviour_Battle>();
        _eqsQuerier =_self.Value.GetComponent<EQSQuerier>();
        _eqsQuerier.IsChasing = false;
        EQSManager.GetInstance().DeleteChasingQuerier(_eqsQuerier);

        _status = _self.Value.GetComponent<StatusScript>();
        _status.IsDead = true;
        _status.GotHit = false;
        
        Event_KillTarget event_KillTarget = new Event_KillTarget(_status.CharacterID);
        Managers.Event.Publish<Event_KillTarget>(event_KillTarget);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent.velocity.magnitude>0.01f)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }
        /*죽음 상태에 있다가 ..*/
        if (_deadDuration.Value <=Time.time  - _deadTime)
            return Status.Success;
        
        return Status.Running;
    }

    protected override void OnEnd()
    {


        /*Respawn 시작*/
        //리스폰 상태로 진입
        _enemyState.Value=eEnemyState.RESPAWN;
    }
}

