using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
#if UNITY_EDITOR
using UnityEditor.Rendering.LookDev;
using static UnityEditor.Experimental.GraphView.GraphView;
#endif
using Unity.VisualScripting;
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SmaugFlyAction", story: "Smaug takes off, Flies for a moment and lands after breathing fire .", category: "Action", id: "67a0a88ecedd44ade2f0fa3ad0fccabb")]
public partial class SmaugFlyAction : Action
{
    [SerializeReference] public  BlackboardVariable<GameObject> _self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> _wayPoints;
    [SerializeReference] public BlackboardVariable<float> _floatingDuration; //floating
    [SerializeReference] public BlackboardVariable<int> _numMaxVisitPoint;
    [SerializeReference] public BlackboardVariable<float> _flyingSpeed;
    [SerializeReference] public BlackboardVariable<float> _stoppingDistance;
    [SerializeReference] public BlackboardVariable<int> _flySkillNO;
    
    Animator _animator; 
    private bool    _hasInitialized = false;
    private int     _targetIndex;
    private Vector3 _targetPoint;
    private int     _numWayPoints;
    private int     _numVisitPoints;
    private int     _numGoalVisitPoints;
    private bool    _isNowMoving;
    /*Hashes*/
    private int     _gliderTriggerHash;
    private int     _onFlyTriggerHash;
    private int     _InPlaceTriggerHash;
   
    private int     _gliderHash;
    private int     _takeOffHash;
    private int     _floatingHash;
    private int     _flyForwardHash;
    private int     _battleLocomotionHash;
    Transform       _player;
    private float _startTime;
    private bool _isWaitingTimeEnd = false;
    private int _numFlyModes = 2;
    bool _isWayPointMode;
    bool _animEnd = false;
    protected override Status OnStart()
    {

        if (null== _animator) _animator = _self.Value.GetComponent<Animator>();
        _isWaitingTimeEnd  = false;
        _startTime     = Time.time;
        _numVisitPoints = 0;
        _numGoalVisitPoints = UnityEngine.Random.Range(1, Mathf.Max(2, _numMaxVisitPoint.Value + 1));
        _isNowMoving = false;
        _flySkillNO.Value =1;
        _animator.SetInteger("FlySkillNO", _flySkillNO.Value);
        switch (_flySkillNO.Value)
        {
            case 0:
                _isWayPointMode =false;
                break;
            case 1:
                _isWayPointMode =true;
                break; 
        
        }
        if (_hasInitialized)
        {
            CheckAndActivateHash(_battleLocomotionHash, _onFlyTriggerHash);
            return Status.Running;
        }
        _hasInitialized = true;
        _gliderTriggerHash = Animator.StringToHash("OnGlide");
        _onFlyTriggerHash = Animator.StringToHash("OnFly");
        _InPlaceTriggerHash =  Animator.StringToHash("InPlace");
  

        _gliderHash =Animator.StringToHash("Base Layer.FlyLocomotion.Fly Glide");
        _takeOffHash=Animator.StringToHash("Base Layer.FlyLocomotion.Take Off");
        _floatingHash=Animator.StringToHash("Base Layer.FlyLocomotion.Fly Float");
        _flyForwardHash = Animator.StringToHash("Base Layer.FlyLocomotion.Fly Forward");
        _battleLocomotionHash = Animator.StringToHash("Base Layer.BattleLocomotion");
        _player = GameObject.FindGameObjectWithTag("Player").transform;
         _numWayPoints =  _wayPoints.Value.Count;

        CheckAndActivateHash(_battleLocomotionHash, _onFlyTriggerHash);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time - _startTime > 30f)
        {
            Debug.LogWarning("Smaug flight timed out; moving to aerial attack", _self.Value);
            _animator.CrossFade("Base Layer.FlyLocomotion.Fly Float", 0.15f);
            return Status.Success;
        }

        switch (_isWayPointMode)
        {
            case true:
                DoWayPointsRoute();break;
            case false:
                DoInPlaceRoute();break;
        }

        UpdateAnimation();
        if(true ==_isWaitingTimeEnd&&RotateToPlayerAndCheckExitPoint())
            return Status.Success;
        return Status.Running;
    }

    protected override void OnEnd()
    {
        //_animator.SetBool(_gliderTriggerHash, false);
        //_animator.SetBool(_InPlaceTriggerHash, false);
        //_animator.SetBool(_takeOffTriggerHash, false);
        //_animator.SetBool(_flyForwardTriggerHash, false);
        _animEnd = false;
        _flySkillNO.Value =1;//(_flySkillNO.Value+1) %_numFlyModes;
    }
    private void DoInPlaceRoute()
    {
        if(Time.time -_startTime >_floatingDuration.Value)
        {
            _isWaitingTimeEnd  = true; 
        }

    }
    private void DoWayPointsRoute()
    {
        if (_isWaitingTimeEnd) return;
        GetRandomWayPoints();
        MoveToWayPoint();
        if(CheckIfArrived())
            IncreaseNumVisitPoint();
       

      
    }
    private void GetRandomWayPoints()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash ==_takeOffHash) return;
        if (true ==_isNowMoving) return;

        _isNowMoving = true;
        _targetIndex = UnityEngine.Random.Range(0, _numWayPoints);
        _targetPoint = _wayPoints.Value[_targetIndex].transform.position;


        if (_numVisitPoints ==_numGoalVisitPoints-1)
        {
            int bestPointIndex = 0;
            float length = float.MaxValue;

            for (int i = 0; i<_wayPoints.Value.Count; ++i)
            {
                float candidateDistance = (_player.position - _wayPoints.Value[i].transform.position).sqrMagnitude;
                if (candidateDistance < length)
                {
                    length = candidateDistance;
                    bestPointIndex = i;
                }
            }
            _targetPoint =  _wayPoints.Value[bestPointIndex].transform.position;
        }
    }

    private void MoveToWayPoint()
    {
        if (false ==_isNowMoving) return ;
        if (_numVisitPoints>=_numGoalVisitPoints) return;
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash ==_takeOffHash) return;

        Vector3 targetDir = _targetPoint -  _self.Value.transform.position;
        targetDir.Normalize();
        _self.Value.transform.position = Vector3.MoveTowards(
            _self.Value.transform.position, _targetPoint, _flyingSpeed.Value * Time.deltaTime);
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            _self.Value.transform.rotation
                = Quaternion.Slerp(_self.Value.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    public bool  RotateToPlayerAndCheckExitPoint()
    {
        //if (false != _animator.GetBool(_floatingHash)) return false;

        //if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash ==_floatingHash) return false ;

        Vector3 targetDir = _player.position -  _self.Value.transform.position;
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            _self.Value.transform.rotation
                = Quaternion.Slerp(_self.Value.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        if(Mathf.Rad2Deg *
            Mathf.Acos(Vector3.Dot(targetDir.normalized , _self.Value.transform.forward.normalized))<10)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _floatingHash)
            {
                return true;
            }
        }
        return false; 
    }
  
    private bool CheckIfArrived()
    {
        float length = (_self.Value.transform.position - _targetPoint).magnitude;
        if (true ==_isNowMoving &&length <_stoppingDistance)
        {
            _isNowMoving= false; 
            return true;
        }
        return false; 
    }
    public void IncreaseNumVisitPoint()
    {
        ++_numVisitPoints;
    }

    public bool CheckAndActivateHash (int currentSupposedToBe, int triggerHash)
    {
        if (
            _animator.GetCurrentAnimatorStateInfo(0).fullPathHash == currentSupposedToBe)
        {
            _animator.SetTrigger(triggerHash);
            return true; 
        }
        return false; 
    }
    public void UpdateAnimation()
    {
        
       
        if (_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == _flyForwardHash) { 
            int a = 0; }
        
        switch (_isWayPointMode)
        {
            case true:
           
            
            
            
                    if (1 == _numVisitPoints && _numVisitPoints <_numGoalVisitPoints)
                    {
                        CheckAndActivateHash(_flyForwardHash,_gliderTriggerHash);
                        break;
                    }                    
                    else if(1 == _numGoalVisitPoints )
                    {
                        CheckAndActivateHash(_flyForwardHash , _InPlaceTriggerHash);
                        break;
                    }
            

            if(_numGoalVisitPoints ==_numVisitPoints)
            {
                    _isWaitingTimeEnd =true;
                    CheckAndActivateHash(_gliderHash, _InPlaceTriggerHash);
            }
            break;

            case false:
                CheckAndActivateHash(_floatingHash, _InPlaceTriggerHash);

                break;
        }
        
    }


    
}

