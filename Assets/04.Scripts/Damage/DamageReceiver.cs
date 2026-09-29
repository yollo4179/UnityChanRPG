using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum eDamageReceiverType
{
    PLAYER,
    MONSTER,
    OBJECT,
}

public  class  DamageReceiver : MonoBehaviour ,IDamageable
{

    [SerializeField] eDamageReceiverType damageReceiverType;
    


    protected StatusScript _status;
    public bool isGuarding = false;
    public float guardReduction = 0.6f;
    public float guardAngle = 120f;
   

    Dictionary<(EntityId, int), float> _dicAttackTimeTable = new Dictionary<(EntityId, int), float>();
    float _attackAvailableDuration = 0.5f;
    Coroutine _clearCo = null;
    float _clearTTL = 1.2f;  // TTL = 재히트창 + 버퍼 (예: 0.2~1.0s)
    readonly WaitForSeconds _wait1s = new WaitForSeconds(1f);
    private readonly List<(EntityId, int)> _toClear = new();

    DamageFontsDesc dmgFontsDesc = new DamageFontsDesc();

    /*데미지 폰트 뿐 아니라 죽음 이벤트 발생 시에 아이템도 뿌려야 한다*/
    ObjectDropTable _objDropTable;

    protected virtual void Awake()
    {
        _status = GetComponent<StatusScript>();
        _objDropTable =GetComponent<ObjectDropTable>(); 


    }


    private IEnumerator ClearHistoryLoop()
    {
        while (true)
        {
            //지우는 개수 한정하여 천천히 지우는 것도 가능
            yield return _wait1s;
            float now = Time.time;
            _toClear.Clear();

            foreach (var pair in _dicAttackTimeTable)
                if (now - pair.Value > _clearTTL)
                    _toClear.Add(pair.Key);

            for (int i = 0; i < _toClear.Count; i++)
                _dicAttackTimeTable.Remove(_toClear[i]);
        }
    }

    protected virtual  void OnEnable()
    {
        if (_clearCo == null) 
            _clearCo = StartCoroutine(ClearHistoryLoop()); //1초마다 공격 클리어
    }
    protected virtual void OnDisable()
    {
        if (_clearCo != null)
        {
            StopCoroutine(_clearCo);
            _clearCo = null;
        }
    }

    public bool TakeDamage(in DamageInfo damageInfo)
    {

        var key = (damageInfo.attackerID, damageInfo.attackID);
        float now = Time.time;

        //if (null == _clearCo)
        //    _clearCo = StartCoroutine(clearHistory());

        if (_dicAttackTimeTable.TryGetValue(key, out float lastTime))
        {
            if (now - lastTime < _attackAvailableDuration)
                return false; // 재히트 창 미도달 → 거부

            _dicAttackTimeTable[key] = now; // 재허용 및 갱신
        }
        else
        {
            // 첫 히트 허용 + 등록
            _dicAttackTimeTable.Add(key, now);
        }
        /*데미지 계산 */

        /*스킬 공격이었다.*/
        if (null != damageInfo.skillHitInfo)
        {
            for (int i = 0; i<damageInfo.skillHitInfo._numHits; ++i) {
                var ret = CaculateDamage(damageInfo, true);
                dmgFontsDesc.AddFontInfo(ret.Item1, ret.Item2);
            }
        }
        else
        {
            var ret = CaculateDamage(damageInfo);
            dmgFontsDesc.AddFontInfo(ret.Item1, ret.Item2);
        }

        dmgFontsDesc.SetPosition(transform);
        Managers.UI.GetOpenUIByName("UI_SSDamageFonts_Canvas").GetComponent<UI_SSDamageFonts>().FontMaker(dmgFontsDesc);
        dmgFontsDesc.ClearFontsDesc();



        if (_status.CurHealth<= 0)
        {
            Die();
        }

        _status.GotHit = true;

        switch (damageReceiverType) {
            case eDamageReceiverType.MONSTER:
                _status.OnHealthChangedEvent?.Invoke(_status, 3f);//공격의 타입에 따라서 얼마나 빨리 줄어들게 할지 결정
                break;
            case eDamageReceiverType.PLAYER:
                _status.OnPlayerStatusChangedEvent?.Invoke(_status, Player.ePlayerSlider.HEALTH, 3f);
                break;
    }
        return true;
    }
    public (int ,bool) CaculateDamage(in DamageInfo damageInfo, bool isSkill = false)
    {
        bool isCritical = false;
        float extraDamage = 0;
        float extraCriticalDamage = 0;
        if( isSkill &&null != damageInfo.skillHitInfo)
        {
            extraDamage +=  damageInfo.skillHitInfo._skillDamage;
            extraCriticalDamage += damageInfo.skillHitInfo._skillCriticalDamage;
        }


        float damage = damageInfo.weaponDamage+damageInfo.baseDamage +extraDamage;
        damage = ApplyArmor(damageInfo.baseDamage, _status.Defense);

        if (tryCritical(damageInfo, isSkill))
        {
            isCritical =true; 
            float criticalFactor = damageInfo.criticalDamage +extraCriticalDamage;
            float offset = UnityEngine.Random.Range(0f, (criticalFactor)/100f);
            damage = damage * (1 + offset + (criticalFactor/100f));
        }
        damage =  TryGuardReduction(damage, damageInfo);

        _status.CurHealth-= damage;
        _status.CurHealth = Mathf.Clamp(_status.CurHealth, 0, _status.MaxHealth);

        return ((int)damage, isCritical);
    }

    float ApplyArmor (float damage, float armor)
    {
        //*100퍼는 절대 되지 않도록*/
        float damageReduction = armor / (armor + 100f);
        float finalDamage = damage * (1 - damageReduction);
        return finalDamage;
    }
    bool tryCritical(in DamageInfo damageInfo,bool isSkill = false)
    {
        float extraCriticalChance = 0;
        if (isSkill)
        {
            extraCriticalChance += damageInfo.skillHitInfo._skillCirticalChance;
        }

        float rnd = ( damageInfo.criticalChance + extraCriticalChance)/100f;
        if(UnityEngine.Random.Range(0F,1F) < rnd)
        {
            return true;
        }
        return false;
    }
    float  TryGuardReduction(float damage, in DamageInfo damageInfo) {
        
        if (isGuarding==false) return damage;
        if (IsWithinGuardRange(damageInfo)) 
            damage *= (1 - guardReduction);
        return damage; 
    }
    bool IsWithinGuardRange(in DamageInfo damageInfo)
    {
        

        return Vector3.Angle(damageInfo.hitNormal,transform.forward)<guardAngle;
    }

    protected virtual void Die()
    {
        // gameObject.SetActive(false);
        GetComponent<StatusScript>().IsDead =true;

        if(null != _objDropTable)
        {
            _objDropTable.lootTable.SpawnDrop(transform,1,1f,true);
            
        }
        //Formonster
        BehaviorGraphAgent btGraphAgent= GetComponent<BehaviorGraphAgent>();
        if (null !=  btGraphAgent)
        {
            btGraphAgent.BlackboardReference.GetVariableValue<eEnemyState>(BlackboardKeys.ENEMY_STATE, out var val);

            btGraphAgent.SetVariableValue(BlackboardKeys.ENEMY_STATE, eEnemyState.DEAD);

            btGraphAgent.BlackboardReference.GetVariableValue<eEnemyState>(BlackboardKeys.ENEMY_STATE, out  val);
            var a=val;
        }
        else
        {
            GetComponent<Animator>().SetTrigger(Animator.StringToHash("OnDead"));
            ShaderEffects shaderEffects = GetComponent<ShaderEffects>();
            if (shaderEffects != null)
            {
                shaderEffects.SetNowMarerial(eShaderEffect.DISOLVE);
                shaderEffects.DoFade(1.3f, -0.3f, 8f, eFadeMode.FADE_OUT);
            }
        }
        
    }
   
}
