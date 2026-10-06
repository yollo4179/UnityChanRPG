using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerMovementState : PlayerState
{
  
    float m_X; 
    float m_Z; 
    public override void Enter()
    {
        m_X = Input.GetAxisRaw("Horizontal");
        m_Z = Input.GetAxisRaw("Vertical");
        Animator animator = m_PlayerAnimatorCom._Animator;
        animator.SetFloat("Horizontal", m_X);
        animator.SetFloat("Vertical", m_Z);
        animator.SetFloat("Speed", Mathf.Clamp01(new Vector2(m_X, m_Z).sqrMagnitude));
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        UpdateMovement();
        UpdateAnimation();

        CheckStateChanges();

    }

    protected override void UpdateMovement()
    {
        m_X = Input.GetAxisRaw("Horizontal");
        m_Z = Input.GetAxisRaw("Vertical");
        m_PlayerMovementCom.MoveTo(new Vector3(m_X, 0f, m_Z));
    }

    protected override void UpdateAnimation()
    {
        m_PlayerAnimatorCom.OnMovement(m_X, m_Z);
    }
    protected override void CheckStateChanges()
    {

        if(CheckJumpCondition())
        {

            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.JUMP); return; 

        }
        else if (CheckSprintCondition())
        {
            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.DASH); return;
        }

        else if (CheckAttack1Condition())
        {
            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.BASE_ATTACK); return;
        }

        CheckAndChangeToSkillState();

     }
}
