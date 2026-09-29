using Unity.Behavior;
using UnityEngine;

public class WakeUpTrigger : MonoBehaviour
{
    [SerializeField] bool wakeUpByNPC = false;
    [SerializeField] bool _hasCutScene = false; 
    Animator _animator;
    BehaviorGraphAgent _bhAgent;
    bool _hasTriggerOn=false;

    public void ResetTrigger()
    {
        _hasTriggerOn=false;
    }
    public bool HasCutScene() { return _hasCutScene; }
    public  bool CheckTrigger() { return _hasTriggerOn; }

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        //_bhAgent = GetComponent<BehaviorGraphAgent>();
        _hasTriggerOn =false;
    }

    public void OnTriggerEnter(Collider oth)
    {
        if (true == wakeUpByNPC) return;
        if (true == _hasTriggerOn) return;

        if (true == oth.CompareTag("Player"))
        {
            _hasTriggerOn = true;
           
        }
    }
}
