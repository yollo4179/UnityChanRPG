using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WolfRespawn", story: "The agent is Respawned by [spawner] cell", category: "Action", id: "a4525fdbf2d3deba72575d369fd3527b")]
public partial class WolfRespawnAction : Action
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

    SpawnTile _spawnTile;

    /*Status 관리*/
    StatusScript _status; 
    protected override Status OnStart()
    {
        _status =_self.Value.GetComponent<StatusScript>();
        //Self.Value
        if (null != _spawner)
        {
            _spawnTile= _spawner.Value.GetComponent<EnemySpawner>().GetSpawnTileByIndex(CellIndex);
            _spawnTile.SetMonPos();
        }

        /*애니메이터 리스폰하면서 idle (속도 0 )으로 애니메이션 트리거*/
        _animator  = _self.Value.GetComponent<Animator>();
        _animator.Play(Animator.StringToHash("Base Layer.WanderLocomotion"), 0, 0f);
        _animator.SetTrigger("OnWander");
         _animator.SetFloat("Speed", 0f);


        /*셰이더 이펙트 시작*/
        _effects   = _self.Value.GetComponent<ShaderEffects>();
        _effects.SetNowMarerial(eShaderEffect.PHASE);
        _effects.DoFade(-0.5f, 0.76f, 5f,eFadeMode.FADE_IN);

      
        return Status.Running;

        /*Todo Collider*/
    }

    protected override Status OnUpdate()
    {

        if( true ==_effects.IsFadeEffectDone)
        {
            return Status.Success;
        }
        return Status.Running;

        
    }

    protected override void OnEnd()
    {
        //_status.OnHealthChangedEvent?.Invoke(_status, 0f);
        _status.LoadScriptInfo();

        _state.Value=  eEnemyState.IDLE;

        /*Reset으로 나중에 수정 (WolfController 같은데서 )*/
        Collider[] col = _self.Value.GetComponentsInChildren<Collider>();
        foreach (Collider c in col)
        {
           
                c.enabled =true;
        }

        _status.IsDead = false;
    }
}

