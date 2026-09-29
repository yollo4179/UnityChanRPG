using NUnit.Framework;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using System;
using Unity.Behavior;
using System.Linq;

public class SmaugBattleScript : MonsterBattleScript
{

    public enum eSmaugMovement
    { 
        Chase,
        Stop,
        //MoveToClosestWayPoint,
    }

    [SerializeField]private float _stoppingDuration; //20초 정도
    BehaviorGraphAgent _btAgent; 
    private float _stoppingStartTime;
    private bool _isStopping;
    private eSmaugMovement _nowMovementType;
    private eSmaugMovement _nowMovementTypeAfterStop = eSmaugMovement.Chase;
    List<Vector3> wayPoints;
    bool _bMoveToWayPoint;
    Vector3 _targetWayPoint;
    float _stoppingDistance = 3f;
    public override void Awake()
    {
        base.Awake();
        _btAgent = GetComponent<BehaviorGraphAgent>();

    }
    public override void UpdateBattle(bool isFirst) 
    {


        Vector3 toTarget = VectorUtil.PlatVector(_target.transform.position - transform.position);
        float lengthToTargetOnXZ = toTarget.magnitude;
        toTarget.Normalize();

        Vector3 targetDirection = MovingLogic(lengthToTargetOnXZ, toTarget, isFirst);
        targetDirection = Vector3.RotateTowards(targetDirection, toTarget, Time.deltaTime*3f, Time.deltaTime*5f);
        UpdateMovement(targetDirection, targetDirection);
        UpdateRotation(targetDirection, targetDirection);
        ChooseAnimation(toTarget);

        _attackChecker.CheckAttackCondition();

    }
    public override void UpdateMovement(Vector3 direction, Vector3 lookDir)
    {
        if (direction.sqrMagnitude < 0.01f) return;
        {
            // 직접 이동
            transform.position += direction * _monsterSpeed * Time.deltaTime;
        }

    }
    public void UpdateRotation(Vector3 direction, Vector3 lookDir)
    {
        if (_nowMovementType == eSmaugMovement.Stop) return; 
            // 부드러운 회전
            if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    public Vector3 MovingLogic(float lengthToTargetOnXZ ,Vector3 toTarget,bool isFirst)
    {
        switch (_nowMovementType)
        {
            case eSmaugMovement.Chase:
                {
                    if (lengthToTargetOnXZ < _rangeOffset)
                    {
                        _nowMovementType=eSmaugMovement.Stop;
                        _stoppingStartTime = Time.time;
                        break;
                    }
                    _nowState = eNowState.MoveF;
                    return toTarget;
                }
            case eSmaugMovement.Stop: 
                {
                    _nowState = eNowState.Stop;
                    if (Time.time - _stoppingStartTime > _stoppingDuration) 
                    {
                        _nowMovementType =
                          (eSmaugMovement)
                          (((int)_nowMovementType + 1) % Enum.GetValues(typeof(eSmaugMovement)).Length);
                    }
                    return Vector3.zero; 
                }
            
                

        }
        

        return Vector3.zero;
    }
}
