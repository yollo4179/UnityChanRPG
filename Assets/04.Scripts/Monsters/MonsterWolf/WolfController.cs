using UnityEngine;
using System.Collections.Generic;


namespace Wolf {
    public enum eWolfState
    {
        STATE_ATTACK0_BUFF = 0,
        STATE_ATTACK1_CLAW = 1,
        STATE_ATTACK2_POKE = 2,
    }
    [System.Serializable] public class SkillSet
    {
        [SerializeField] public SkillSO skillSO;
        [SerializeField] public eWolfState playerState;
        [SerializeField] public bool isState;
    }
}
public class WolfController : MonsterController
{
    

    [SerializeField]List<Wolf.SkillSet> _wolfSkillSet=new List<Wolf.SkillSet>();

    public void Awake()
    {
        _dicSkillDescs=new Dictionary<int, SkillSO>();
        foreach ( var wolfSOPair in _wolfSkillSet)
        {
            _dicSkillDescs.Add((int)wolfSOPair.playerState, wolfSOPair.skillSO);
        }
         
}

}
