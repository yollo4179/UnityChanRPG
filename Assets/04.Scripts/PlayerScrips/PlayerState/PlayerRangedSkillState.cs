using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRangedSkillState : PlayerState
{
    enum eSkill
    {
        START
    }

   
   

    int dbgNowIdx = -1;
    int dbgPreIdx = -1;
    

    Coroutine _co = null;
    private readonly List<ChainLightningProj> _activeChains = new List<ChainLightningProj>();

    private void StopActiveChains()
    {
        foreach (ChainLightningProj chain in _activeChains)
            if (chain != null && chain.gameObject.activeSelf) chain.StopShooting();
        _activeChains.Clear();
    }
   

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
        dbgNowIdx = -1;
        dbgPreIdx = -1;

        _animDone =false;

        m_PlayerMovementCom.resetMovementVector();
        /*공격 바로 진입*/
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f)
            if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_nowAnim])
            {
                _animDone=true; return;
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
        if (_animator != null) _animator.ResetTrigger("OnSkill");
        StopActiveChains();
        if (_co != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_co);
            _co = null;
        }
        if (_effectHandle != null)
        {
            Managers.Pool.GetBack(_effectHandle);
            _effectHandle = null;
        }
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
                if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_nowAnim])
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
        _animator.SetTrigger("OnSkill");
        EventSequence eventSequence = base._skillSO.EventSequence; //데이터를 가져온다. 


        Debug.LogFormat($"<color=#FF0000>nowState Enter:{dbgNowIdx} :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime} </color>");

        //이전 애니메이션으로 보간중이라면 기다린다.
        while (( _animator.GetCurrentAnimatorStateInfo(0).fullPathHash) != _animHash[_nowAnim])
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


        _animator.ResetTrigger("OnSkill");
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
                if ((  _animator.GetCurrentAnimatorStateInfo(0).normalizedTime) >= 0.9f)
                {
                  
                    for (int i = 0; i < isPrevEventOns.Count; i++)
                    {
                        if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false, events[i]); isPrevEventOns[i] = false; }
                    }
                    _status.CurMana=_status.CurMana -_skillSO.ManaCost;
                    _status.OnPlayerStatusChangedEvent.Invoke(_status, Player.ePlayerSlider.MANA, 3F);
                    _co =null;
                    _animDone =true;
                    Debug.LogFormat($"<color=#FFFFF0>nowState:{dbgNowIdx} :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime} </color>");
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
    private IEnumerator DelayEffectActive(Poolable handle)
    {
        Poolable p = handle;
        yield return new WaitForSeconds(1f);
        Managers.Pool.GetBack(p);

    }

    public void FireEvents(eAnimEvent eventName, bool isOn, AnimEventDesc eventDesc = default)
    {
        if (isOn && Managers.UI.BlocksAttackInput) return;
        // Debug.Log(eventName);
        switch (eventName)
        {
            case eAnimEvent.SUMMON_PROJECTILES:
            {
                    if(isOn) //한번 호출()
                    {
                        foreach(var spawnSocket in eventDesc.spawnSocket)
                        {
                            Poolable poolable = Managers.Pool.LendPoolableTo(eventDesc.poolingProjectileKey,null);//자식 필요하면  옵션으로 분기
                            PlayerProjectile projectile = poolable.GetComponent<PlayerProjectile>(); //플레이어 정보를 가져온다. ( 컨트롤러로부터)
                            projectile.SetSkillSO(_skillSO);
                            if (projectile is ChainLightningProj chain) _activeChains.Add(chain);
                            projectile.Init(m_PlayerController);
                            
                        }
                        

                        
                    }

                    if (!isOn) StopActiveChains();
                    break;
            }
            case eAnimEvent.COLLIDER:
                {
                    if (isOn)
                    {
                        nowAttackID  =_hitBoxDictionary[_nowHitAreaMarker].IncreaseAttackID(30);//123 /1 ->412/2 -> 341/3 ->234/4   
                        _hitBoxDictionary[_nowHitAreaMarker].GetComponent<Collider>().enabled = true;
                        _hitBoxDictionary[_nowHitAreaMarker].enabled = true;

                    }
                    else
                    {
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
                        if (true ==eventDesc.effectSetting.isOnWeapon)
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

    public void SpawnProjectile()
    {

    }
}
