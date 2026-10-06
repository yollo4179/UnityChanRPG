
using DigitalRuby.LightningBolt;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_ANDROID
using UnityEngine.InputSystem.Android;
#endif
using UnityEngine.Rendering;
using static UnityEngine.UI.GridLayoutGroup;

public class ChainLightningProj : PlayerProjectile
{
    [SerializeField][ Range(1,10)] int maximunEnemiesInChain = 8;
    [SerializeField] float RefreshRate  = 0.02F;
    [SerializeField] float delayBtwEachChain = 0.2f;  
     Transform  playerFirePoint;
     EnemyDetector  playerEnemyDetector ; 
    [SerializeField] string lineRendererPrefabPoolName;
    [SerializeField] string impacPreFabPoolName; 

    int level = 1; 
    List<GameObject> spawnedLineRenderers=new List<GameObject>();
    readonly List<Poolable> _impactEffects = new List<Poolable>();
    List<EntityId> EnemiesInChain = new List<EntityId>();
    GameObject currentClosestEnemy; 
    bool shooting;
    bool shot;
    bool _lock = false;
    Coroutine _co = null;
    int _chainGen = 0 ;
    DamageInfo _damageInfo;
    SkillHitInfo _skillInfo = new SkillHitInfo();
    LayerMask _layerMask;

    bool _initOnce = false;

    private void Awake()
    {
        playerFirePoint = GetComponentsInChildren<Transform>(true)
             .FirstOrDefault(x => x.name == "Weapon_Tip_ND");//NonDirectional

        playerEnemyDetector =GetComponent<EnemyDetector>();
        _layerMask =LayerMask.GetMask("PlayerAttack");
    }
    public override void Init(PlayerControllerCom playerController)
    {
       // if (!_initOnce)
       {
            _layerMask =LayerMask.GetMask("PlayerAttack");
            base.Init(playerController);
            /*컨트롤러에서 캐싱해두는 방법도 있음*/
            playerFirePoint = _playerController.GetComponentsInChildren<Transform>(true)
                  .FirstOrDefault(x => x.name == "Weapon_Tip_ND");//NonDirectional

            playerEnemyDetector =_playerController.GetComponent<EnemyDetector>();

            SetOwner(_playerController.gameObject);
            InjectScript(playerController.GetComponent<StatusScript>());
            PlayerSkillInfo playerSkillInfo = Managers.Player.GetSkillInfoByHandle(_skillSO.handle);


            _skillHitInfo._skillDamage=playerSkillInfo.BaseDamage* playerSkillInfo.Level;
            _skillHitInfo._skillCirticalChance =playerSkillInfo.ExtraCriticalChance* playerSkillInfo.Level;
            _skillHitInfo._skillCriticalDamage=playerSkillInfo.ExtraCriticalDamage *playerSkillInfo.Level;
            _skillHitInfo._numHits= playerSkillInfo.NumberOfHit;
            _skillHitInfo._skillLevel =playerSkillInfo.Level;
            
            InjectSkillInfo(_skillHitInfo);

            EnemiesInChain = new List<EntityId>();
            spawnedLineRenderers=new List<GameObject>();
            _initOnce = true;
        }
        Shot();
    }

    public void Shot()
    {
        Debug.Assert(null != playerEnemyDetector);
      
        var list = playerEnemyDetector.GetEnemiesInRange(); 

        if (null!=list && 0< list.Count)
        {
            /*주위에 적이 있다. 처음 하는 공격이다.*/
            if (!shooting)
            {
                StartShooting();//첫 대상에 공격 시작
            }
        }
        else
        {
            /*주위에 적이 없다.*/
            StopShooting();
        }

    }
    
    void Update()
    {
        /*ForTest*/
        /*버튼을 누르면 스킬을  활성화한다.*/
        //if (Input.GetKeyDown(KeyCode.Q))
        //{

        //    Shot();
        //}
        //if(Input.GetKeyUp(KeyCode.Q))
        //{
        //    StopShooting();
        //}


    }
    void StartShooting()
    {
        /*처음 공격 알고리즘*/
        if (null != _co) return;
        shooting =true; 
        if(null!=playerEnemyDetector &&null!= lineRendererPrefabPoolName &&null!= playerFirePoint)
        {
            if(!shot) //처음 쏜다.(플레이어로부터)
            {
                _chainGen++;
                shot =true;
                currentClosestEnemy = playerEnemyDetector.GetClosestEnemySortedList()[0]; //플레이어와 가장 가까운 적 
                if (currentClosestEnemy==null) return;

                NewLineRenderer(playerFirePoint, currentClosestEnemy.transform,true); //선을 잇는다.
                
                if(1<maximunEnemiesInChain)
                {
                    /*플레이어와 잇고 용량이 더 있으면 맞은 적의 다음 적 조사*/
                    EnemiesInChain.Add(currentClosestEnemy.transform.root.GetEntityId());
                    _co =StartCoroutine(CheckAndChainReaction(currentClosestEnemy,_chainGen));
                }
            
            }
        }
     }
    HashSet<EntityId> _visited = new();  // ID로 관리 권장
    

    IEnumerator CheckAndChainReaction(GameObject closestEnemy,int generationToken)
    {
        if (_chainGen!= generationToken) yield break;

        yield return new WaitForSeconds(delayBtwEachChain);

        //코너 인스턴스 추가 close taransform에 
        int p = -1;
        switch (level)
        {
            case 1:
                {
                    p=1; break;
                }
            case 2:
                {
                    p=2; break;
                }
            case 3:
                {
                    p=3; break;
                }
            case 4:
                {
                     p=4; break;
                }
            case 5:
                {
                    p=5; break;
                }
            case 6:
                {
                    p=6; break;
                }
            case 7:
                {
                    p=7; break;
                }
        }
        if(!debugEdges())
        {
            int k = 0;
        }

        if (generationToken != _chainGen) yield break;

        if (level >= maximunEnemiesInChain)
        {
            StopShooting();
            yield break; //체인 연쇄 종료 조건
        }
        else
        {
            if (shooting) //공격 수행중이다. 
            {
                
                /*부모를 추가하고*/
                EnemyDetector detector = closestEnemy != null ? closestEnemy.GetComponent<EnemyDetector>() : null;
                var sortedList = detector != null ? detector.GetClosestEnemySortedList() : null;
                if (null==sortedList)
                {
                    StopShooting();
                    yield break;
                }
                else if (0>=sortedList.Count)
                {
                    StopShooting();
                    yield break; 
                }
                else
                {
                    GameObject target = null;
                    for (int i = 0; i<sortedList.Count; ++i)
                    {
                        if (sortedList[i] == null || sortedList[i] == closestEnemy) continue;

                        target =sortedList[i];


                        EntityId to = target.transform.root.GetEntityId();
                        if (!EnemiesInChain.Contains(to))
                        {
                            ++level;
                            /*부모와 가까운 적을 조사한다.*/
                            EnemiesInChain.Add(target.transform.root.GetEntityId());
                            NewLineRenderer(closestEnemy.transform, target.transform, false);
                            StartCoroutine(CheckAndChainReaction(target, generationToken));
                            yield break; //여기 걸리지 않으면 더이상 이어질 수있는 몬스터가 없다.
                        }
                    }
                    StopShooting();
                }
            }
        }

    }
    void NewLineRenderer(Transform startPos, Transform  endPos, bool bFromPlayer)
    {
       
        Poolable lineR = Managers.Pool.LendPoolableTo(lineRendererPrefabPoolName, null);
        spawnedLineRenderers.Add(lineR.gameObject);
        _dicSE.Add(startPos.root.GetEntityId(), endPos.root.GetEntityId());

        lineR.gameObject.GetComponentInChildren<LightningBoltScript>().StartObject=startPos.gameObject;
        lineR.gameObject.GetComponentInChildren<LightningBoltScript>().EndObject=endPos.gameObject;

        StartCoroutine(UpdateLineRenderer(lineR.gameObject, startPos,endPos, bFromPlayer));
        /*Effect*/
        Poolable impactVFX = Managers.Pool.LendPoolableTo(impacPreFabPoolName, transform);
        _impactEffects.Add(impactVFX);
        impactVFX.gameObject.transform.position=endPos.position;
        impactVFX.gameObject.transform.rotation =CameraUtil.GetCameraRotation(endPos.position);
        impactVFX.GetComponent<Effect>().ParticleOn();

        /*hitBox*/
        //Poolable poolable = Managers.Pool.LendPoolableTo("HitBoxPlayer_Prefab", null);
        //GameObject hitBoxGO = poolable.gameObject;
        //hitBoxGO.transform.position=endPos.position;
        //HitBox hitBox= hitBoxGO.GetComponent<HitBox>();
        //hitBox.SetOwner(_playerController.gameObject);
        //hitBox.InjectScript(_playerController.gameObject.GetComponent<StatusScript>());
        //hitBox.InjectSkillInfo(_skillHitInfo);   
     

        /*Attack*/
        GiveDamage(_damageInfo, endPos);
        /*트랜스폼 정보 넘겨서따라다니게 만들수도*/
    }
    public void SetOwner(GameObject owner)
    {
        _damageInfo.Source = _playerController.gameObject;
        _damageInfo.attackerID = GetEntityId();
        _damageInfo.attackID = 0;
        _damageInfo.hitNormal = -transform.forward;
        return;
    }
    public void InjectScript(StatusScript statusScript)
    {

        _damageInfo.hitLayerMask = _layerMask;
        _damageInfo.criticalChance = statusScript.CriChance;
        _damageInfo.criticalDamage = statusScript.CriDamage;
        _damageInfo.baseDamage = statusScript.AttackDamage;
        _damageInfo.weaponDamage = 0;
   
        return ;
    }
    public void InjectSkillInfo(SkillHitInfo skillInfo)
    {
        _skillInfo = skillInfo;
        _damageInfo.skillHitInfo = skillInfo;
        return;
    }

    public void GiveDamage( DamageInfo dmgInfo, Transform enemy)
    {

        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            dmgInfo.hitNormal = -transform.forward;
            // damageInfo.hitNormal = -transform.forward;
            dmgInfo.attackID =0;
            dmgInfo.attackerID =GetEntityId();
            dmgInfo.hitLayerMask = enemy.gameObject.layer;
            dmgInfo.hitPoint= enemy.position;

            if (enemy.gameObject.layer !=gameObject.layer)
                damageable.TakeDamage(dmgInfo);
        }
    }
    IEnumerator UpdateLineRenderer(GameObject lineR, Transform startPos, Transform endPos, bool bFromPlayer = false)
    {
        var pool = lineR.GetComponent<Poolable>();
        while (shooting && shot && pool._nowUsing && startPos != null && endPos != null)
        {
            lineR.GetComponent<LineRendererController>().SetPosition(startPos, endPos);
            yield return null; // 다음 프레임
        }
        lineR.GetComponent<LineRendererController>().SetPosition(Vector3.zero, Vector3.zero);
        lineR.gameObject.GetComponentInChildren<LightningBoltScript>().StartObject=null;
        lineR.gameObject.GetComponentInChildren<LightningBoltScript>().EndObject=null; 
    }
    public void StopShooting()
    {
        ClearChain();
        Poolable pool = GetComponent<Poolable>();
        if (pool != null && pool._nowUsing) Managers.Pool.GetBack(pool);
    }

    private void OnDisable()
    {
        ClearChain();
    }

    private void ClearChain()
    {
        // Stop pending links before returning visuals that another cast can reuse.
        ++_chainGen;
        StopAllCoroutines();
        shooting = false;
        shot = false;
        _co = null;
        foreach (GameObject line in spawnedLineRenderers)
        {
            if (line == null) continue;
            line.GetComponent<LineRendererController>().SetPosition(Vector3.zero, Vector3.zero);
            LightningBoltScript bolt = line.GetComponentInChildren<LightningBoltScript>(true);
            if (bolt != null) { bolt.StartObject = null; bolt.EndObject = null; }
            Poolable pool = line.GetComponent<Poolable>();
            if (pool != null && pool._nowUsing) Managers.Pool.GetBack(pool);
        }
        spawnedLineRenderers.Clear();
        foreach (Poolable impact in _impactEffects)
        {
            // An impact may already have finished and returned to its own pool.
            if (impact == null || !impact._nowUsing || impact.transform.parent != transform) continue;
            foreach (ParticleSystem particle in impact.GetComponentsInChildren<ParticleSystem>(true))
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Managers.Pool.GetBack(impact);
        }
        _impactEffects.Clear();
        level = 1;
        EnemiesInChain.Clear();
        _visited.Clear();
        _dicSE.Clear();
    }
    public Dictionary<EntityId, EntityId> _dicSE = new Dictionary<EntityId, EntityId>();

    bool debugEdges()
    {
        bool ret =true; 
        for (int i=0 ; i<EnemiesInChain.Count-1;++i)
        {
            if (_dicSE.TryGetValue(EnemiesInChain[i], out var me))
            {
                if(me !=EnemiesInChain[i+1])
                    return false;
            }
        }
        return ret; 
    }
}
