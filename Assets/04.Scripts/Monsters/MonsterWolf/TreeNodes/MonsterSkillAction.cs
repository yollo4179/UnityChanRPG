using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MonsterSkillAction", story: "MonsterSkills", category: "Action", id: "b0a3fc8364fe9e63c3ee1a9a308de560")]
public partial class MonsterSkillAction : Action
{

    [SerializeReference] public BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<int> _skillNo;
    
    [SerializeReference] public BlackboardVariable<bool> _isOnSkill;
    [SerializeReference] public BlackboardVariable<eEnemyState> _enemyState;

    [SerializeReference] public BlackboardVariable<List<string>> _strAnimationHashList;

    [SerializeReference] public BlackboardVariable<bool> _bTurnDuringSkill;
    [SerializeReference] public BlackboardVariable<bool> _setSpecificSkillSOHandleOption;
    [SerializeReference] public BlackboardVariable<int> _specificHandleIndexFromController;
    int _originSkillNo;

    public int[] _animationHashKeys;
    int _numSkills;
    bool hasInitialized = false; 

    #region From Self
    Animator _animator;
    StatusScript _status;

    bool _animationDone;
    int attackHashes;
   
    Coroutine _co;
    SkillSO _nowSkillSO;
    MonsterController _controller;
    MonsterAttackPrerequisiteChecker _attackPrerequisiteChecker;
    Dictionary<string, Transform> _dicBones = new Dictionary<string, Transform>();
  
    Poolable _handleCollider;
    Poolable _effectHandle;
    //BuffIndex
    Dictionary<int, int> _dicNowBuffIndex = new Dictionary<int, int>(); //skillNo , buffIndex
    #endregion

    protected override Status OnStart()
    {


        _isOnSkill.Value = true;
        if (null==_attackPrerequisiteChecker) _attackPrerequisiteChecker =_self.Value.GetComponent<MonsterAttackPrerequisiteChecker>();
        if (null==_controller) _controller = _self.Value.GetComponent<MonsterController>();
        if (null==_animator) _animator = _self.Value.GetComponentInChildren<Animator>();
        if (null==_status) _status = _self.Value.GetComponent<StatusScript>();

          _animator.SetTrigger("OnSkill");
        _animator.SetInteger("SkillNO", _skillNo.Value);
        
        
        _originSkillNo=_skillNo.Value;
        if (true ==_setSpecificSkillSOHandleOption)
        {
                _skillNo.Value = _specificHandleIndexFromController;
                _nowSkillSO = _controller.GetSkillSOByHandle(_specificHandleIndexFromController);
                 Debug.Assert(_nowSkillSO != null);
        }
        else
        {
                _nowSkillSO = _controller.GetSkillSOByHandle(_skillNo.Value); //0~2
        }
        
        _animationDone = false;
        _co =null;




        if (true ==hasInitialized) return Status.Running;
        hasInitialized = true;
       
        attackHashes = Animator.StringToHash("Base Layer.BattleLocomotion");
        _animationHashKeys = _strAnimationHashList.Value
            .Select(Animator.StringToHash)
            .ToArray();
        _numSkills = _animationHashKeys.Length;

        
        _isOnSkill.Value = true;
        _animator.SetTrigger("OnSkill");
        _animator.SetInteger("SkillNO", _skillNo.Value);
        _nowSkillSO = _controller.GetSkillSOByHandle(_skillNo.Value); //0~2
        _animationDone = false;
        _co =null;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
       


        if (true == _status.GotHit)
        {
            _status.GotHit = false;
        }
        if (eEnemyState.DEAD == _enemyState.Value)
        {
           

            return Status.Success;
        }

        if (null==_co&&false ==_animationDone)
        {
            _co = CoroutineRunner.Instance.StartCoroutine(ActivateAnimationEvents());
        }
        if (_animationDone)
        {
            return Status.Success;
        }



        return Status.Running;
    }
    protected override void OnEnd()
    {

        ++_skillNo.Value;
        _skillNo.Value%=_numSkills;
        if(true ==_setSpecificSkillSOHandleOption)
        {
            _skillNo.Value = _originSkillNo; 
        }

        _status.GotHit = false;
        _attackPrerequisiteChecker.ClearAttackCoolTime();
        switch (_enemyState.Value)
        {
            case eEnemyState.DEAD:
                {
                    if (null != _effectHandle)
                    {
                        _effectHandle.gameObject.transform.SetParent(Managers.Scene.CurrentScene.transform);
                        CoroutineRunner.Instance.StartCoroutine(DelayEffectActive(_effectHandle));//혹은 이벤트먀니저에게 부탁 ( 10초 지나면 없애주세요)
                    }
                    break;
                }

            default:
                {
                    _enemyState.Value = eEnemyState.CHASE;
                    break;
                }
        }




    }
    protected void UpdateAction()
    {


        if (null==_co&&false ==_animationDone)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.9f)
            {
                _animationDone =false;
                _co = CoroutineRunner.Instance.StartCoroutine(ActivateAnimationEvents());
            }
            else
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animationHashKeys[_skillNo.Value])
                    _animationDone=true;
            }
        }


    }
    public void TurnToPlayer()
    {
        GameObject _target = GameObject.FindGameObjectWithTag("Player");

        Vector3 toTarget = VectorUtil.PlatVector(_target.transform.position - _self.Value.transform.position);
        //_nowLookDir = Vector3.RotateTowards(_nowLookDir, toTarget, Time.deltaTime*3f, Time.deltaTime*5f);
        Quaternion targetRotation = Quaternion.LookRotation(toTarget);
        _self.Value.transform.rotation = Quaternion.Slerp(_self.Value.transform.rotation, targetRotation, Time.deltaTime* 10f);

    }
    private bool IsPointInRange(float tNow, float tStart, float tEnd)
    {
        return tStart<=tNow && tNow<= tEnd;
    }

    private IEnumerator ActivateAnimationEvents()
    {

        EventSequence eventSequence = _nowSkillSO.EventSequence; //데이터를 가져온다. 



        //이전 애니메이션으로 보간중이라면 기다린다.
        while ((_animator.GetCurrentAnimatorStateInfo(0).fullPathHash) != _animationHashKeys[_skillNo.Value])
        {
            /*if(true == turnDuringSkill) */TurnToPlayer();

            //현재 애니메이션에 도달하지 않았다 // 전이 상태가 아니다. //전이를 위해서 트리거를 건다.
            if (
                _animator.IsInTransition(0)&&
                _animator.GetNextAnimatorStateInfo(0).fullPathHash !=_animationHashKeys[_skillNo.Value]

                )
            {
                _animator.SetTrigger("OnSkill");
                _animator.SetInteger("SkillNO", _skillNo.Value);
            }
            #region 디버그
            //디버그 
            if (_animator.IsInTransition(0))
            {
                Debug.LogFormat($"<color=#00ff00>NowTransition :norTime :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime}</color>");

            }
            else
            {
                Debug.LogFormat($"<color=#ffff00>Before Transition :norTime :{_animator.GetCurrentAnimatorStateInfo(0).normalizedTime}</color>");
            }
            #endregion
            yield return null;
        }



        var events = eventSequence.eventClips[0].events;
        List<bool> isPrevEventOns = Enumerable.Repeat(false, events.Count).ToList(); //세팅 (애니메이션 이벤트 플래그 -> false


        try
        {
            //현재 공격 애니메이션이 진행중이라면 풀패스와 지금 코드를 비교한다.
            while (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _animationHashKeys[_skillNo.Value])
            {
                if (true == _bTurnDuringSkill) TurnToPlayer();

                //1이 넘어가는 경우 연속재생 차단 
                float tNowKeyFrame = Mathf.Repeat(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                bool nowEventOn = false;


                // 조건 1. 전이 중이면 다음 애니메이션으로 이동할  준비를 한다. 
                if ((/*preNormal =*/ _animator.GetCurrentAnimatorStateInfo(0).normalizedTime) >= 0.9f)
                {

                    for (int i = 0; i < isPrevEventOns.Count; i++)
                    {
                        if (isPrevEventOns[i]) { FireEvents(events[i].eventName, false, events[i]); isPrevEventOns[i] = false; }
                    }
                    _co =null;
                    _animationDone =true;

                    Debug.Log($"<color=#00ffff>{0} : next AttackIndex</color>");
                    yield break;

                }
                //이벤트를 확인한다. //이벤트는 Has Exit Time 전으로 모두 처리한다. 
                for (int i = 0; i<events.Count; ++i)
                {
                    nowEventOn = IsPointInRange(tNowKeyFrame, events[i].startTime, events[i].endTime);

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


        _animationDone =true; //animation이 분리되어있다면 nowAnim조건도 같이 조사해서 탈출 조건을 구성해야함
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
        // Debug.Log(eventName);
        switch (eventName)
        {
            case eAnimEvent.COLLIDER:
                {
                    if (isOn)
                    {

                        if (!_dicBones.TryGetValue(eventDesc.spawnSocket[0], out var bone))
                        {
                            var all = _self.Value.transform.GetComponentsInChildren<Transform>(true);
                            var list = all.Where(x => x.name ==eventDesc.spawnSocket[0]).ToList();
                            _dicBones.Add(eventDesc.spawnSocket[0], list[0]); //무조건 하나 있도록 뼈 삽입
                        }
                        Debug.Assert(_dicBones.ContainsKey(eventDesc.spawnSocket[0]) == true, $"{eventDesc.spawnSocket[0]} is not found");
                        _handleCollider = Managers.Pool.LendPoolableTo("HitBoxMonster_Prefab", _dicBones[eventDesc.spawnSocket[0]], false);
                        // _handleCollider.transform.SetParent(_dicBones[eventDesc.spawnSocket[0]],worldPositionStays:false);
                        HitBox hitBox = _handleCollider.GetComponent<HitBox>();
                        hitBox.enabled = true;
                        hitBox.InjectScript(_status);//wolf 기본공격 넘기기
                        hitBox.SetOwner(_self.Value);
                        hitBox.IncreaseAttackID(30);
                        SkillHitInfo skillHitInfo = new SkillHitInfo
                        {
                            _skillDamage = _nowSkillSO.Damage,
                            _skillCirticalChance = 10, //Todo 스킬별로 다르게 추가
                            _skillCriticalDamage = 3,
                            _numHits = eventDesc.effectSetting.hitNum,
                            _skillLevel = _nowSkillSO.SkillLevel,

                        };
                        hitBox.InjectSkillInfo(skillHitInfo);
                        BoxCollider hitBoxCollider = hitBox.GetComponent<BoxCollider>();
                        hitBoxCollider.center = eventDesc.colCenter;
                        hitBoxCollider.size = eventDesc.colSize;
                        hitBoxCollider.enabled = true;
                    }
                    else
                    {
                        HitBox hitBox = _handleCollider.GetComponent<HitBox>();
                        hitBox.ResetSkillInfo();
                        hitBox.GetComponent<Collider>().enabled = false;
                        Managers.Pool.GetBack(_handleCollider);
                    }
                    break;
                }
            case eAnimEvent.EFFECT:
                {
                    if (isOn)
                    {

                        /*트랜스폼 설정*/

                        if (!_dicBones.TryGetValue(eventDesc.spawnSocket[0], out var bone))
                        {
                            var all = _self.Value.transform.GetComponentsInChildren<Transform>(true);
                            var list = all.Where(x => x.name ==eventDesc.spawnSocket[0]).ToList();
                            _dicBones.Add(eventDesc.spawnSocket[0], list[0]); //무조건 하나 있도록 뼈 삽입
                        }

                        _effectHandle = Managers.Pool.LendPoolableTo(eventDesc.poolingEffectName, _dicBones[eventDesc.spawnSocket[0]]);

                        _effectHandle.gameObject.SetActive(true);
                        _effectHandle.transform.localPosition=eventDesc.effectSetting.localPosition;

                        _effectHandle.transform.localEulerAngles= (eventDesc.effectSetting.localRotation);
                        _effectHandle.transform.localScale=eventDesc.effectSetting.localScale;
                        var pss = _effectHandle.GetComponentsInChildren<ParticleSystem>(true);


                        foreach (var ps in pss)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                            ps.Play(true);
                        }
                    }
                    else
                    {
                        if (true ==eventDesc.effectSetting.delayEffectActive)
                        {
                            if (null != _effectHandle)
                            {
                                _effectHandle.gameObject.transform.SetParent(Managers.Scene.CurrentScene.transform);
                                CoroutineRunner.Instance.StartCoroutine(DelayEffectActive(_effectHandle));//혹은 이벤트먀니저에게 부탁 ( 10초 지나면 없애주세요)
                            }
                        }
                        else
                        {
                            if (null != _effectHandle)
                            {
                                _effectHandle.enabled = false;
                                Managers.Pool.GetBack(_effectHandle);
                            }
                        }
                        _effectHandle =null;
                    }
                    break;
                }

            case eAnimEvent.ON_SKILL:
                {
                    if (isOn)
                    {
                        if (eSkillType.BUFF==_nowSkillSO.SkillType)
                        {
                            BuffInfo[] buffs = _nowSkillSO.BuffInfo;
                            if (!_dicNowBuffIndex.TryGetValue(_skillNo.Value, out var buffIndex))
                            {
                                _dicNowBuffIndex.Add(_skillNo.Value, 0);
                            }

                            BuffInfo nowBuff = buffs[_dicNowBuffIndex[_skillNo.Value]];
                            _status.AddBuff(nowBuff.buffType, nowBuff.value, nowBuff.duration, false);
                            _dicNowBuffIndex[_skillNo.Value] =(_dicNowBuffIndex[_skillNo.Value] + 1)%buffs.Length;

                        }

                    }
                    break;
                }
        }

    }
}

