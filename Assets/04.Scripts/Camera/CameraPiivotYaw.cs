using UnityEngine;
using UnityEngine.UIElements;

public class CameraPiivotYaw : MonoBehaviour
{

    [SerializeField]
    private float m_RotSpeed=500;
   

   
    void Update()
    {

          
        float deltaYaw = Input.GetAxis("Mouse X") * m_RotSpeed * Time.deltaTime;
        transform.RotateAround(transform.position, Vector3.up, deltaYaw);//»ç½Ç (0,0,0)

    }
}
