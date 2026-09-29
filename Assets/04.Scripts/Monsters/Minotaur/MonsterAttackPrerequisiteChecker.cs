using Unity.Behavior;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;
#if UNITY_ANDROID
using Unity.Android.Gradle.Manifest;
#endif
public class MonsterAttackPrerequisiteChecker : MonoBehaviour
{
    [SerializeField] public float coolDown = 0f;
    protected BehaviorGraphAgent _behaviorAgent;
    protected Transform _targetTransform; 


    [Header("MonsterAttackDesc")]
    [SerializeField]protected List<MonsterAttackDesc> _monsterAttackDescList;
    [SerializeField] protected List<MonsterAttackDesc> _monsterSpecialSkillList;
    [SerializeField] protected List<MonsterAttackDesc> _monsterShieldSkillList;
    protected int _nowSpecialSkillIndex = 0;
    protected int _nowSkillIndex = 0;
    protected float _coolDownTime = 0;
    [SerializeField] protected float _minAttackAngle = 10f;
    [SerializeField] protected float _attackRange = 3.5f;
    [SerializeField] protected float _farAttackRange = 3.5f;
    protected StatusScript _statusScript;
    protected Animator _animator;
    protected int _skillAnimFullpathHash;

    public bool _hasInitialized = false; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual  void Start()
    {
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();

        List<MonsterAttackDesc> copyMonsterAttackDesc = new List<MonsterAttackDesc>(_monsterAttackDescList);

        /*스킬 순서 동기화*/
        for (int i = 0; i< copyMonsterAttackDesc.Count; ++i)
        {
            _monsterAttackDescList[copyMonsterAttackDesc[i].SkillIdx] = copyMonsterAttackDesc[i];
        }
        _statusScript = GetComponent<StatusScript>();
        _animator =GetComponent<Animator>();
        _skillAnimFullpathHash = Animator.StringToHash("Base Layer.BattleLocomotion");

        _targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
   public  virtual void CheckAttackCondition()
    {
       


            //조건만 여기에서 설정하고 실제 공격은 BehaviorTree에서 처리한다. SkillCode에 따라서 처리한다.  Controller 

        Vector3 toTarget = VectorUtil.PlatVector(_targetTransform.position - transform.position);
        Vector3 toSrc = transform.forward;
        float nowAngle = Vector3.Angle(toTarget, toSrc);
        if (nowAngle > _minAttackAngle) return;
        if (_skillAnimFullpathHash !=_animator.GetCurrentAnimatorStateInfo(0).fullPathHash) return;
        if (toTarget.magnitude > _attackRange) return;



        _behaviorAgent.BlackboardReference.GetVariableValue<int>(BlackboardKeys.SKILL_NO, out _nowSkillIndex);
        MonsterAttackDesc nowSkill = _monsterAttackDescList[_nowSkillIndex];

        if (Time.time - _coolDownTime > _monsterAttackDescList[_nowSkillIndex].CooldownTime)
        {
            _coolDownTime = Time.time;
           
            /*쿨타임 지나고 스킬 시전*/
            _behaviorAgent.SetVariableValue(BlackboardKeys.ENEMY_STATE, eEnemyState.ATTACK);
            return;
        }
    }

    //공격 끝나고 Action End시 쿨타임 초기화

    public void ClearAttackCoolTime()
    {
        foreach (var skillDesc in _monsterAttackDescList)
        {
            skillDesc.skillStartTime = Time.time;

        }
    }
    public void ClearSpecialSkillCoolTime()
    {
        foreach (var skillDesc in _monsterSpecialSkillList)
        {
            skillDesc.skillStartTime = Time.time;

        }
    }
    public void ClearShieldCoolTime()
    {
        foreach (var skillDesc in _monsterShieldSkillList)
        {
            skillDesc.skillStartTime = Time.time;

        }
    }
}
