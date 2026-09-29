using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Build.Content;
#endif

using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
public enum eCharacterType
{
    PLAYER,
    MONSTER,
    OBJECT,
}

public enum eBuff
{
    DAMAGE_UP,
    DEFECNSE_UP,
    
    MAXMANA_UP,
    MAXHEALTH_UP,
    
    RECOVER_CURRENTHEALTH,
    RECOVER_CURRENTMANA,

    CRITICALCHANCE_UP,
    CRITICALDAMAGE_UP,
    BUFF_END,
}
public class BuffDesc //나중에 풀링용으로 분리
{
    public eBuff _buffType;
    public float _buffValue;
    public float _buffDuration;
    public float _buffStartTime;
    public bool  _isBuffDone =false;
    public bool _isOnlyOnce = false;
    Delegate  _nowPlay;
    private StatusScript _statusScript;
    public void Initilaize(StatusScript statusScript,eBuff buffType, float buffValue, float buffDuration, bool isOnlyOnce)
    {
        _buffType = buffType;
        _buffValue = buffValue;
        _buffDuration = buffDuration;
        _buffStartTime = Time.time;
        _isOnlyOnce = isOnlyOnce;
        _statusScript = statusScript;
        StartBuff();
    }
    

    public void StartBuff()
    {
        _buffStartTime = Time.time;
        _isBuffDone = false;

        switch (_buffType)
        {
            case eBuff.DAMAGE_UP:
                _statusScript.AttackDamage += _buffValue;
                break;
            case eBuff.DEFECNSE_UP:
                _statusScript.Defense += _buffValue;
                break;
            case eBuff.MAXMANA_UP:
                _statusScript.MaxMana += _buffValue;
                _statusScript.CurMana += _buffValue;
                break;
            case eBuff.MAXHEALTH_UP:
                _statusScript.MaxHealth += _buffValue;
                _statusScript.CurHealth += _buffValue;
                _statusScript.OnHealthChangedEvent?.Invoke(_statusScript, 3f);
                break;
            case eBuff.RECOVER_CURRENTHEALTH:
                _statusScript.CurHealth += _buffValue;
                if(_statusScript.CurHealth > _statusScript.MaxHealth )
                {
                    _statusScript.CurHealth = _statusScript.MaxHealth;
                }
                _statusScript.OnHealthChangedEvent?.Invoke(_statusScript, 3f);
                _isOnlyOnce = true; 
                break;
            case eBuff.RECOVER_CURRENTMANA:
                _statusScript.CurMana += _buffValue;
                if(_statusScript.CurMana > _statusScript.MaxMana )
                {
                    _statusScript.CurMana = _statusScript.MaxMana;
                }
                _isOnlyOnce = true; 
                break;
            case eBuff.CRITICALCHANCE_UP:
                _statusScript.CriChance += _buffValue;
                break;
            case eBuff.CRITICALDAMAGE_UP:
                _statusScript.CriDamage += _buffValue;
                break;

        }
    }
    public bool playBuff()
    {
        if(true ==_isOnlyOnce ||Time.time - _buffStartTime >= _buffDuration)
        {
            _isBuffDone = true;
        }
        return _isBuffDone;
    }
    public void BuffEnd()
    {
        switch(_buffType)
        { 
            case eBuff.DAMAGE_UP:
                _statusScript.AttackDamage -= _buffValue;
                break;
            case eBuff.DEFECNSE_UP:
                _statusScript.Defense -= _buffValue;
                break;
            case eBuff.MAXMANA_UP:
                _statusScript.MaxMana -= _buffValue;
                if(_statusScript.CurMana > _statusScript.MaxMana )
                {
                    _statusScript.CurMana = _statusScript.MaxMana;
                }
                break;
            case eBuff.MAXHEALTH_UP:
               
                _statusScript.MaxHealth -= _buffValue;
                if(_statusScript.CurHealth > _statusScript.MaxHealth )
                {
                    _statusScript.CurHealth = _statusScript.MaxHealth;
                }
                _statusScript.OnHealthChangedEvent?.Invoke(_statusScript, 3f);
                break;
            case eBuff.CRITICALCHANCE_UP:
                _statusScript.CriChance -= _buffValue;
                break;  
            case eBuff.CRITICALDAMAGE_UP:
                _statusScript.CriDamage -= _buffValue;
                break;
        }
        
    }
    
}

public class StatusScript : MonoBehaviour //캐릭터 고유의 값
{
    [SerializeField] eCharacterType characterType;
    [SerializeField] int _characterID;
    public Action<StatusScript, float> OnHealthChangedEvent { get; set; }
    public  Action<StatusScript, Player.ePlayerSlider, float> OnPlayerStatusChangedEvent { get; set;}
    
    /*Todo 늘어나면 몬스터 스크립트 플레이어 스크립트 나누기 */
    List<BuffDesc> _activeBuffs = new List<BuffDesc>();
    List<BuffDesc> _buffsToRemove = new List<BuffDesc>();
    private float _maxHealth; public float MaxHealth { get => _maxHealth; set => _maxHealth=value; }
    private float _curHealth; public float CurHealth { get => _curHealth; set => _curHealth=value; }
    private float _maxMana; public float MaxMana { get => _maxMana; set => _maxMana=value; }
    private float _curMana; public float CurMana { get => _curMana; set => _curMana=value; }

    private float _defense; public float Defense { get => _defense; set => _defense = value; }
    private int _level; public int Level { get => _level; set => _level = value; } //level up할때마다 갱신
    private float criDamage; public float CriDamage { get => criDamage; set => criDamage=value; }
    private float criChance; public float CriChance { get => criChance; set => criChance=value; }
    private float attackDamage; public float AttackDamage { get => attackDamage; set => attackDamage=value; }

 

    private bool _isDamageChanging; public bool IsDamageChanging { get => _isDamageChanging; set => _isDamageChanging =value; }
    private bool _isDead; public bool IsDead{ get => _isDead; set => _isDead=value; }

    private bool _gotHit; public bool GotHit { get => _gotHit; set => _gotHit=value; }


    public int CharacterID { get => _characterID; set => _characterID = value; }


    public void AddBuff(eBuff buffType,float buffValue,float buffDuration,bool isOnlyOnce)
    {
        BuffDesc buffDesc = new BuffDesc(); //TODO : Object Pooling

        buffDesc.Initilaize(this,buffType, buffValue, buffDuration, isOnlyOnce);
        _activeBuffs.Add(buffDesc);
    }
    public void ClearAllBuffs()
    {
        _activeBuffs.Clear();
        _buffsToRemove.Clear();
    }
   
    public void Update()
    {
        foreach(var buff in _activeBuffs)
        {
            if (true ==buff.playBuff())
            {
                buff.BuffEnd();
                _buffsToRemove.Add(buff);
            }
        }
        foreach(var buff in _buffsToRemove)
        {
            _activeBuffs.Remove(buff);
        }
        _buffsToRemove.Clear();
    }
    public void LoadScriptInfo()
    {
        switch (characterType)
        {
            case eCharacterType.PLAYER:
                {
                    PlayerInfo playerInfo = Managers.Player.PlayerInfo;

                    _maxHealth =playerInfo.MaxHP;
                    _curHealth =playerInfo.MaxHP;
                    _maxMana=playerInfo.MaxMP;
                    _curMana=playerInfo.MaxMP;

                    _defense =playerInfo.Defense;
                    _level =playerInfo.Level; //level up할때마다 갱신
                    criChance=playerInfo.CriChance;
                    criDamage=playerInfo.CriDemage;
                    attackDamage = playerInfo.Attack;
                    break;
                }
            case eCharacterType.MONSTER:
                {
                    MonsterInfo monsterInfo = Managers.Monster.GetMonsterInfo(_characterID);
                    _maxHealth=monsterInfo.MaxHP;
                    _curHealth=monsterInfo.MaxHP;
                    _defense=monsterInfo.Defense;
                    _level=monsterInfo.Level;
                    criChance=monsterInfo.CriChance;
                    criDamage=monsterInfo.CriDemage;
                    attackDamage =monsterInfo.Attack;
                    break;
                }
            case eCharacterType.OBJECT:
                {
                    break;
                }
        }

        EnemyDetector _ed = GetComponent<EnemyDetector>();
        _ed?.ClearAllEnemies();
    }

    public void Awake()
    {
        LoadScriptInfo();
        
        
    }
  
}
