using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class SpringArmCom : MonoBehaviour
{

    /**/
    private Transform m_Transform;
    private Transform m_OriginalTransform;
    
    [SerializeField]
    private Transform TargetTransform ;

   // private float m_InterpTime = 0.5f;
    public LayerMask collisionMask;     // 충돌 감시 레이어
    private Vector3 m_velocity;



    //private float MaxPitch = 70.0f;
    private float m_PitchX = 20f;
    private float m_OriginalPitchX = 0.0f;

    private float m_YawY = 0f;
    private float m_OriginalYawY = 0f;

    private const float m_fDistanceLimit = 5f;
    private float m_fDistance = 3f;
    private float m_fOriginalDistance = 0f;

    private float Sensitivity = 0.5f;
    private float RotSpeed = 500f;

    public Transform Transform
    {
        get => Transform;
    }
    void Awake()
    {

        //m_Transform.rotation = Quaternion.Euler(m_EulerRotationX, m_EulerRotationY, 0f);
        //m_Transform.position = Vector3.zero;
        //m_Transform.localScale= Vector3.one;
        //
        m_Transform =GetComponent<Transform>(); 
        /*Base*/
        m_Transform.Translate(0f, 0f, -m_fDistance, Space.Self); //Local
        m_Transform.RotateAround(TargetTransform.position, Vector3.right, m_PitchX); //PitchUp
        transform.LookAt(TargetTransform.position);


        m_OriginalTransform = m_Transform;
        m_fOriginalDistance = m_fDistance;
        m_OriginalPitchX = m_PitchX;
        m_OriginalYawY  = m_YawY;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        /*Default Logic*/


        //Vector3 DesiredPosition = TargetTransform.position  -transform.position;

        

        //Transform.position =Vector3.SmoothDamp(Transform.position, DesiredPosition, ref m_velocity, m_InterpTime);

        
        if (Input.GetKeyDown(KeyCode.F1))
        {
            m_Transform = m_OriginalTransform;
            m_fDistance= m_fOriginalDistance;
            m_PitchX = m_OriginalPitchX;
            m_YawY = m_OriginalYawY;
            transform.LookAt(TargetTransform.position);
        }

        /*우클릭하면서 마우스를 움직이면  */
        if( Input.GetMouseButton(1) )//우클릭 중
        {
            float deltaPitch = -Input.GetAxis("Mouse Y") * RotSpeed * Time.deltaTime;
            float deltaYaw = Input.GetAxis("Mouse X") * RotSpeed * Time.deltaTime;

            m_Transform.RotateAround(TargetTransform.position, Vector3.up, deltaYaw);
            
            transform.LookAt(TargetTransform.position);
        }

        /*휠을 움직이면  거리가 변한다.*/
        
           

            Vector2 wheelInput2 = Input.mouseScrollDelta;
            if (wheelInput2.y > 0)
            {
            m_Transform.position += wheelInput2.y *Sensitivity *m_Transform.forward;
            // 휠을 밀어 돌렸을 때의 처리 ↑
            }
            else if (wheelInput2.y < 0)
            {
            m_Transform.position +=  wheelInput2.y *Sensitivity *m_Transform.forward; ;
            // 휠을 당겨 올렸을 때의 처리 ↓
            }
        

    }
}
