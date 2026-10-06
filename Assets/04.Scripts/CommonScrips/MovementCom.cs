using UnityEngine;

public class MovementCom : MonoBehaviour
{
   
    protected Animator m_Animator;
    protected Vector3 m_vNextMovement = Vector3.zero;

    protected Vector3 m_MovementVector = Vector3.zero;

   
    private CharacterController m_CharacterController;
    protected CharacterController CharacterController{ get => m_CharacterController; } 
    protected float m_YSpeed = -0.5f;
    [SerializeField]
    private Vector3 m_GroundCheckOffset;
    protected Vector3 GroundCheckOffset { get => m_GroundCheckOffset; }

    [SerializeField]
    private float m_GroundCheckRadius = 0.2f;
    protected float GroundCheckRadius { get => m_GroundCheckRadius; }

    protected LayerMask m_GravityCheckLayer;

    [SerializeField]
    private Mesh m_DebugSpehere;
    protected Mesh DebugSpehere{ get =>m_DebugSpehere;}

    protected float m_MovementSpeed = 5.0f;

    //private float m_RotSpeed = 3f;
    protected bool m_isGround = false;

    protected bool m_isJumping = false;
    protected float m_fJumpForce = 6f;
    public void UpdateYSpeedToJumpForce() => m_YSpeed = m_fJumpForce;
    protected bool m_isSprinting = false;


    //private bool m_MoveHorize = false;
    /*region 게터세터*/

    public float MovementSpeed
    {
        get => m_MovementSpeed;
        set => m_MovementSpeed  =value;
    }

    public bool isJumping
    {
        set => m_isJumping = value;
        get => m_isJumping;
    }

    public bool isGround
    {
        set => m_isGround = value;
        get => m_isGround;
    }
    public bool isSprinting
    {
        set => m_isSprinting = value;
        get => m_isSprinting;
    }

    protected virtual void Awake()
    {
        if (m_CharacterController == null)
            m_CharacterController = GetComponent<CharacterController>();
        if (m_Animator == null)
            m_Animator = GetComponent<Animator>();

        

    }
    
    public void ApplyGravity()
    {
        if (!m_isGround||m_YSpeed>0)
        {
            m_YSpeed +=Physics.gravity.y*Time.deltaTime;

        }
        m_MovementVector.y =m_YSpeed;
    }

    public virtual void isOnGround()
    {

        if (Physics.CheckSphere(transform.TransformPoint(GroundCheckOffset), GroundCheckRadius, m_GravityCheckLayer, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            if (m_isJumping && (!m_isGround||m_YSpeed<=0))
            {
                m_YSpeed=-0.5f;
                m_isJumping =false;
                
            }

            m_isGround = true;
        }
        else
        {
            m_isGround= false;

        }
    }
    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(GroundCheckOffset), GroundCheckRadius);

    }



}
