using UnityEngine;
using System.Collections.Generic;

public class FSM
{
	private Stack<FSMState> m_stateStack = new Stack<FSMState>();

	public delegate void FSMState(FSM a_fsm, GameObject a_object);

	public void Update(GameObject a_object)
    {
		if (m_stateStack.Peek () != null)
        {
			m_stateStack.Peek ().Invoke (this, a_object);
		}
	}

	public void PushState(FSMState a_state)
    {
		m_stateStack.Push(a_state);
	}

	public void PopState()
    {
		m_stateStack.Pop();
	}
}
