using Unity.Behavior;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;
public class WolfAttackPrerequisiteChecker : MonoBehaviour, IMonsterAttackPrerequisiteChecker
{
    [SerializeField] public float coolDown = 0f;
    private BehaviorGraphAgent _behavioragent;  
    private EQSQuerierBehaviour_Battle _behaviorBattle;
    private EQSQuerier _querier;
    
    /*Attack 클래스를 리스트로 관리한다 Switch로 처리한다 (필요 시)*/
    [Header("MonsterAttackDesc")]
    [SerializeField] List<MonsterAttackDesc> _monsterAttackDescList;
    int _nowSkillIndex=0;
    float _coolDownTime = 0;
    [SerializeField] float _minAttackAngle = 10f;
    
    Animator _animator;
    int _skillAnimFullpathHash;
    

    public void Awake()
    {
        _behavioragent = GetComponent<BehaviorGraphAgent>();
        _behaviorBattle = GetComponentInParent<EQSQuerierBehaviour_Battle>();
        _querier = GetComponentInParent<EQSQuerier>();
 
        List<MonsterAttackDesc> copyMonsterAttackDesc = new List<MonsterAttackDesc> (_monsterAttackDescList); 

        /*스킬 순서 동기화*/
        for(int i=0; i< copyMonsterAttackDesc.Count; ++i)
        {
            _monsterAttackDescList[copyMonsterAttackDesc[i].SkillIdx] = copyMonsterAttackDesc[i];
        }
        _animator=GetComponent<Animator>();
        _skillAnimFullpathHash = Animator.StringToHash("Base Layer.BattleLocomotion");

    }
    /*조건은 여기서 설정*/

    public void UpdateAttack()
    {
        //조건만 여기에서 설정하고 실제 공격은 BehaviorTree에서 처리한다. SkillCode에 따라서 처리한다.  Controller 
        EQSPoint point = _querier.OccupingPoint;
        if (null == point) return;
        if (null == point.occupant) return;
        if (0 != point.ringIndex) return;
        Vector3 toTarget = _querier.Target.transform.position - _querier.transform.position;
        Vector3 toSrc = _querier.transform.forward;
        float nowAngle = Vector3.Angle(toTarget, toSrc);
        if (nowAngle > _minAttackAngle) return;
        if (_skillAnimFullpathHash !=_animator.GetCurrentAnimatorStateInfo(0).fullPathHash) return;

       


        _behavioragent.BlackboardReference.GetVariableValue<int>(BlackboardKeys.SKILL_NO, out _nowSkillIndex);
        MonsterAttackDesc nowSkill = _monsterAttackDescList[_nowSkillIndex];

        if (Time.time - _coolDownTime  >  _monsterAttackDescList[_nowSkillIndex].CooldownTime  )
        {
            /*쿨타임 지나고 스킬 시전*/
            _coolDownTime =Time.time;
            _behavioragent.SetVariableValue(BlackboardKeys.ENEMY_STATE, eEnemyState.ATTACK);
            return;
        }

        

        
    }
}
