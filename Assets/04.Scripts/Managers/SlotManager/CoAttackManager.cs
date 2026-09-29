using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class CoAttackManager : MonoBehaviour
{
    #region 싱글톤
    private static CoAttackManager m_Instance;
   private static CoAttackManager Instance
    {
        get
        {
            if(null ==m_Instance)
            {
                m_Instance = FindFirstObjectByType<CoAttackManager>();
            }
            if (null ==m_Instance) {
                var GO =new GameObject(nameof(CoAttackManager));
                m_Instance = GO.AddComponent<CoAttackManager>();
            }
            return m_Instance;
        }
    }
    public static CoAttackManager GetInstance() { return Instance; }
    #endregion

    [Header("최대 동시 공격 인원")]
    public int maxNumCoAttackers = 5;

    [Header("쿨다운 관리")]
    public float CoolDownDuration = 1f; 

    readonly HashSet<EQSQuerier> CoAttackers = new HashSet<EQSQuerier>();

    readonly Dictionary<EQSQuerier, float> AttackerAndCoolDownPairs = new Dictionary<EQSQuerier, float>();
    readonly Queue <EQSQuerier> WatingQueue = new Queue <EQSQuerier>();


    private void Awake()
    {
        GetInstance(); 
    }

    public void RequestAttack (EQSQuerier Agent)
    {
        if(null==Agent)
        {
            return;
        }
        //*쿨다운 관리 (스케줄링)*/
        if(!AttackerAndCoolDownPairs.ContainsKey(Agent))
        {
            AttackerAndCoolDownPairs.Add(Agent, Time.time);
        }
        if (AttackerAndCoolDownPairs.TryGetValue(Agent, out float CoolDown))
        {
            if (CoolDown + CoolDownDuration > Time.time) return;
            CoolDown =Time.time;

        }
        //Attack
        if (IsAttacking(Agent))
            return;
        if (CoAttackers.Count< maxNumCoAttackers)
        {
            /*동시에 공격할 수 인원이 있다면*/
            StartCoroutine(CoAttack(Agent)); 
        }
        else
        {

            if (!WatingQueue.Contains(Agent))
                WatingQueue.Enqueue(Agent);
        }
    }
    public bool IsAttacking(EQSQuerier Agent)
    {
        return CoAttackers.Contains(Agent);
    }

    public void PromoteToCoAttackers()
    {
        while(CoAttackers.Count< maxNumCoAttackers && 0 < WatingQueue.Count )//허용할 수 있는 수준까지 %% 대기자가 남아있을떄까지
        {
            EQSQuerier NextAgent = WatingQueue.Dequeue();
            /*살아있을때만 */
            StartCoroutine(CoAttack(NextAgent));
        }
    } 
    IEnumerator CoAttack(EQSQuerier Agent)
    {
        /*1.코루틴이 아니라 이벤트 매니저로 몬스터 공격의 종료이벤트를 받아도 ㅇㅋ <Agent의 주소, 공격의 타입?>
         * 2.,근데 이벤트 Publish 몬스터가 공격할때마다 하면 객체 생성비용이 좀 있나? */
        /*몬스터의 공격이 끝났다는 플래그를 공격 종료시 Set */


        //CoAttackers.Add(Agent); //해시에 CoAttacker  등록하고 Coroutine 등록해서 공격 끝나면 해제
        //while ( false==Agent.AttackEndFlag) /*게팅은 애니메이션 이벤트에서 */
        //{
        //    yield return null; 
        //}
        //Agent.SetAttackFlagTo(false);
        ///* 나의 공격이 끝났으면 동시공격, look up 테이블에서 나를 제거한다 . 내차례는 끝낫으니...*/
        //CoAttackers.Remove(Agent);

        yield return null;



    }


}
