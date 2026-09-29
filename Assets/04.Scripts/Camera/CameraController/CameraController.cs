using UnityEngine;




public class CameraController : MonoBehaviour
{
    public  float CameraYaw ;


    public Quaternion GetCameraYawRotaion()
    {
        return Quaternion.Euler(0f,transform.eulerAngles.y,0f);  //로컬이 아니다 (이것만 고려한) 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Cursor.visible =false;
        //Cursor.lockState = CursorLockMode.Locked;  
    }

    // Update is called once per frame
    void Update()
    {
       


    }
    private void LateUpdate()
    {
        

    }

}
