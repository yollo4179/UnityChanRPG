using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class PlayerMovementCom : MovementCom
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //[SerializeField]
    //private CharacterController m_CharacterCom;
    
   


    [SerializeField]
    CameraController m_CameraController;
    [SerializeField, Min(1f)]
    private float m_SprintSpeedMultiplier = 1.6f;
    private float m_DefaultStepOffset;
    private int m_EnemyLayerMask;
    private bool m_HitEnemyDuringMove;
    Quaternion m_TargetRotation;
    private float m_ModelRotationSpeed  = 300f; 

   
    
   


    /* 이벤트 함수*/
    protected override void Awake()
    {

        // m_CharacterCom =GetComponent<CharacterController>();

        m_GravityCheckLayer = LayerMask.GetMask("Terrain");
        
        base.Awake();
        m_DefaultStepOffset = CharacterController.stepOffset;
        m_EnemyLayerMask = LayerMask.GetMask("Enemies");
    }

  
    // Called after this frame's input/state update.
    public void TickMovement()
    {
        /*WithJump Logic*/
        isOnGround();
        if (!m_isGround||m_YSpeed>0)
        {
            m_YSpeed +=Physics.gravity.y*Time.deltaTime;

        }
        m_MovementVector.y =m_YSpeed*Time.deltaTime;
        if (Managers.UI.BlocksGameplayInput)
        {
            m_MovementVector.x = 0f;
            m_MovementVector.z = 0f;
        }
        



        /*Jump*/

        MoveWithCollision(m_MovementVector);
    }
    // Use the same collision path for input and animation root motion.
    public void MoveWithCollision(Vector3 displacement)
    {
        if (CharacterController == null || !CharacterController.enabled) return;
        Vector3 scale = transform.lossyScale;
        float radius = CharacterController.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        float halfHeight = Mathf.Max(radius, CharacterController.height * Mathf.Abs(scale.y) * 0.5f);
        Vector3 center = transform.TransformPoint(CharacterController.center);
        Vector3 top = center + Vector3.up * (halfHeight - radius);
        Vector3 bottom = center - Vector3.up * (halfHeight - radius);
        float margin = Mathf.Max(0.02f, CharacterController.skinWidth);
        bool nearEnemy = Physics.CheckCapsule(top, bottom, radius + margin,
            m_EnemyLayerMask, QueryTriggerInteraction.Ignore);
        Vector3 horizontal = new Vector3(displacement.x, 0f, displacement.z);
        float distance = horizontal.magnitude;
        if (distance > Mathf.Epsilon && Physics.CapsuleCast(top, bottom, radius,
                horizontal / distance, out RaycastHit hit, distance + margin,
                m_EnemyLayerMask, QueryTriggerInteraction.Ignore))
        {
            nearEnemy = true;
            horizontal *= Mathf.Clamp01((hit.distance - margin) / distance);
            displacement.x = horizontal.x;
            displacement.z = horizontal.z;
        }
        // A sweep cannot detect a collider we already overlap. Block input further
        // into that monster while still allowing the player to walk away.
        if (nearEnemy && horizontal.sqrMagnitude > 0f)
        {
            foreach (Collider obstacle in Physics.OverlapCapsule(top, bottom, radius + margin,
                         m_EnemyLayerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 away = center - obstacle.ClosestPoint(center);
                away.y = 0f;
                if (away.sqrMagnitude < 0.0001f)
                {
                    away = center - obstacle.bounds.center;
                    away.y = 0f;
                }
                if (away.sqrMagnitude < 0.0001f) horizontal = Vector3.zero;
                else
                {
                    away.Normalize();
                    float inward = Vector3.Dot(horizontal, away);
                    if (inward < 0f) horizontal -= away * inward;
                }
            }
            displacement.x = horizontal.x;
            displacement.z = horizontal.z;
        }
        // Keep stairs usable, but never auto-step onto a monster collider.
        CharacterController.stepOffset = nearEnemy ? 0f : m_DefaultStepOffset;
        float startY = transform.position.y;
        m_HitEnemyDuringMove = false;
        CollisionFlags flags = CharacterController.Move(displacement);
        float allowedY = startY + Mathf.Max(0f, displacement.y);
        if ((nearEnemy || m_HitEnemyDuringMove) && transform.position.y > allowedY + 0.001f)
        {
            // Remove only the upward depenetration caused by a monster.
            // Excluding enemies for this correction prevents another upward push.
            LayerMask originalExclusions = CharacterController.excludeLayers;
            try
            {
                CharacterController.excludeLayers = originalExclusions.value | m_EnemyLayerMask;
                CharacterController.Move(Vector3.down * (transform.position.y - allowedY));
            }
            finally
            {
                CharacterController.excludeLayers = originalExclusions;
            }
        }
        if ((flags & CollisionFlags.Above) != 0 && m_YSpeed > 0f) m_YSpeed = 0f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((m_EnemyLayerMask & (1 << hit.gameObject.layer)) != 0)
            m_HitEnemyDuringMove = true;
    }

    void FixedUpdate()
    {
/*Rigid*/

    }
    public void resetMovementVector()
    {
        m_MovementVector = Vector3.zero;
    }
    /* xz 세팅 */
    public void MoveTo(Vector3 vDirection)
    {
        if (Managers.UI.BlocksGameplayInput)
        {
            resetMovementVector();
            return;
        }
        float MoveAmount = Mathf.Abs(vDirection.x)+Mathf.Abs(vDirection.z); 
      
        m_MovementVector=Vector3.zero;

        /*정규화로 동일한 이동*/
        m_MovementVector =  (new Vector3(vDirection.x, 0, vDirection.z)).normalized;
        // Apply sprint once to horizontal input, leaving gravity unchanged.
        float speed = MovementSpeed * (m_isSprinting ? m_SprintSpeedMultiplier : 1f);
        m_MovementVector *= Time.deltaTime * speed;

        /*입력받은 단위벡터는 카메라에게 회전*/

        Quaternion CamRot = m_CameraController.GetCameraYawRotaion();
        m_MovementVector = CamRot*m_MovementVector;   //카메라의 look , 카메라의 Right   
        //이동벡터는 ㅇㅋ 

        //캐릭터는 입력된 목표를 향해 Damp좀 줘서 천천히 회전시키자.
        if (MoveAmount > 0) {
             m_TargetRotation = Quaternion.LookRotation(m_MovementVector); //카메라의  look을 목표로 캐릭터 천천히 회전 
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, m_TargetRotation, Time.deltaTime*m_ModelRotationSpeed);

    }
    public void RotateToCamera() //In State Attack
    {
        if (Managers.UI.BlocksGameplayInput) return;
        Quaternion CamRot = m_CameraController.GetCameraYawRotaion();
       

        transform.rotation = Quaternion.RotateTowards(transform.rotation, CamRot, Time.deltaTime*m_ModelRotationSpeed);
        m_TargetRotation =transform.rotation; //위에게  move상태에서 계속 돌아감 (회전 목표값 현재로 갱신)
    }
   
    public override void  isOnGround()
    {
       
        if (Physics.CheckSphere(transform.TransformPoint(GroundCheckOffset), GroundCheckRadius,m_GravityCheckLayer, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            if (m_isJumping && (!m_isGround||m_YSpeed<=0))
            {
                m_YSpeed=-0.5f;
                m_isJumping =false;
                m_Animator.SetTrigger("OnGround");
            }
            m_isGround = true;
        }
        else
        {
            m_isGround= false;
        }
    }
    public override void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(GroundCheckOffset), GroundCheckRadius);

    }

}
   