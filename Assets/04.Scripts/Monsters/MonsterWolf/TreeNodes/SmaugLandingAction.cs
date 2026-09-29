using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SmaugLanding", story: "SmaugLanding", category: "Action", id: "493e8ca62fbdb7ed0c47b19bb4713ce5")]
public partial class SmaugLandingAction : Action
{
   [SerializeReference]public BlackboardVariable<GameObject> _self;
   [SerializeReference]public BlackboardVariable<eEnemyState> _state;
    int _landingHash;
    int _landingTriggerHash;
    int _targetStateHash;

    Animator _animator;
    bool hasInitialized = false; 
    protected override Status OnStart()
    {
        if(null==_animator) _animator = _self.Value.GetComponent<Animator>();
        if(true==hasInitialized)return Status.Running;
        hasInitialized = true;
        _landingHash = Animator.StringToHash("Base Layer.FlyLocomotion.Land");
        _landingTriggerHash = Animator.StringToHash("OnLand");
        _targetStateHash = Animator.StringToHash("Base Layer.BattleLocomotion");
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash !=_landingHash)
        {
            _animator.SetTrigger(_landingTriggerHash);
        }

        if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash  ==_targetStateHash)
        {
            return Status.Success;
        }


        return Status.Running;
    }

    protected override void OnEnd()
    {
        _state.Value = eEnemyState.CHASE;
    }
}

