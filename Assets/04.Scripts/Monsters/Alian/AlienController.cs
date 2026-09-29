using System.Collections.Generic;
using UnityEngine;
namespace Alien
{
    public enum eAlienState
    {
        STATE_ATTACK0_HOOK_RIGHT = 0,
        STATE_ATTACK1_HOOK_LEFT = 1,
        STATE_ATTACK2_JUMP = 2,
       
    }
    [System.Serializable]
    public class SkillSet
    {
        [SerializeField] public SkillSO skillSO;
        [SerializeField] public eAlienState alienState;
        [SerializeField] public bool isState;
    }
}
public class AlienController : MonsterController
{
    [SerializeField] List<Alien.SkillSet> _alienSkillSet = new List<Alien.SkillSet>();
    Animator _animator;
    [SerializeField]bool _lockY = true;
    [SerializeField] bool _lockRotationY = false;
    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _dicSkillDescs =new Dictionary<int, SkillSO>();
        foreach (var minoSOPair in _alienSkillSet)
        {
            _dicSkillDescs.Add((int)minoSOPair.alienState, minoSOPair.skillSO);
        }
        _animator.applyRootMotion= true;

    }
    public void OnAnimatorMove()
    {
        Vector3 deltaPos = _animator.deltaPosition;
        
        if (_lockY)
            deltaPos.y =0;
        else
        {
            Debug.Log($"<color =#ffff00>RootY{deltaPos.y}</color>");
        }
        transform.position+=deltaPos;

        /*루트모션 회전 제어*/
        Quaternion dr = _animator.deltaRotation;
        if (_lockRotationY)
        {
            // 예: yaw만 반영하고 pitch/roll 제거
            Vector3 e = dr.eulerAngles;
            dr = Quaternion.Euler(0f, e.y, 0f);
        }
        transform.rotation =dr*transform.rotation;
        // GetComponent<CharacterController>().Move(deltaPos);
    }
}