using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterSleep", story: "MonsterisInSleep", category: "Action", id: "f9a1d053edcad542ec0211a2a6a53806")]
public partial class MonsterSleepAction : Action
{
    [SerializeReference]public BlackboardVariable<GameObject> _self;
    [SerializeReference] public  BlackboardVariable<eEnemyState> _state; 
    private Animator _animator;

    private  WakeUpTrigger _wakeUpTrigger;
    int _wakeUpHash;
    int _stateHash;
    int _targetHash; 

    protected override Status OnStart()
    {
        if (null == _animator)
            _animator =  _self.Value.GetComponent<Animator>();
        _wakeUpHash = Animator.StringToHash("OnWakeUp");//ToBattle;
        _stateHash = Animator.StringToHash("Base Layer.Sleep");
        _targetHash = Animator.StringToHash("Base Layer.BattleLocomotion");



        if (null ==_wakeUpTrigger)
            _wakeUpTrigger = _self.Value.GetComponent<WakeUpTrigger>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        bool hasTriggerOn = _wakeUpTrigger.CheckTrigger();
        if(hasTriggerOn)
        {
            if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash != _targetHash)
            {
                _animator.SetTrigger(_wakeUpHash);
                return Status.Running;
            }
            else
            {
                return Status.Success;
            }
        }
        return Status.Running;

    }

    protected override void OnEnd()
    {

        switch (_wakeUpTrigger.HasCutScene())//분기 많아지면 내부에서 스위치문으로 받아오기 .
        {
            case true:
                _state.Value = eEnemyState.CUTSCENE;
                break;

            case false:
                _state.Value = eEnemyState.CHASE;
                break;
        }


    }
}

