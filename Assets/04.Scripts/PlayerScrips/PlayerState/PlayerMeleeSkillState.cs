using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMeleeSkillState: PlayerState
{

    Coroutine _co = null;
    public override void Initialize()
    {
        

        _hitBoxList = m_PlayerController.GetHitBoxList();
        _hitBoxDictionary =new Dictionary<eHitAreaMarker, HitBox>();
        foreach (var hitBox in _hitBoxList)
        {
            _hitBoxDictionary.Add(hitBox.AreaMarker, hitBox);
        }


    }

    public override sealed void Enter()
    {

        _animator =  m_PlayerAnimatorCom._Animator;
        _animHash=new List<int>();
        foreach (var clip in _skillSO.EventSequence.eventClips)
        {
            _animHash.Add(Animator.StringToHash(clip.animFullPath));//HITArea는 SkillSO에서 CLIP 정보를 받아와서 처리해줌
        }
        

        _animDone =false;
        
        _animator.applyRootMotion =true;
        m_PlayerMovementCom.resetMovementVector();
        /*공격 바로 진입*/
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_nowAnim])
            {
                _animDone=true;return;
            }
        _co = CoroutineRunner.Instance.StartCoroutine(ActivateAnimationEvents());
    }
    public override sealed void UpdateState()
    {

        ///*기본 움직임 점프는 착지하면 상태 풀림*/
        UpdateAction();
        CheckStateChanges();

    }
    public override sealed void Exit()
    {
        CoroutineRunner.Instance.StopCoroutine(ActivateAnimationEvents());


        m_PlayerAnimatorCom._Animator.applyRootMotion =false;
    }
    protected override sealed void UpdateAction()
    {


        if (null==_co&&false ==_animDone)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.9f)
            {
                _animDone =false;
                _co = CoroutineRunner.Instance.StartCoroutine(ActivateAnimationEvents());
            }
            else
            {
                if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_nowAnim])
                    _animDone=true;
            }
        }


    }

    protected override sealed void UpdateAnimation()
    {

    }
    protected override sealed void CheckStateChanges()
    {
        
        if (true == _animDone)
        {
           
            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.MOVEMENT);
            return;
        }

    }
    private bool IsPointInRange(float tNow, float tStart, float tEnd)
    {
        return tStart<=tNow && tNow<= tEnd;
    }

    private IEnumerator ActivateAnimationEvents()
    {
        _animator.SetInteger("SkillNO", _skillSO.SkillNO);
        _animator.SetBool("OnSkill", true);
        EventSequence eventSequence = base._skillSO.EventSequence; //데이터를 가져온다. 


       
        //이전 애니메이션으로 보간중이라면 기다린다.
        while ((_animator.GetCurrentAnimatorStateInfo(0).fullPathHash) != _animHash[_nowAnim])
        {
            if (_animator.GetNextAnimatorStateInfo(0).fullPathHash !=_animHash[_nowAnim])
                m_PlayerAnimatorCom.onSkill();

            if (_animator.IsInTransition(0))
            {
                Debug.LogFormat($"<color=#00ff00>NowTransition :norTime :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime}</color>");

            }
            else
            {
                Debug.LogFormat($"<color=#ffff00>Before Transition :norTime :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime}</color>");
            }
            yield return null;
        }

        // 이벤트 정보 가져온다. . 
      

        var events = eventSequence.eventClips[_nowAnim].events;
        List<bool> isPrevEventOns = Enumerable.Repeat(false, events.Count).ToList(); //세팅 (애니메이션 이벤트 플래그 -> false
        _nowHitAreaMarker = eventSequence.eventClips[_nowAnim].hitArea;


        ///////////////////세팅 완료
        try
        {
            //현재 공격 애니메이션이 진행중이라면 풀패스와 지금 코드를 비교한다.
            while (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_nowAnim])
            {
                //m_PlayerMovementCom.RotateToCamera();

                float tNow = Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                bool nowEventOn = false;


                // 조건 1. 전이 중이면 다음 애니메이션으로 이동할  준비를 한다. 
                if (( _animator.GetCurrentAnimatorStateInfo(0).normalizedTime) >= 0.9f)
                {
                    
                    for (int i = 0; i < isPrevEventOns.Count; i++)
                    {
                        if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false, events[i]); isPrevEventOns[i] = false; }
                    }
                    _co =null;
                    _animDone =true;
                    _status.CurMana=_status.CurMana -_skillSO.ManaCost; 
                    _status.OnPlayerStatusChangedEvent.Invoke(_status, Player.ePlayerSlider.MANA,3F);
                    Debug.Log($"<color=#00ffff>{_nowAnim} : next AttackIndex</color>");
                    yield break;

                }
                //이벤트를 확인한다. //이벤트는 Has Exit Time 전으로 모두 처리한다. 
                for (int i = 0; i<events.Count; ++i)
                {
                    nowEventOn = IsPointInRange(tNow, events[i].startTime, events[i].endTime);

                    if (nowEventOn!=isPrevEventOns[i])
                    {
                        if (nowEventOn)
                            FireEvents(events[i].eventName, true, events[i]);
                        else
                            FireEvents(events[i].eventName, false, events[i]);
                       
                    }
                    isPrevEventOns[i]= nowEventOn;
                }
                yield return null;
            }
        }
        finally { }

        // 상태 벗어나면 정리(OFF 보장)
        for (int i = 0; i < isPrevEventOns.Count; i++)
        {
            if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false, events[i]); isPrevEventOns[i] = false; }
        }

        _status.CurMana=_status.CurMana -_skillSO.ManaCost;
        _status.OnPlayerStatusChangedEvent.Invoke(_status, Player.ePlayerSlider.MANA, 3F);
        _animDone =true; //animation이 분리되어있다면 nowAnim조건도 같이 조사해서 탈출 조건을 구성해야함
        _co =null;
        yield break;
        //정리
    }
    private IEnumerator DelayEffectActive(  Poolable handle)
    {
        Poolable p = handle;
        yield return new WaitForSeconds(1f);
        Managers.Pool.GetBack(p);

    }

    public void FireEvents(eAnimEvent eventName, bool isOn, AnimEventDesc eventDesc = default)
    {
        // Debug.Log(eventName);
        switch (eventName)
        {
            case eAnimEvent.COLLIDER:
                {
                    if (isOn)
                    {
                        nowAttackID  =_hitBoxDictionary[_nowHitAreaMarker].IncreaseAttackID(30);//123 /1 ->412/2 -> 341/3 ->234/4   
                        SetHitBoxInfo();
                        _hitBoxDictionary[_nowHitAreaMarker].GetComponent<Collider>().enabled = true;
                        _hitBoxDictionary[_nowHitAreaMarker].InjectScript(_status);
                        _hitBoxDictionary[_nowHitAreaMarker].enabled = true;
                        
                    }
                    else
                    {

                        //클리어 함수는 나중에
                        _hitBoxDictionary[_nowHitAreaMarker].ResetSkillInfo();
                        _hitBoxDictionary[_nowHitAreaMarker].GetComponent<Collider>().enabled = false;
                        _hitBoxDictionary[_nowHitAreaMarker].enabled = false;
                    }
                    break;
                }
            case eAnimEvent.EFFECT:
                {
                    if (isOn)
                    {
                        
                        /*트랜스폼 설정*/
                        if(true ==eventDesc.effectSetting.isOnWeapon)
                        {
                            _effectHandle =Managers.Pool.LendPoolableTo(eventDesc.poolingEffectName, _hitBoxDictionary[_nowHitAreaMarker].transform);
                            
                        }
                        else
                        {
                            _effectHandle =Managers.Pool.LendPoolableTo(eventDesc.poolingEffectName, _hitBoxDictionary[_nowHitAreaMarker].transform.root);
                            
                        }
                        _effectHandle.gameObject.SetActive(true);
                        _effectHandle.transform.localPosition=eventDesc.effectSetting.localPosition;
                        _effectHandle.transform.localRotation=Quaternion.Euler(eventDesc.effectSetting.localRotation);
                        _effectHandle.transform.localScale=eventDesc.effectSetting.localScale;
                        var pss = _effectHandle.GetComponentsInChildren<ParticleSystem>(true);

                        
                        foreach (var ps in pss)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                            // 완전 초기화가 필요하면 다음 줄도 사용
                            // ps.Simulate(0f, true, true, true);
                            ps.Play(true);
                        }
                    }
                    else
                    {
                        if (true ==eventDesc.effectSetting.delayEffectActive)
                        {
                            //_effectHandle.gameObject.transform.SetParent(Managers.Scene.CurrentScene.transform);
                            CoroutineRunner.Instance.StartCoroutine(DelayEffectActive(_effectHandle));//혹은 이벤트먀니저에게 부탁 ( 10초 지나면 없애주세요)
                        }
                        else
                        {
                            //_effectHandle.enabled = false;
                            Managers.Pool.GetBack(_effectHandle);
                        }
                        _effectHandle =null;
                    }
                    break;
                }
        }

    }
}
