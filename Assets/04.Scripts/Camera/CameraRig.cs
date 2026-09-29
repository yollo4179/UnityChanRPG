using UnityEngine;

public class CameraRig : MonoBehaviour
{
    [SerializeField]
    private Transform m_TargetTransform;
    
    private float m_DampTime = 0.12f;
    [SerializeField]
    private float m_PlayerSpeed;
    private Vector3 m_Velocity =Vector3.zero;

   // private float m_DampRotSpeed = 0.1f;
    private Vector3 m_RotVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*Rig 카메라의 받침대는 항상  보간하며 플레이어를 따라온다.*/
        /*transform.position  = m_TargetTransform.position;*/
        transform.position  = Vector3.SmoothDamp(transform.position, m_TargetTransform.position,ref m_Velocity, m_DampTime);
        //transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetTransform.rotation, m_DampRotSpeed);
    }
}
