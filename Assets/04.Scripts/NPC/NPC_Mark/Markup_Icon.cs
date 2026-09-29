using UnityEditor.Rendering;
using UnityEngine;

public class Markup_Icon : MonoBehaviour
{
    public void Update()
    {
        transform.rotation =  CameraUtil.GetCameraRotation(transform.position);
    }
}
