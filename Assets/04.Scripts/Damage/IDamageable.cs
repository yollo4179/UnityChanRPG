using UnityEngine;

public struct DamageInfo
{
    public GameObject Source; // The source of the damage (e.g., the attacker)
    public float baseDamage;
    public float weaponDamage;
    public string damageType; 
    public bool isCritical;
    public float criticalChance;
    public float criticalDamage;

    public LayerMask hitLayerMask;
    public LayerMask srcLayerMask;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public EntityId attackerID;  //공격 식별자 ( 중복 공격 방지)
    public int attackID;

    public SkillHitInfo skillHitInfo;
}


public interface IDamageable 
{

    bool TakeDamage(in DamageInfo damageInfo);
    
}
