using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterRespawn", story: "MonsterRespawnAction", category: "Action", id: "ef7b5d067df29543f9082fa06de4986c")]
public partial class MonsterRespawnAction : Action
{
    /*Self*/
    [SerializeReference] public BlackboardVariable<GameObject> _self;
    /*Animator*/
    [SerializeReference] public Animator _animator;
    [SerializeReference] public ShaderEffects _effects;
    [SerializeReference] public BlackboardVariable<eEnemyState> _state;

    #region /*런타임 주입*/
    [SerializeReference] public BlackboardVariable<GameObject> _spawner;//Manager
    [SerializeReference] public BlackboardVariable<int> CellIndex;
    #endregion

    NavMeshAgent _agent;
    SpawnTile _spawnTile;

    /*Status 관리*/
    StatusScript _status;
    Collider[] _col; 

    DetectPlayer _detectPlayer;
    protected override Status OnStart()
    {
       if(null ==_detectPlayer)
        {
            _detectPlayer = _self.Value.GetComponent<DetectPlayer>();
        }
        if( _agent == null )
        {
            _agent = _self.Value.GetComponent<NavMeshAgent>();
        }
        _agent.enabled = true;
        if (null == _status)
        {
            _status =_self.Value.GetComponent<StatusScript>();
        }
         //Self.Value
        if (null != _spawner)
        {
            _spawnTile= _spawner.Value.GetComponent<EnemySpawner>().GetSpawnTileByIndex(CellIndex);
            _spawnTile.SetMonPos();
        }

        /*애니메이터 리스폰하면서 idle (속도 0 )으로 애니메이션 트리거*/
        if (null == _animator)
        {
            _animator  = _self.Value.GetComponent<Animator>();
        }
        _animator.Play(Animator.StringToHash("Base Layer.WanderLocomotion"), 0, 0f);
        _animator.SetTrigger("OnWander");
        _animator.SetFloat("Speed", 0f);


        /*셰이더 이펙트 시작*/
        if (null == _effects)
        {
            _effects   = _self.Value.GetComponent<ShaderEffects>();
        }
        _effects.SetNowMarerial(eShaderEffect.PHASE);
        _effects.DoFade(-0.5f, 0.76f, 5f, eFadeMode.FADE_IN);


        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (true ==_effects.IsFadeEffectDone)
        {
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _detectPlayer.EnableTargetDetector();
        _status.LoadScriptInfo();

        _state.Value=  eEnemyState.WANDER;

        /*Reset으로 나중에 수정 (WolfController 같은데서 )*/
        if(null ==_col)
            _col = _self.Value.GetComponentsInChildren<Collider>();
        
        foreach (Collider c in _col)
        {
            if (0<(c.includeLayers.value & LayerMask.GetMask("Enemies")))
                c.enabled =true;
        }

        _status.IsDead = false;
    }
}

