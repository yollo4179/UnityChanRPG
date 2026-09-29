using UnityEngine;


public static class CameraUtil 
{

   
    public static Quaternion GetCameraRotation(Vector3 BillBoardPos)
    {
        Camera mainCamera = Camera.main; 
        Vector3 toCam = mainCamera.transform.position - BillBoardPos;
        if (toCam.sqrMagnitude < 0.01) toCam = mainCamera.transform.forward;
        return Quaternion.LookRotation(toCam.normalized, mainCamera.transform.up) ;
    }

}

