using UnityEngine;
using UnityEngine.UIElements;

public class CameraPivotPitch : MonoBehaviour
{
    [SerializeField]
    private float m_PitchX = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.RotateAround(transform.position, Vector3.right, m_PitchX);
    }


}
