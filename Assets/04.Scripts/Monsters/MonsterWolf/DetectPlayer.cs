using System.Linq;
using Unity.Behavior;
using UnityEngine;

public class DetectPlayer : MonoBehaviour
{

    LayerMask _playerMask;
    LayerMask _occludeMask;
    [SerializeField] private float _radius;
    [SerializeField] private float _sightAngle;
    private bool _isDetected = false;
    Transform _target;

    BehaviorGraphAgent _behaviorGraphAgent; 
    
    private void Start()
    {
        _playerMask = LayerMask.GetMask("Player");
        _occludeMask = LayerMask.GetMask("Obstacles");

        _behaviorGraphAgent =GetComponent<BehaviorGraphAgent>();
        
    }
    // Update is called once per frame
    void Update()
    {

        _isDetected =false;
        Collider[] arrCol  = Physics.OverlapSphere(transform.position, _radius, _playerMask);
        if (arrCol.Length<=0) return; 
        else
        {
            arrCol.ToArray<Collider>().Where(x => x.name == "Player");   
        }
        //서버라면 반복문으로 첫번째 타겟을 가져오는것이 아니라
        /*각도 필터로 거른다.*/
        Vector3 DirToTarget = (arrCol[0].transform.position -transform.position).normalized;
        float dot = Vector3.Dot(DirToTarget, transform.forward.normalized);

        Vector3 _offset =  Vector3.up*0.25f;

       
        if (Mathf.Acos(dot)*Mathf.Rad2Deg < _sightAngle*0.5f )
        {
            
         
            
            if (Physics.Raycast(
                _offset+transform.position,
                DirToTarget,
                out RaycastHit hit,
                _radius,
                _playerMask,
                QueryTriggerInteraction.Ignore
                ))
            {
                if (hit.collider.includeLayers ==_playerMask) {
                    _target = hit.collider.transform; // 시야 OK  -->To do Querier에 TargetSetting 
                    _isDetected=true;
                    /*Success*/
                }
                else _target = null;

            }
            
        }

        _behaviorGraphAgent.SetVariableValue(BlackboardKeys.IS_TARGET_DETECTED, _isDetected);
        /*fail*/

    }
    public void DisableTargetDetector()
    {
        _isDetected = false;
        _behaviorGraphAgent.SetVariableValue(BlackboardKeys.IS_TARGET_DETECTED, _isDetected);
        enabled = false;
    }
    public void EnableTargetDetector()
    {
     
        _behaviorGraphAgent.SetVariableValue(BlackboardKeys.IS_TARGET_DETECTED, _isDetected);
        enabled = true;
    }
    public void OnSceneGUI()
    {
        
    }

    public void OnDrawGizmos()
    {
        Gizmos.color =new Color(1f, 0f, 0f, 0.3f);
        if (_isDetected)
            Gizmos.color =new Color(0f, 1f, 0f, 0.2f);
        int seg = 40;

        Vector3 src = transform.position + Vector3.up*0.1f;

        Vector3 stdVec = transform.forward.normalized;

        Vector3 startVector = Quaternion.Euler(0, -_sightAngle*0.5f, 0)*stdVec * _radius;

        float surplus = _sightAngle /seg;
        for (int i = 0; i<seg; ++i)
        {
            Vector3 to = src + Quaternion.Euler(0, surplus* i, 0)*startVector;
            Gizmos.DrawLine(src, to);
        }

    }
}
