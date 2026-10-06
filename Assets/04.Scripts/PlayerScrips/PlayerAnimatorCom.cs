using Unity.AppUI.Core;
using UnityEngine;
using System.Collections.Generic;


public class PlayerAnimatorCom : AnimatorCom
{
    

    private bool _lockY = false;
    private bool _lockRotationY = false;
    private PlayerControllerCom _controller;
    protected override void Awake()
    {
        base.Awake();
        // Keep root-motion extraction enabled. Changing it during combat resets
        // the Animator and interrupts pose blending; OnAnimatorMove gates movement.
        _Animator.applyRootMotion = true;
        _controller = GetComponentInParent<PlayerControllerCom>();
    }
    public void OnMovement(float _Horizontal, float _Vertical)
    {
        _Animator.SetFloat("Horizontal", _Horizontal);
        _Animator.SetFloat("Vertical", _Vertical);
        float speed = Mathf.Clamp01(new Vector2(_Horizontal, _Vertical).sqrMagnitude);
        _Animator.SetFloat("Speed", speed, 0.2f, Time.deltaTime);



    }
    //Root모션 적용중일때만 동작
    public void OnAnimatorMove()
    {
        if (Managers.UI.BlocksGameplayInput) return;
        // Only combat states use root motion; locomotion is moved by input.
        if (_controller == null || !_controller.UsesAnimationRootMotion) return;
        Vector3 deltaPos = _Animator.deltaPosition;
        if (_lockY)
            deltaPos.y =0;
        MoveRootTo(deltaPos);

        /*루트모션 회전 제어*/
        Quaternion dr = _Animator.deltaRotation;
        if (_lockRotationY)
        {
            // 예: yaw만 반영하고 pitch/roll 제거
            Vector3 e = dr.eulerAngles;
            dr = Quaternion.Euler(0f, e.y, 0f);
        }
        RotateRootTo(dr);
       // GetComponent<CharacterController>().Move(deltaPos);
    }

    public void RotateRootTo(Quaternion rotation)
    {


        transform.rotation = rotation*transform.rotation;
    }
    public void MoveRootTo(Vector3 dir)
    {
        // Vertical motion belongs to movement gravity/jump, not walk animation bobbing.
        dir.y = 0f;
        PlayerMovementCom movement = GetComponent<PlayerMovementCom>();
        if (movement != null) movement.MoveWithCollision(dir);
    }

    public void OnJump()
    {
        /*Jump trigger*/
        _Animator.SetTrigger("OnJump");
    }
    public void DashIs(bool State)
    {
        _Animator.SetBool("OnDash", State);
    }
    public void onBaseAttack()
    {
        _Animator.SetTrigger("OnBaseAttack");
    }
    public void onSkill()
    {
        _Animator.SetTrigger("OnSkill");
    }

}
