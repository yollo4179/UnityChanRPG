using UnityEngine;
using UnityEngine.UIElements;

public class CameraArmInterp : MonoBehaviour
{
    private float m_fCameraHitSpeed = 10f;
    private float m_Distance = 3f;
    private float m_CurDistance;
    private float Sensitivity = 0.3f;
    private LayerMask collisionMask;  //마스크 연산?31비트? 64비트 다쓰나
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.Translate(0f, 0f, -m_Distance);
        m_CurDistance = m_Distance;
        collisionMask= LayerMask.GetMask("Terrain");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 wheelInput2 = Input.mouseScrollDelta;
        
        m_Distance+= -wheelInput2.y*(Sensitivity *transform.forward).magnitude;


        /* 카메라 충돌 시에 동작*/
        float TargetLength = m_Distance; //부딪히지 않은 경우 대비

        RaycastHit hit; 
        Transform RaySource = GetComponentInParent<Transform>().parent;//Parent는 자기 자신부터 찾는다. 
        
        if (null!= RaySource&& Physics.SphereCast(RaySource.position,0.25f,-RaySource.forward,out hit, m_Distance, collisionMask))
        {
            TargetLength = hit.distance-0.1f;  
        }
        /*부딪혔든 아니든*/
        Debug.DrawRay(RaySource.position,  -RaySource.forward.normalized * m_Distance, Color.green, 0f, false);

        m_CurDistance = Mathf.Lerp(m_CurDistance , TargetLength, m_fCameraHitSpeed* Time.deltaTime);


        transform.localPosition = new Vector3(0f,0f,-m_CurDistance);

}
}
