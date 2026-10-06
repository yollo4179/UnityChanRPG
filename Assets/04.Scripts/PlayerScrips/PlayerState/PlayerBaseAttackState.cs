using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
#if UNITY_EDITOR
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEditor.PackageManager;
#endif
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;




public sealed class PlayerBaseAttackState : PlayerState
{

    public enum eCombo
    {
        COMBO1,
        COMBO2,
        COMBO3,
        KICK,
        COMBO_END,
    }    
    
    Dictionary<int, int> _dicCodetoIdx = new Dictionary<int, int>();
    int _nowCode = -1;
    int _preCode = -1;

    int dbgNowIdx = -1;
    int dbgPreIdx = -1;
    int _animStack;
    
    
    Coroutine _co = null;
  



    

    public override void Initialize()
    {

        

        _animator =  m_PlayerAnimatorCom._Animator;
        _animHash = Enumerable.Repeat(0, (int)eCombo.COMBO_END).ToList();
        _animHash[(int)eCombo.COMBO1] = Animator.StringToHash("Base Layer.ComboAttack_Sword.PlayerCombo1@Player");
        _animHash[(int)eCombo.COMBO2] = Animator.StringToHash("Base Layer.ComboAttack_Sword.PlayerCombo2@Player");
        _animHash[(int)eCombo.COMBO3] = Animator.StringToHash("Base Layer.ComboAttack_Sword.PlayerCombo3@Player");
        _animHash[(int)eCombo.KICK] = Animator.StringToHash("Base Layer.ComboAttack_Sword.PlayerKick@Player");
        
        _dicCodetoIdx.Add(_animHash[0], 0);
        _dicCodetoIdx.Add(_animHash[1], 1);
        _dicCodetoIdx.Add(_animHash[2], 2);
        _dicCodetoIdx.Add(_animHash[3], 3);

        _hitBoxList = m_PlayerController.GetHitBoxList();
        _hitBoxDictionary =new Dictionary<eHitAreaMarker, HitBox>();
        foreach (var hitBox in _hitBoxList)
        {
            _hitBoxDictionary.Add(hitBox.AreaMarker, hitBox);
        }


    }

    public override sealed void Enter()
    {
        dbgNowIdx = -1;
        dbgPreIdx = -1;




        _animDone =false;
        _animStack =0;

        _elapsedTime  = 0.0f;

        _lastClicked=false;

        m_PlayerMovementCom.resetMovementVector();
        /*공격 바로 진입*/

        _animator.SetBool("InCombo", true);
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
        m_PlayerAnimatorCom._Animator.SetBool("InCombo", false);
    }
    protected override sealed void UpdateAction()
    {


        if ( null==_co)
        {
            if (true ==_lastClicked||Input.GetMouseButtonDown(0))
            {
                if (_lastClicked) 
                    _lastClicked =false; 
                _animDone =false;
               // CoroutineRunner.Instance.StopCoroutine(ActivateAnimationEvents());
                _co = CoroutineRunner.Instance.StartCoroutine(ActivateAnimationEvents());
            }
        }
        else
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                if(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash ==_animHash[_animStack])
                {
                   // m_PlayerAnimatorCom.onBaseAttack();
                    _lastClicked =true; 
                }
            }
        }

    }

    protected override sealed void UpdateAnimation()
    {

    }
    protected override sealed void CheckStateChanges()
    {
        Animator myAnimator = m_PlayerAnimatorCom._Animator;
        if (true == _animDone)
        {
            _elapsedTime +=Time.deltaTime;
        }

        if (true == _animDone&&false ==_lastClicked)
        {
            _animator.SetBool("InCombo", false);
            m_PlayerController.ChangeState((int)PlayerControllerCom.PLAYERSTATE.MOVEMENT);


            return;
        }

    }
    private bool IsPointInRange(float tNow,float tStart,float tEnd)
    {
        return tStart<=tNow && tNow<= tEnd;
    }

    private  IEnumerator ActivateAnimationEvents()
    {
       
        EventSequence eventSequence = base._skillSO.EventSequence; //데이터를 가져온다. 


        Debug.LogFormat($"<color=#FF0000>nowState Enter:{dbgNowIdx} :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime} </color>");
       
        //이전 애니메이션으로 보간중이라면 기다린다.
        while ((_nowCode = _animator.GetCurrentAnimatorStateInfo(0).fullPathHash) != _animHash[_animStack])
        {
            if(_animator.GetNextAnimatorStateInfo(0).fullPathHash !=_animHash[_animStack])
                m_PlayerAnimatorCom.onBaseAttack();

            if(_animator.IsInTransition(0))
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
        int idxClips = -1;

        _dicCodetoIdx.TryGetValue(_animHash[_animStack], out idxClips);
            
        var events = eventSequence.eventClips[idxClips].events;
        List<bool> isPrevEventOns = Enumerable.Repeat(false, events.Count).ToList(); //세팅 (애니메이션 이벤트 플래그 -> false
        
        if (true ==_dicCodetoIdx.TryGetValue(_nowCode, out var idx))
        {
            if (3 == _dicCodetoIdx[_nowCode])
                _nowHitAreaMarker = eHitAreaMarker.RIGHT_LEG;
            else
                _nowHitAreaMarker = eHitAreaMarker.RIGHT_WEAPON;
        }
        ///////////////////세팅 완료
        try
        {
            //현재 공격 애니메이션이 진행중이라면 풀패스와 지금 코드를 비교한다.
            while (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animHash[_animStack])
            {
                m_PlayerMovementCom.RotateToCamera();

                float tNow = Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                bool nowEventOn = false;


                // 조건 1. 전이 중이면 다음 애니메이션으로 이동할  준비를 한다. 
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
                {
                    for (int i = 0; i < isPrevEventOns.Count; i++)
                    {
                        if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false); isPrevEventOns[i] = false; }
                    }
                    _elapsedTime=0;
                   _animDone =true;
                    // ++_animStack;

                    if (_dicCodetoIdx.TryGetValue(_animator.GetCurrentAnimatorStateInfo(0).fullPathHash, out var val))
                    {
                        switch (val)
                        {
                            case 0:
                                dbgNowIdx =val;
                                Debug.LogFormat($"<color=#F00000>{dbgNowIdx}</color>");
                                if (dbgPreIdx !=dbgNowIdx)
                                    Debug.LogFormat($"<color=#F00000>{dbgPreIdx}->{dbgNowIdx}</color>");
                                dbgPreIdx =dbgNowIdx;
                                break;
                            case 1:
                                dbgNowIdx =val;
                                Debug.LogFormat($"<color=#FF0000>{dbgNowIdx}</color>");
                                if (dbgPreIdx !=dbgNowIdx)
                                    Debug.LogFormat($"<color=#FF0000>{dbgPreIdx}->{dbgNowIdx}</color>");
                                dbgPreIdx =dbgNowIdx;
                                break;
                            case 2:
                                dbgNowIdx =val;
                                Debug.LogFormat($"<color=#FFF000>{dbgNowIdx}</color>");
                                if (dbgPreIdx !=dbgNowIdx)
                                    Debug.LogFormat($"<color=#FFF000>{dbgPreIdx}->{dbgNowIdx}</color>");
                                dbgPreIdx =dbgNowIdx;
                                break;
                            case 3:
                                dbgNowIdx =val;
                                Debug.LogFormat($"<color=#FFFF00>{dbgNowIdx}</color>");
                                if (dbgPreIdx !=dbgNowIdx)
                                    Debug.LogFormat($"<color=#FFFF00>{dbgPreIdx}->{dbgNowIdx}</color>");
                                dbgPreIdx =dbgNowIdx;
                                break;
                            default:
                                dbgNowIdx =val;
                                Debug.LogFormat($"<color=#FFFFF0>{dbgNowIdx}</color>");
                                if (dbgPreIdx !=dbgNowIdx)
                                    Debug.LogFormat($"<color=#FFFFF0>{dbgPreIdx}->{dbgNowIdx}</color>");
                                dbgPreIdx =dbgNowIdx;
                                break;
                        }
                       
                    }
                    _elapsedTime = 0;
                    ++_animStack;
                    _animStack%=4;
                    _co =null;
                    _animDone =true;
                    Debug.LogFormat($"<color=#FFFFF0>nowState:{dbgNowIdx} :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime} </color>");
                    Debug.Log($"<color=#00ffff>{_animStack} : next AttackIndex</color>");
                    yield  break;
                   
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
                if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false); isPrevEventOns[i] = false; }
            }

        //정리
        _elapsedTime = 0;
        ++_animStack;
        _animStack%=4;
        _co =null;
        _animDone =true;
        yield break;
        //정리
    }
   

    public void FireEvents(eAnimEvent eventName,bool isOn, AnimEventDesc eventDesc =default)
    {
        if (isOn && Managers.UI.BlocksAttackInput) return;
       // Debug.Log(eventName);
        switch (eventName)
        {
            case eAnimEvent.COLLIDER:
            {
                    if (isOn) {
                        nowAttackID  =_hitBoxDictionary[_nowHitAreaMarker].IncreaseAttackID(30);//123 /1 ->412/2 -> 341/3 ->234/4   
                        _hitBoxDictionary[_nowHitAreaMarker].GetComponent<Collider>().enabled = true;
                        _hitBoxDictionary[_nowHitAreaMarker].InjectScript(_status);
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
                    if (isOn) {
                        _effectHandle =Managers.Pool.LendPoolableTo(eventDesc.poolingEffectName, _hitBoxDictionary[_nowHitAreaMarker].transform.root);
                        _effectHandle.gameObject.SetActive(true);
                        /*트랜스폼 설정*/
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
                        //_effectHandle.enabled = false;
                        Managers.Pool.GetBack(_effectHandle);
                        _effectHandle =null;
                    }
                    break;
                }
        }

    }
}
