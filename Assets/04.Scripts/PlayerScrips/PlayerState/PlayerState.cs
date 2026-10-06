using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public abstract class PlayerState : IState
{
    protected PlayerControllerCom m_PlayerController;
    protected PlayerMovementCom m_PlayerMovementCom; 
    protected PlayerAnimatorCom m_PlayerAnimatorCom;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    /*Base T*/
    protected bool _lastClicked = false;
    protected float _elapsedTime = 0.0f;
    protected float _nextAttackReservationDuration = 0.3f;
    protected List<int> _animHash;

    protected int _nowAnim = 0;
    protected Animator _animator;
    protected HitBox[] _hitBoxList;
    protected Dictionary<eHitAreaMarker, HitBox> _hitBoxDictionary;
    protected eHitAreaMarker _nowHitAreaMarker = 0;//어쩌면 배열로 해도 될듯. activeHitBoxes[]배열로 관리
    protected int nowAttackID = 0;
    protected Poolable _effectHandle = null;
    protected bool _animDone = false;
    protected SkillHitInfo _skillHitInfo = new SkillHitInfo();

    public override void SetController(IController Controller)
    {
        /*다이나믹 캐스트로 일단 박자(한번 호출 성능 문제x)
          여기선 PlayerControlller만 쓸거임*/
        m_PlayerController =Controller as PlayerControllerCom;
        _status= m_PlayerController.GetComponent<StatusScript>();   


    }
    public override void SetMovementCom(MovementCom MovementCom)
    {
        
        m_PlayerMovementCom =MovementCom as PlayerMovementCom;
    }
    public override void SetAnimatorCom(AnimatorCom AnimatorCom)
    {
        
        m_PlayerAnimatorCom= AnimatorCom as PlayerAnimatorCom;
    }

    protected virtual void UpdateMovement()
    {

    }
    protected virtual void UpdateAction()
    { 
    }

    protected virtual void UpdateAnimation()
    {

    }
    protected virtual void CheckStateChanges()
    {

    }
    /*Condition*/
    /*점프 조건 입니다.*/
   protected    bool CheckJumpCondition() { return m_PlayerMovementCom.isGround && Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.JUMP)) ; }
   protected    bool CheckJumpExitCondition() { return (false == m_PlayerMovementCom.isJumping) ;  }
    /*대시 컨디션 입니다*/
   protected    bool CheckSprintCondition() { return m_PlayerMovementCom.isGround && Input.GetKey(m_PlayerController.GetKeyCode(eKEY_CODE.DASH)); }
   protected    bool CheckSprintExitCondition() { return Input.GetKeyUp(m_PlayerController.GetKeyCode(eKEY_CODE.DASH));}

    /*기본 공격 조건입니다. */ 
    protected bool CheckAttack1Condition()
    {
        return !Managers.UI.BlocksAttackInput &&
            !Managers.UI.IsPointerOverUI &&
            Input.GetMouseButton(0);
    }

    protected bool CheckSkillCondition(out int skillSlot)
    {
         skillSlot = 0;
         if (Managers.UI.BlocksAttackInput) return false;


        if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT1)))
            skillSlot =1;
        else if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT2)))
            skillSlot =2;
        else if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT3)))
            skillSlot =3;
        else if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT4)))
            skillSlot =4;
        else if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT5)))
            skillSlot =5;
        else if (Input.GetKeyDown(m_PlayerController.GetKeyCode(eKEY_CODE.SLOT6)))
            skillSlot=6;

        return 0 < skillSlot;
    }
    public void CheckAndChangeToSkillState()
    {
        if (CheckSkillCondition(out var skillSot))
        {
            SkillSO skillSO = Managers.UI.GetOpenUIByName("HUD_Canvas_Prefab").GetComponentInChildren<UI_DisplayQuickSlots>().GetSkillInfoBySlotNO(skillSot);
            if (skillSO != null) //해당 슬롯이 스킬이 아니면NULL이 나올것
            {
                switch (skillSO.SkillType)
                {
                    case eSkillType.MELEE:
                        {

                            m_PlayerController.GetState((int)PlayerControllerCom.PLAYERSTATE.SKILL_MELEE).SetSkillSO(skillSO);
                            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.SKILL_MELEE);

                            break;
                        }

                    case eSkillType.RANGED:
                        {
                            m_PlayerController.GetState((int)PlayerControllerCom.PLAYERSTATE.SKILL_RANGED).SetSkillSO(skillSO);
                            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.SKILL_RANGED);
                            break;
                        }
                    case eSkillType.BUFF:
                        {
                            m_PlayerController.GetState((int)PlayerControllerCom.PLAYERSTATE.SKILL_BUFF).SetSkillSO(skillSO);
                            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.SKILL_BUFF);
                            break;
                        }
                }

            }
        }

    }

    protected override void SetHitBoxInfo()
    {
        if (!_hitBoxDictionary.TryGetValue(_nowHitAreaMarker,out var hitBox)) return; 

        _hitBoxDictionary[_nowHitAreaMarker].InjectScript(m_PlayerController.GetComponent<StatusScript>());
        PlayerSkillInfo playerSkillInfo = Managers.Player.GetSkillInfoByHandle(_skillSO.handle);

        //skillSO마다 스킬 능력치 계산법 재정의해서 넘기기 TO DO
        _skillHitInfo  = new SkillHitInfo
        {
            _skillDamage=playerSkillInfo.BaseDamage* playerSkillInfo.Level,
            _skillCirticalChance =playerSkillInfo.ExtraCriticalChance* playerSkillInfo.Level,
            _skillCriticalDamage=playerSkillInfo.ExtraCriticalDamage *playerSkillInfo.Level,
            _numHits= playerSkillInfo.NumberOfHit,
            _skillLevel =playerSkillInfo.Level
        };
        _hitBoxDictionary[_nowHitAreaMarker].InjectSkillInfo(_skillHitInfo);
    }
}


