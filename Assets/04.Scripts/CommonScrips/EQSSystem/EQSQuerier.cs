using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EQSQuerier : MonoBehaviour
{

    EQSTarget _target;
    public EQSTarget Target { get { return _target; } }

    private EQSQuerierBehaviour_Battle m_eqsQuerierBehabiour;
    public EQSQuerierBehaviour_Battle EQSQuerierBehabiour { get => m_eqsQuerierBehabiour; }

    public EQSPoint OccupingPoint { get; set; }


    private NavMeshAgent m_NavMeshAgent;

    public NavMeshAgent NavAgent { get => m_NavMeshAgent; }

    bool _isChasing = false;
    public bool IsChasing{get=>_isChasing; set=> _isChasing =value;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_eqsQuerierBehabiour = GetComponent<EQSQuerierBehaviour_Battle>();
        m_NavMeshAgent =GetComponent<NavMeshAgent>();
        EQSManager.GetInstance().GetAllQueriers().Add(this);
        _target =  EQSManager.GetInstance().GetTarget();
    }
 
    // Update is called once per frame
    void Update()
    {
        
    }
    public void TryReserveSlot()
    {
        
        //if (Time.time - LastEvalTime < ReEvalInterval  && !StartReeval) return;
        //LastEvalTime = Time.time; StartReeval = false;


        //var best = EQSManager.GetInstance().GetBestSlot(this);
        //if (null == best) return;


        //// 이미 예약되어있고 그게 Best가 아니다? 기존 예약 해제
        //if (ReservedSlot != null && ReservedSlot != best && ReservedSlot.Reserved == this)
        //    ReservedSlot.Reserved = null;


        //// 새 예약
        //ReservedSlot = best;
        //if (best.Reserved != this)
        //    best.Reserved= this;
    }
    public void ReleaseSlot()
    {
        //if (null !=CurrentSlot  && this== CurrentSlot.Occupant )
        //    CurrentSlot.Occupant = null;
        //if (null != ReservedSlot  && this ==ReservedSlot.Reserved)
        //    ReservedSlot.Reserved = null;
        //CurrentSlot = null; ReservedSlot = null;
    }


    void DebugDraw()
    {
        //if (ReservedSlot != null)
        //{
        //    Debug.DrawLine(transform.position + Vector3.up * 0.1f, ReservedSlot.SlotPos + Vector3.up * 0.1f, Color.cyan);
        //}
        //if (CurrentSlot != null)
        //{
        //    Debug.DrawLine(transform.position + Vector3.up * 0.12f, CurrentSlot.SlotPos+ Vector3.up * 0.12f, Color.red);
        //}
    }
}
