using UnityEngine;
using System.Collections;

public interface IFSMState
{
	void Update (IFSMState a_fsm, GameObject a_obj);
}
