using UnityEngine;
using UnityEngine.Playables;

public class PlayerJumpState : PlayerState
{
    float m_X;
    float m_Z;

    public override sealed void Enter()
    {
        m_PlayerMovementCom.UpdateYSpeedToJumpForce();
        m_PlayerAnimatorCom.OnJump();
        m_PlayerMovementCom.isJumping=true;
    }
    public override sealed void UpdateState()
    {
        /*기본 움직임 점프는 착지하면 상태 풀림*/
        UpdateMovement();
        
        CheckStateChanges();
    }

    protected override sealed void UpdateMovement()
    {
        m_X = Input.GetAxis("Horizontal");
        m_Z = Input.GetAxis("Vertical");
        m_PlayerMovementCom.MoveTo(new Vector3(m_X, 0f, m_Z));
    }

    protected override sealed void UpdateAnimation()
    {

    }
    protected override sealed void CheckStateChanges()
    {
        if (CheckJumpExitCondition())
        {
            if (CheckSprintCondition())
            {
                m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.DASH);
            }
            else
            {
                m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.MOVEMENT);
            }


        }
    }
}
