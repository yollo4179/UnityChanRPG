using UnityEngine;
public class CoroutineRunner : MonoBehaviour
{
    static CoroutineRunner _inst;
    public static CoroutineRunner Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject(nameof(CoroutineRunner));
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<CoroutineRunner>();
            }
            return _inst;
        }
    }
    
}
