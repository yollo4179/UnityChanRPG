using UnityEngine;

public class CameraPiivotYaw : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float m_MouseSensitivity = 3f;
    [SerializeField, Min(0.01f)]
    private float m_RotationSmoothTime = 0.08f;

    private float m_TargetYaw;
    private float m_YawVelocity;

    private void OnEnable()
    {
        m_TargetYaw = transform.eulerAngles.y;
        m_YawVelocity = 0f;
    }

    private void Update()
    {
        if (Managers.UI.BlocksCameraInput)
        {
            m_TargetYaw = transform.eulerAngles.y;
            m_YawVelocity = 0f;
            return;
        }

        // Mouse axes already report frame displacement; do not scale by deltaTime.
        m_TargetYaw = Mathf.Repeat(m_TargetYaw +
            Input.GetAxis("Mouse X") * m_MouseSensitivity, 360f);
        float yaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetYaw,
            ref m_YawVelocity, m_RotationSmoothTime);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
