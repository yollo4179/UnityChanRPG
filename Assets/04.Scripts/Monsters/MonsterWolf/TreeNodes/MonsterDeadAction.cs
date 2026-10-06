using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterDeadAction", story: "MonsterDead", category: "Action", id: "a169aaa80d43184c94c01025e1b1f9f6")]
public partial class MonsterDeadAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> _self;


    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;
    [SerializeReference] public BlackboardVariable<float> _deadDuration;
    Animator _animator;
    ShaderEffects _shader;
    int _deadTriggerHash;
    int _deathHash;

    float _deadTime;
    bool _hasInitialized = false; 
    [SerializeReference] public float _respawnDuration;
    Collider[] _col; 
    DetectPlayer _detectPlayer = null;


    StatusScript _status;
    protected override Status OnStart()
    {
        /*Animation 贸府*/

        if (null ==_shader) _shader = _self.Value.GetComponent<ShaderEffects>();
        if (null ==_animator) _animator = _self.Value.GetComponent<Animator>();
        if (null ==_col) _col = _self.Value.GetComponentsInChildren<Collider>();
        if (null ==_status) _status = _self.Value.GetComponent<StatusScript>();
        if(null == _detectPlayer) _detectPlayer =  _self.Value.GetComponent<DetectPlayer>();

        if (false ==_hasInitialized)
        {
            _deathHash = Animator.StringToHash("Base Layer.death");
            if (!_animator.HasState(0, _deathHash))
                _deathHash = Animator.StringToHash("Base Layer.Die");
            _deadTriggerHash = Animator.StringToHash("OnDead");
            _hasInitialized = true;
        }
        _animator.ResetTrigger("OnBattle");
        _animator.ResetTrigger("OnWander");
        _animator.ResetTrigger("OnHit");
        if (_animator.HasState(0, _deathHash))
            _animator.Play(_deathHash, 0, 0f);
        else
            _animator.SetTrigger(_deadTriggerHash);
        NavMeshAgent agent = _self.Value.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
        /*Shader贸府*/
        _shader.SetNowMarerial(eShaderEffect.DISOLVE);
        _shader.DoFade(1.3f, -0.3f, 8.5f, eFadeMode.FADE_OUT);

        _deadTime = Time.time;

        foreach (Collider c in _col)
        {
            c.enabled =false;

        }
        
        
        _status.IsDead = true;
        _status.GotHit = false;
        Event_KillTarget event_KillTarget = new Event_KillTarget(_status.CharacterID);
        Managers.Event.Publish<Event_KillTarget>(event_KillTarget);
       

       
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Missing or differently named animation states must not block respawning forever.
        return Time.time - _deadTime >= _deadDuration.Value ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        _detectPlayer.DisableTargetDetector();
        _enemyState.Value=eEnemyState.RESPAWN;
    }
}

