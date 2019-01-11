using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IGOAP
{
	Dictionary<string, object> GetWorldState();

    Dictionary<string, object> CreateGoalState();

	void PlanFailed(Dictionary<string, object> a_failedGoal);

	void PlanFound(Dictionary<string, object> a_goal, Queue<GOAPAction> a_actions);

	void ActionsComplete();

	void PlanAborted(GOAPAction a_aborter);

	bool MoveAgent(GOAPAction a_nextAction);

    bool ShouldAbort();
}
