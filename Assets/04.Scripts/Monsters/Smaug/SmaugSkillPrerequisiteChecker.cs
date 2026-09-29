using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class SmaugSkillPrerequisiteChecker : MonsterAttackPrerequisiteChecker
{
    int cachedSkill = -1;
    
    protected override void Start()
    {
        base.Start();


        _nowSpecialSkillIndex = 0;

    }

    // Update is called once per frame
    public override void CheckAttackCondition()
    {
        //조건만 여기에서 설정하고 실제 공격은 BehaviorTree에서 처리한다. SkillCode에 따라서 처리한다.  Controller 
        if (false == _hasInitialized)
        {
            ClearAttackCoolTime();
            ClearSpecialSkillCoolTime();
            ClearShieldCoolTime();
            _hasInitialized = true;
        }

        Vector3 toTarget = VectorUtil.PlatVector(_targetTransform.position - transform.position);
        float lengthXZ = toTarget.magnitude;
        Vector3 toSrc = transform.forward;
        float nowAngle = Vector3.Angle(toTarget, toSrc);

        if (_skillAnimFullpathHash !=_animator.GetCurrentAnimatorStateInfo(0).fullPathHash) return;
        if (toTarget.magnitude > _farAttackRange) return;




       
        MonsterAttackDesc nowFlySkill = _monsterSpecialSkillList[_nowSpecialSkillIndex];
        if (Time.time - nowFlySkill.skillStartTime  >  nowFlySkill.CooldownTime)
        {
            nowFlySkill.skillStartTime = Time.time;
            _behaviorAgent.SetVariableValue(BlackboardKeys.ENEMY_STATE, eEnemyState.FLY);
            return;
        }


        _behaviorAgent.BlackboardReference.GetVariableValue<int>(BlackboardKeys.SKILL_NO, out _nowSkillIndex);
        
        bool skillOn = false; 
        if (lengthXZ <_attackRange&& nowAngle < _minAttackAngle)
        {
            if (-1 !=cachedSkill)
            {
                if (Time.time - _monsterAttackDescList[cachedSkill].skillStartTime  >  _monsterAttackDescList[cachedSkill].CooldownTime)
                {
                    _behaviorAgent.BlackboardReference.SetVariableValue(BlackboardKeys.SKILL_NO, cachedSkill);
                    _nowSkillIndex  =cachedSkill;
                    cachedSkill = -1;
                    skillOn = true;
                }
            }
            if (false ==skillOn &&Time.time - _monsterAttackDescList[_nowSkillIndex].skillStartTime  >  _monsterAttackDescList[_nowSkillIndex].CooldownTime)
            {
                skillOn = true;
   
            }
        }
        if (false ==skillOn&&Time.time - _monsterAttackDescList[2].skillStartTime  >  _monsterAttackDescList[2].CooldownTime)
        {
            skillOn = true;
            if(_nowSkillIndex != 2)
                cachedSkill =_nowSkillIndex;
            _behaviorAgent.BlackboardReference.SetVariableValue(BlackboardKeys.SKILL_NO, 2);        
        }

        if(true ==skillOn)
        {
            ClearAttackCoolTime();
            /*쿨타임 지나고 스킬 시전*/
            _behaviorAgent.SetVariableValue(BlackboardKeys.ENEMY_STATE, eEnemyState.ATTACK);

        }
    }
}
