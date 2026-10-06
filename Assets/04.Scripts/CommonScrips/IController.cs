using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerControllerCom;
public class IController : MonoBehaviour
{
    protected MovementCom m_MovementCom;
    protected AnimatorCom m_AnimatorCom;

    /*Common*/
    
    protected Dictionary<int, SkillSO> _dicSkillDescs;
    public SkillSO GetSkillSOByHandle(int handle)
    {
        _dicSkillDescs.TryGetValue(handle, out SkillSO skillSO);
        return skillSO;
    }
    public Dictionary<int, SkillSO> GetDicSkillSos() { return _dicSkillDescs; }
    /*Common*/

    protected void InitializeState(IState _State)
    {
        
        _State.SetController(this);
        _State.SetMovementCom(m_MovementCom);
        _State.SetAnimatorCom(m_AnimatorCom);
        _State.Initialize();
    }
    public void ChangeState(int state)
    {

        if (m_StateList.Count<= state ||0 > state )
        {
            Debug.LogAssertion("OutOfIndex Controller");
            return; 
        }
        /*실제 로직*/
        if (null != m_NowState) m_NowState.Exit();   
        m_NowState = m_StateList[state];
        m_NowState.Enter();

    }
    public IState GetState(int state)
    {
        Debug.Assert(m_StateList.Count> state ||0 <= state);

        return m_StateList[state];
    }
    protected void AddState(IState state)
    {
        m_StateList.Add(state); 
    }
    protected void CreateStateList(int numStates)
    {
        IState[] arrStates = new IState[numStates]; 
        m_StateList = arrStates.ToList<IState>();
    }
    protected void setState(int stateIdx, IState state)
    {
        m_StateList[stateIdx] = state; 
    }
    protected void UpdateState()
    {
        

        m_NowState.UpdateState(); 
    }
    /*list는 이넘으로 관리하겠다 .  */
    //[SerializeField,SerializeReference]
    protected List<IState> m_StateList;
    [SerializeReference]
    IState m_NowState;
    protected IState CurrentState => m_NowState;
}
