using UnityEngine;

public class AnimReset : StateMachineBehaviour
{
    [SerializeField] string triggerName = null;
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(triggerName);
    }
}
