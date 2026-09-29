using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using UnityEngine;

/**/
[System.Serializable]public enum eSkillType
{
    BASE,
    RANGED,
    MELEE,
    BUFF,
}

[System.Serializable]public enum eAnimEvent
{
    TRAIL,
    COLLIDER,
    EFFECT,//혹은 페어로 관리
    JUMP,
    LAND,
    SUMMON_PROJECTILES,
    ON_SKILL,
    END
}
[System.Serializable] public struct EffectSetting{
    [SerializeField] public Vector3 localPosition;
    [SerializeField] public Vector3 localRotation;
    [SerializeField] public Vector3 localScale;
    [SerializeField] public bool isLoop;
    [SerializeField] public bool delayEffectActive;
    [SerializeField] public bool isOnWeapon;
    [SerializeField] public int hitNum;
    //FadeIn FadeOut //  Durartion 

} 
[System.Serializable]public struct AnimEventDesc
{
    [SerializeField] public eAnimEvent eventName; 
    [SerializeField] public float startTime;
    [SerializeField] public float endTime;

    [SerializeField] public string  poolingEffectName;
    [Header("EffectOptions")]
    [SerializeField] public EffectSetting effectSetting;
    

    [Header("Projectiles")]
    [SerializeField] public string poolingProjectileKey; // 프로젝타일 프리펩 키를 알려주시오 ( 등록은  Load 코드에서 풀링으로 등록) 
    [SerializeField] public int numProjectiles; //소환할 불릿 수를 알려주시오
    
    [SerializeField] public string[] spawnSocket; //소켓의 이름을 알려주시오 

    [Header("Collider")]
    [SerializeField]  public Vector3 colCenter;
    [SerializeField] public Vector3 colSize;
   
    //이펙트 프리팹
}
[System.Serializable]public struct EventClipInfo
{
    [SerializeField] public string clipName;
    [SerializeField] public List<AnimEventDesc> events;
    [SerializeField] public string animFullPath;
    [SerializeField] public eHitAreaMarker hitArea;
}
[System.Serializable]public struct EventSequence
{
    [SerializeField] public List<EventClipInfo> eventClips;
}
[System.Serializable]public class BuffInfo
{
    [SerializeField] public eBuff buffType;
    [SerializeField] public float duration;
    [SerializeField] public float value; //강화 수치
}


[System.Serializable, CreateAssetMenu(fileName = "SkillSO", menuName = "Scriptable Objects/SkillSO")]
public class SkillSO : ScriptableObject
{
    [SerializeField] EventSequence stateNodeEvents;
    [SerializeField] Sprite skillIcon;
    [SerializeField, TextArea(2, 5)] string skillName;
    [SerializeField, TextArea(2, 5)] string skillDesc;
    [SerializeField, TextArea(2, 5)] string skillClass;
    [SerializeField] float coolTime;
    [SerializeField] float manaCost;
    [SerializeField] float range;
    [SerializeField] float damage;
    [SerializeField] float damageIncreasePercentPoint;
    [Header("Buff")]
    [SerializeField] BuffInfo[] buffInfo;
    [SerializeField] int skillNO;
    [SerializeField] int skillLevel;
    
    [SerializeField] int openLevel;
    [SerializeField] eSkillType skillType;

    [Header("Handle")]
    [SerializeField] public int handle;          // IndexKey: 저장/네트워크/조회용 “안정 ID”
    [SerializeField] public string alias;




    public EventSequence EventSequence => stateNodeEvents;
    public Sprite SkillIcon => skillIcon;
    public string SkillName => skillName;
    public string SkillDesc => skillDesc;
    public string SkillClass => skillClass;
    public float CoolTime => coolTime;
    public float ManaCost => manaCost;
    public float Range => range;
    public float Damage => damage;


    public int SkillNO => skillNO;
    public int OpenLevel => openLevel;
    public float DamageIncreasePercentPoint { get=> damageIncreasePercentPoint; set=> damageIncreasePercentPoint =value; }

    public eSkillType SkillType { get => skillType; }
    public BuffInfo[] BuffInfo { get => buffInfo; set => buffInfo = value; }
    public int SkillLevel { get=> skillLevel; set=> skillLevel =value; }

}
