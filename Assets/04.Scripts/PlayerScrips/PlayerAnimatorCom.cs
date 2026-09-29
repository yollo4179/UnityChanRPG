using Unity.AppUI.Core;
using UnityEngine;
using System.Collections.Generic;


public class PlayerAnimatorCom : AnimatorCom
{
    

    private bool _lockY = false;
    private bool _lockRotationY = false;
    protected override void Awake()
    {
        base.Awake();
    }
    public void OnMovement(float _Horizontal, float _Vertical)
    {
        _Animator.SetFloat("Horizontal", _Horizontal);
        _Animator.SetFloat("Vertical", _Vertical);
        _Animator.SetFloat("Speed", new Vector2(_Horizontal, _Vertical).sqrMagnitude);



    }
    //Root모션 적용중일때만 동작
    public void OnAnimatorMove()
    {
        Vector3 deltaPos = _Animator.deltaPosition;
        if (deltaPos != Vector3.zero)
        {
            int a = 0;
        }
        if (_lockY)
            deltaPos.y =0;
        else
        {
            Debug.Log($"<color =#ffff00>RootY{deltaPos.y}</color>");
        }
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
        transform.position += dir;
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
