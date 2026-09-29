using UnityEngine;

public class Projectile : MonoBehaviour
{
    LayerMask mask;
    protected SkillSO _skillSO;
    protected SkillHitInfo _skillHitInfo=new();
    public void SetSkillSO(SkillSO skillSO)
    {

        _skillSO = skillSO;
        
    }
    public void Init() { }
    public void SetCollisionMask()
    {

    }

    protected  void SetHitBoxInfo()
    {
        
        PlayerSkillInfo playerSkillInfo = Managers.Player.GetSkillInfoByHandle(_skillSO.handle);

        //skillSO마다 스킬 능력치 계산법 재정의해서 넘기기 TO DO


        _skillHitInfo._skillDamage = playerSkillInfo.BaseDamage* playerSkillInfo.Level;
        _skillHitInfo._skillCirticalChance =playerSkillInfo.ExtraCriticalChance* playerSkillInfo.Level;
        _skillHitInfo._skillCriticalDamage=playerSkillInfo.ExtraCriticalDamage *playerSkillInfo.Level;
        _skillHitInfo._numHits= playerSkillInfo.NumberOfHit;
        _skillHitInfo._skillLevel =playerSkillInfo.Level;
        
       
    }
}
