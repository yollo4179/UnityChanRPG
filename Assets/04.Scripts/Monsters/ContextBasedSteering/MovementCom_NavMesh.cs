using UnityEngine;
using Unity.AI;
using UnityEngine.AI;
public class MovementCom_NavMesh : MonoBehaviour
{
    [SerializeField]
    protected Transform m_TargetTransform;
    //[SerializeField]
    //protected Animator m_Animator;
   protected NavMeshAgent NavAgent;
    protected virtual void Awake()
    {
        NavAgent= GetComponent<NavMeshAgent>();
       
    }

    private void Update()
    {
      //  m_Animator.SetFloat("MovementSpeed", Mathf.Clamp01(agent.velocity.magnitude) );

        Vector3 Dir = (m_TargetTransform.position- transform.position).normalized;

        
        if((m_TargetTransform.position - transform.position).magnitude >0.5f)
            NavAgent.SetDestination(m_TargetTransform.position);
        //transform.position+=new Vector3(Time.deltaTime, 0, 0);
    }
}
