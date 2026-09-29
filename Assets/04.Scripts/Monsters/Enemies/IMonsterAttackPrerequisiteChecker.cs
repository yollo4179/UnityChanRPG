using System;
using UnityEngine;

[System.Serializable]public class MonsterAttackDesc
{
    [SerializeField]public string   SkillName ;
    [SerializeField]public float    CooldownTime;
    [SerializeField] public float   MinRange;
    [SerializeField] public float   MaxRange;
    [SerializeField] public int     ringPreference;  //0:근접 1:중거리 2:원거리
    [SerializeField] public int     SkillIdx;
    [SerializeField] public int     HPPreference;
    public                  float   skillStartTime;

}

public interface IMonsterAttackPrerequisiteChecker
{
    public void UpdateAttack(); 
}
