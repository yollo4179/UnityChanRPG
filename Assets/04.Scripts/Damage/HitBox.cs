using System.Collections;
using UnityEngine;

public enum eDamageSourceType
{
    PLAYER,
    MONSTER,
    OBJECT,
}
public enum eHitAreaMarker
{   NONE,
    HEAD,
    RIGHT_WEAPON,
    LEFT_WEAPON,
    LEFT_LEG,
    RIGHT_LEG,
    //BODY,
    //LEFT_THIG,
    //RIGHT_THIG,
    //LEFT_ARM, 
    //RIGHT_ARM,
}
public class SkillHitInfo
{
    public float _skillDamage;
    public float _skillCirticalChance;
    public float _skillCriticalDamage;
    public int  _numHits;
    public int _skillLevel; 
}
public class HitBox : MonoBehaviour
{
    [Header("IF MonsterInfo")]
    [SerializeField] eDamageSourceType damageSourceType;
    [SerializeField] int monsterID=-1;
    SkillHitInfo _skillInfo;
    DamageInfo _damageInfo ;
    [SerializeField]LayerMask _layerMask;
    StatusScript _statusScript;
    GameObject _owner;

    Transform _targetTransform;
    Coroutine _co;
    bool _turnOffCondition = false; 
   [SerializeField] eHitAreaMarker _areaMarker;  public eHitAreaMarker AreaMarker  {get => _areaMarker; }

    int _attackID=0;
    EntityId _attackerID = EntityId.None;

    public int  IncreaseAttackID(int maxAttackID)
    {

       return  _attackID =(++_attackID) %maxAttackID;
    }
    public HitBox SetOwner(GameObject owner)
    { 
        _owner= owner;
        _damageInfo.Source = owner;
        _attackerID = GetEntityId();
        _attackID = 0;

        return this;
    }
    /*Script에서 부여?*/
    public HitBox InjectScript(StatusScript statusScript)
    {
        _statusScript = statusScript;
        _damageInfo.hitLayerMask = _layerMask;
        _damageInfo.criticalChance = _statusScript.CriChance;
        _damageInfo.criticalDamage = _statusScript.CriDamage;
        _damageInfo.baseDamage = _statusScript.AttackDamage;
        _damageInfo.weaponDamage = 0;
        _damageInfo.hitLayerMask = _layerMask;
        return this; 
    }
    public HitBox InjectSkillInfo(SkillHitInfo skillInfo) {
        _skillInfo = skillInfo;
        _damageInfo.skillHitInfo = skillInfo;
        return this; 
    }

    public void ResetSkillInfo()
    {
        _skillInfo=null;
        _damageInfo.skillHitInfo=null;
        return;
    }
    public void UpdateTransform(Transform transform)
    {
        _targetTransform =transform;
    }
    public void ResetTransform()
    {
        _targetTransform = null;
    }
    public void TunrOnHitBoxInPlace(float hitBoxTime)
    {
        StopCoroutine(playHitBox(hitBoxTime));
        if(null==_co)
            _co= StartCoroutine(playHitBox(hitBoxTime));


    }
    IEnumerator playHitBox(float hitBoxTime) //원거리용 
    {
        GetComponent<Collider>().enabled = true;

        yield return new WaitForSeconds(hitBoxTime); // 이넘으로 커스텀 조건 관리 To do

        GetComponent<Collider>().enabled = false;

        _co =null;
        ResetSkillInfo();
        yield break;
    }
    private void Start()
    {
        _targetTransform=null;

    }
  
    private void Update()
    {
        if (null != _targetTransform)
        {
            transform.position =  _targetTransform.position;  
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (false ==enabled) 
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if(damageable != null)
        {
           _damageInfo.hitNormal = -transform.forward;
            // damageInfo.hitNormal = -transform.forward;
            _damageInfo.attackID =_attackID;
            _damageInfo.attackerID = _attackerID;
            _damageInfo.hitLayerMask = other.gameObject.layer;
            _damageInfo.hitPoint= ((other.bounds.center)); 

            if(other.gameObject.layer !=_owner.layer)
                damageable.TakeDamage(_damageInfo);
        }
   }
    public void OnDrawGizmos()
    {

        BoxCollider col = GetComponent<BoxCollider>();//캐스팅으로 SWITCH? 일단 박스만
       Bounds bound= col.bounds;
        //bound.center = col.center;
        //bound.size = col.size;

        Gizmos.color = Color.red;
        if (true ==enabled) {
            Gizmos.DrawWireCube(
                bound.center,
                bound.size);
    } }
}

