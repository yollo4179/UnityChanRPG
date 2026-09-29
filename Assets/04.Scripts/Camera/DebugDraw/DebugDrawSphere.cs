using UnityEngine;

public class DebugDrawSphere : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color =Color.brown;
        Gizmos.DrawSphere(transform.position,0.1f);
    }
}
