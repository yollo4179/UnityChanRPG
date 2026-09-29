using UnityEngine;

public class RotateEffect : MonoBehaviour
{
    [SerializeField] float _rotSpeed;
    [SerializeField] bool _isPlay = true; 
    public void Update()
    {
        if (_isPlay) return;
        transform.Rotate(transform.forward, _rotSpeed); 
    }
}
