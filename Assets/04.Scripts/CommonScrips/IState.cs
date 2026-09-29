using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[Serializable]
public abstract class IState 
{
    protected StatusScript _status;
    protected SkillSO _skillSO; 
    protected  MovementCom m_MovementCom; 
    protected AnimatorCom m_AnimatorCom;
    protected IController m_Controller;  //나 아래함수로 넘겨줘  
    public void SetSkillSO(SkillSO skillSO) { _skillSO= skillSO; }
    public virtual void SetController(IController Controller)
    {
        m_Controller =Controller; 
    }
    public virtual  void SetMovementCom(MovementCom MovementCom)
    {
        m_MovementCom =MovementCom; 
    }
    public virtual void SetAnimatorCom(AnimatorCom AnimatorCom)
    {
        m_AnimatorCom= AnimatorCom;
    }

    public void ChangeState( int State)
    {
        m_Controller.ChangeState(State);
    }
    public virtual void Enter()
    {
        
    }
    // Update is called once per frame
    public virtual void  UpdateState()
    {
        
    }
    public virtual void Exit()
    {

    }
    public virtual void Initialize()
    {
        
    }
    protected virtual void SetHitBoxInfo() {
        //_hitBoxDictionary[_nowHitAreaMarker].InjectScript(m_PlayerController.GetComponent<StatusScript>());
        //PlayerSkillInfo playerSkillInfo = Managers.Player.GetSkillInfoByHandle(_skillSO.handle);
        //SkillHitInfo skillHitInfo = new SkillHitInfo
        //{
        //    _skillDamage=1,
        //    _skillCirticalChance=1;
        //    _skillCriticalDamage=1;
        //    _numHit=1,
        //};
        //_hitBoxDictionary[_nowHitAreaMarker].InjectSkillInfo(skillHitInfo);
    }
}
