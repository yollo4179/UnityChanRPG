using UnityEngine;

public class AnimatorCom_CrabGuy : MonoBehaviour
{
    [SerializeField]
    Animator animator; 


    public void Trigger_Attack()
    {
        animator.SetTrigger("Attack");
    }
    public void Movement (float _MovementSpeed)
    {
        animator.SetFloat("MovementSpeed", _MovementSpeed);
    }
}
