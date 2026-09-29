using Unity.Behavior;
using UnityEngine;

public class WolfAnimEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Animator _animator;
    BehaviorGraphAgent _behaviorGraphAgent; 

    public void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _behaviorGraphAgent = GetComponentInParent<BehaviorGraphAgent>();
    }
    public void SkillEnd()
    {
        _animator.SetTrigger("SkillEnd");
        _behaviorGraphAgent.BlackboardReference.SetVariableValue(BlackboardKeys.IS_ON_SKILL, false);

    }
}
