using UnityEngine;

public class PlayerDashState : PlayerState
{
    float m_X;
    float m_Z;

    public override void Enter()
    {
        m_PlayerAnimatorCom.DashIs(true);
        m_PlayerMovementCom.isSprinting=true;

        m_PlayerMovementCom.resetMovementVector();
    }
    public override void UpdateState()
    {
        UpdateMovement();
        CheckStateChanges();
    }
    public override void Exit()
    {
        m_PlayerAnimatorCom.DashIs(false);
        m_PlayerMovementCom.isSprinting=false;
    }




    protected override void UpdateMovement()
    {
        m_X = Input.GetAxisRaw("Horizontal");
        m_Z = Input.GetAxisRaw("Vertical");
        m_PlayerMovementCom.MoveTo(new Vector3(m_X, 0f, m_Z));
    }

    protected override void UpdateAnimation()
    {

    }
    protected override void CheckStateChanges()
    {

        if (CheckJumpCondition())
        {
            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.JUMP);
        }
        else if (CheckSprintExitCondition())
        {

            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.MOVEMENT);
            

        }
    }
}