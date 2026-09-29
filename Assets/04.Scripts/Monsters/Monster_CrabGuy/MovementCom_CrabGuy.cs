using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MovementCom_CrabGuy : MovementCom_NavMesh
{
    [SerializeField]
    Transform EyeTransform;
    //[SerializeField]
    //Transform TargetTransform;

    EQSQuerier m_Agent;

    private  AnimatorCom_CrabGuy m_AnimatorCom; 

    LayerMask m_Mask;

    float MyEyeSight = 60f;

    ///float ArrivalRadius = 0.35f;
    STATE NowState = STATE.IDLE;
    /*일단 이거 이렇게 쓰고 컨트롤러에서 돌리자.*/
    enum STATE
    {
        IDLE,
        CHASE,
        HOLD,
        MOVE_TO_SLOT,
        ATTACK,
        HIT,
        DIE,
        END
    }


    protected override void Awake()
    {
        m_Mask = LayerMask.GetMask("Player", "Obstacles");
        m_Agent= GetComponent<EQSQuerier>();
        m_AnimatorCom=GetComponent<AnimatorCom_CrabGuy>();
        base.Awake();
    }
    public void Update()
    {
        //Vector3 Dir = (m_TargetTransform.position- transform.position).normalized;
        //if ((m_TargetTransform.position - transform.position).magnitude >0.5f)
        //    agent.SetDestination(m_TargetTransform.position);

        /*상태를 체크한다. */
        /*체크 할 때마다 행동을 정의하고 움직임을 구현한다.*/




        //isOnGround();
        //ApplyGravity();

        //CharacterController.Move(m_MovementVector * Time.deltaTime);


        switch (NowState)
        {
        case STATE.IDLE:
        {
            if( Detect())
                NowState = STATE.CHASE;
            m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
            break;
        }
        case STATE.ATTACK:
        {
             Attack();
             break;
        }
        case STATE.CHASE:
        {
                    Chase();
                    m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
                    break;
        }
        case STATE.HOLD:
        {
                    Hold();
                    m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
                    break;
        }
        case STATE.HIT:
        {
             break;
        }
        case STATE.MOVE_TO_SLOT:
        {

                    MoveToRSlotAndOccupy();
                    m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
                    break;
        }

        }



    }
    /*In Idle Or Patroll*/
    public bool Detect()
    {
        /*1.타겟과 나의 내lOOK의 각도를 비교한다.*/
        Vector3 vEyeForward = EyeTransform.up;
        Vector3 TargetDir = m_TargetTransform.position  -transform.position; 
        float Deg_EyeAndTarget = Mathf.Rad2Deg*Mathf.Acos(Vector3.Dot(vEyeForward.normalized, TargetDir.normalized));

        bool DidEyeSightCatch = Deg_EyeAndTarget<MyEyeSight;
        RaycastHit hit;
        if (DidEyeSightCatch)
        {
           
            Physics.Raycast(transform.position + Vector3.up*0.5f, TargetDir.normalized*8f, out hit, 5f, m_Mask);
            if (hit.collider.gameObject&&1<<hit.collider.gameObject.layer == LayerMask.GetMask("Player"))
            {
                return true;
            }
        }
   

        return false; 


    }
    /*Chase*/
    public void Chase()
    {


        Vector3 Dir = (m_TargetTransform.position- transform.position).normalized;
        //if ((m_TargetTransform.position - transform.position).magnitude <2f)
        NavAgent.SetDestination(m_TargetTransform.position); /*->HasPath->True를 위해*/
        NowState=STATE.HOLD;
        m_Agent.TryReserveSlot();
        m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
    }
    /* MoveToSlot*/
    public void MoveToRSlotAndOccupy()
    {
        //if (null == m_Agent.ReservedSlot)
        //{
        //    NowState =STATE.CHASE;
        //}

        //m_Agent.NavAgent.SetDestination(m_Agent.ReservedSlot.pos);

        //float Square = ArrivalRadius*ArrivalRadius;
        //if ( Vector3.SqrMagnitude(m_Agent.ReservedSlot.pos- transform.position)< Square)

        //{
        //    m_Agent.CurrentSlot = m_Agent.ReservedSlot;
        //    m_Agent.CurrentSlot.occupant =m_Agent;
        //    m_Agent.CurrentSlot.reserver= m_Agent;

        //    NowState = STATE.HOLD;
        //}
        //else
        //{
        //    // 주기적으로 더 나은 슬롯이 있나 재평가
        //    m_Agent.TryReserveSlot();
        //}
        //m_AnimatorCom.Movement(Mathf.Clamp01(m_Agent.NavAgent.velocity.magnitude));
    }

    void Hold()
    {
        /* 하는 일 : 공격/TryAttack  위치 확인 후 슬롯해제    */

        
        //if (Vector3.SqrMagnitude(m_Agent.ReservedSlot.pos- transform.position)> ArrivalRadius) //슬롯 러용 범위
        //{
        //    NowState = STATE.CHASE;
        //    return; 
        //}
        ///*Hold 자리에 있다면 공격요청*/
        //CoAttackManager.GetInstance().RequestAttack(m_Agent);
        //if(CoAttackManager.GetInstance().IsAttacking(m_Agent))
        //{
        //    /*공격 요청이 성공한 경우에만 공격 단계로 넘어간다.*/
        //   // NowState = STATE.ATTACK; 
        //}
        
    }
    void Attack()
    {

        m_AnimatorCom.Trigger_Attack();  

        /*공격도 매니저가 관리한다. Attack매니저가 허가한 시간 동안에만 애니메이션이 동작 할 수 있다 . */

        /*공격을 수행한다. 공격의 종료 조건은 ...*/
        /*공격을 수행한 후에 어택 메니저에게 종료 사실을 알리고 제외 처리한다.*/


        /*공격 후에 Hold 상태로 돌아오기*/
    }
    /*MoveToSlot*/
    private void Hit()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector3 vEyeForward = EyeTransform.up;
        vEyeForward.y=0;
        Gizmos.color =Color.green;
        Vector3 Src = transform.position + Vector3.up*0.5f;
        Gizmos.DrawLine(Src,Src+ 8*vEyeForward);
    }
}
