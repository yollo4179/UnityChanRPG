using Unity.VisualScripting;
using UnityEditor;
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
    Quaternion m_TargetRotation;
    private float m_ModelRotationSpeed  = 300f; 

   
    
   


    /* 이벤트 함수*/
    protected override void Awake()
    {

        // m_CharacterCom =GetComponent<CharacterController>();

        m_GravityCheckLayer = LayerMask.GetMask("Terrain");
        
        base.Awake();
    }

  
    private void Update()
    {
        /*WithJump Logic*/
        isOnGround();
        if (!m_isGround||m_YSpeed>0)
        {
            m_YSpeed +=Physics.gravity.y*Time.deltaTime;

        }
        m_MovementVector.y =m_YSpeed*Time.deltaTime;
        



        /*Jump*/

        UpdateDash();
        CharacterController.Move(m_MovementVector);
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
        float MoveAmount = Mathf.Abs(vDirection.x)+Mathf.Abs(vDirection.z); 
      
        m_MovementVector=Vector3.zero;

        /*정규화로 동일한 이동*/
        m_MovementVector =  (new Vector3(vDirection.x, 0, vDirection.z)).normalized;
        m_MovementVector *= Time.deltaTime*MovementSpeed;

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
    public void UpdateDash ()
    {
        if(m_isSprinting)
        {
            
            m_MovementVector*=m_fSprintSpeed;

        }



    }

}
   