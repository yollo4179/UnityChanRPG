using System.Collections.Generic;
using UnityEngine;
namespace Smaug
{
    public enum eSmaugState
    {
        STATE_ATTACK0_WING = 0,
        STATE_ATTACK1_HEAD = 1,
        STATE_ATTACK2_FLAME = 2,
        STATE_ATTACK3_FLY_FLAME =3,

    }
    [System.Serializable]
    public class SkillSet
    {
        [SerializeField] public SkillSO skillSO;
        [SerializeField] public eSmaugState eSmaugState;
        [SerializeField] public bool isState;
    }
}
public class SmaugController : MonsterController
{
    [SerializeField] List<Smaug.SkillSet> _smaugSkillSet = new List<Smaug.SkillSet>();
    Animator _animator;
    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _dicSkillDescs =new Dictionary<int, SkillSO>();
        foreach (var msmaugSOPair in _smaugSkillSet)
        {
            _dicSkillDescs.Add((int)msmaugSOPair.eSmaugState, msmaugSOPair.skillSO);
        }
        _animator.applyRootMotion= true;

    }
}
