using Unity.VisualScripting;
using UnityEngine;

public class AnimEvent_CrabGuy : MonoBehaviour
{
 
    EQSQuerier Agent; 
    public void Awake()
    {
        Agent = GetComponent<EQSQuerier>();
    }
   public void AttackEnd()
    {
       // Agent.SetAttackFlagTo(true);
    }
}
