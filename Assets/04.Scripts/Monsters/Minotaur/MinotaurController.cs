using UnityEngine;
using System.Collections.Generic;


namespace Minotaur
{
    public enum eMinotaurState
    {
        STATE_ATTACK0_SWING1 = 0,
        STATE_ATTACK1_SWING2 = 1,
        STATE_ATTACK2_SWING3 = 2,
        STATE_ATTACK0_KICK1 = 3,
        STATE_ATTACK1_KICK2 = 4,
    }
    [System.Serializable]
    public class SkillSet
    {
        [SerializeField] public SkillSO skillSO;
        [SerializeField] public eMinotaurState minoState;
        [SerializeField] public bool isState;
    }
}
public class MinotaurController : MonsterController
{


    [SerializeField] List<Minotaur.SkillSet> _minotaurSkillSet = new List<Minotaur.SkillSet>();

    public void Awake()
    {
        _dicSkillDescs=new Dictionary<int, SkillSO>();
        foreach (var minoSOPair in _minotaurSkillSet)
        {
            _dicSkillDescs.Add((int)minoSOPair.minoState, minoSOPair.skillSO);
        }

    }

}
