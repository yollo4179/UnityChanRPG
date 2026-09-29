using UnityEngine;

using Newtonsoft.Json;
using UnityEditor;
using System.IO;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;
using Newtonsoft.Json.Bson;

public class PlayerSkillInfo
{
    private int skillID;
    private string skillName;
    private int level;
    private float extraCriticalDamage;
    private float extraCriticalChance;
    private int numberOfHit;
    private float extraDamage;
    private float baseDamage; 

    public int SkillID { get => skillID; set => skillID =value; }
    public string SkillName { get => skillName; set =>skillName =value; }
    public int Level { get => level; set => level = value; }    
    public float ExtraCriticalChance { get=> extraCriticalChance; set => extraCriticalChance = value; }
    public float ExtraCriticalDamage { get => extraCriticalDamage; set => extraCriticalDamage= value; }
    public float ExtraDamage { get =>extraDamage; set => extraDamage = value; }
    public float BaseDamage { get => baseDamage; set => baseDamage = value; }

   public int NumberOfHit { get => numberOfHit; set => numberOfHit = value; }


}
