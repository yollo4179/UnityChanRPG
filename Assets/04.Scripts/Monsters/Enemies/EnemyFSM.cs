using UnityEngine;
using Unity.Behavior;
using UnityEngine.AI;
using NUnit.Framework;
public class EnemyFSM : MonoBehaviour
{
    [SerializeField]Transform _target;
    [SerializeField] NavMeshAgent _agent;
    [SerializeField] BehaviorGraphAgent _behaviorAgent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetUp(Transform target ,  GameObject[]  wayPoints)
    {
        _agent= GetComponent<NavMeshAgent>();
        
        _behaviorAgent = GetComponent<BehaviorGraphAgent>();
        _behaviorAgent.SetVariableValue("PatrolPoints", wayPoints);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
