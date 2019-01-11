using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public sealed class GOAPAgent : MonoBehaviour
{
    // Create our FSM basics
	private FSM stateMachine;
	private FSM.FSMState idleState;
	private FSM.FSMState moveToState;
	private FSM.FSMState performActionState;

    // GOAP data
	private HashSet<GOAPAction> availableActions;
	private Queue<GOAPAction> currentActions;
	private IGOAP dataProvider; // The data the agent finds "CS_Character"
	private GOAPPlanner planner;


	// Use this for initialization
	void Start ()
    {
		stateMachine = new FSM();
		availableActions = new HashSet<GOAPAction>();
		currentActions = new Queue<GOAPAction>();
		planner = new GOAPPlanner();
		FindDataProvider();
		CreateIdle();
		CreateMoveTo();
		CreatePerformAction();
		stateMachine.PushState(idleState);
		GetActions();
	}
	
	// Update is called once per frame
	void Update ()
    {
		stateMachine.Update(gameObject);
	}

	public void NewAction(GOAPAction a_newActionToAdd)
    {
		availableActions.Add(a_newActionToAdd);
	}

	public GOAPAction GetAction(Type action)
    {
		foreach (GOAPAction currAction in availableActions)
        {
			if(currAction.GetType().Equals(action))
            {
				return currAction;
			}
		}

		return null;
	}

	public void RemoveAction(GOAPAction action)
    {
		availableActions.Remove (action);
	}

	private bool HasActionPlan()
    {
		return currentActions.Count > 0;
	}

	private void CreateIdle()
    {
		idleState = (fsm, obj) =>
        { 
			Dictionary<string, object> worldState = dataProvider.GetWorldState();
            Dictionary<string, object> goal = dataProvider.CreateGoalState();

			Queue<GOAPAction> plan = planner.MakePlan(gameObject, availableActions, worldState, goal);
			if (plan != null)
            {
                // If we could make a plan, set it to be processed and tell provider that we've found a plan (To begin processing it)
				currentActions = plan;
				dataProvider.PlanFound(goal, plan);

				fsm.PopState();
				fsm.PushState (performActionState);
			}
            else
            {
                // Return to the provider what goal could not be met
				dataProvider.PlanFailed(goal);
				fsm.PopState();
				fsm.PushState(idleState);
			}
		};
	}

	private void CreateMoveTo()
    {
		moveToState = (fsm, gameObject) => 
        {
			GOAPAction action = currentActions.Peek();
			if (action.RequiresInRange() && action.target == null)
            {
				fsm.PopState();
				fsm.PopState();
				fsm.PushState(idleState);
				return;
			}


            // Move the agent
            if (dataProvider.MoveAgent(action))
            {
                fsm.PopState();
            }

            if (dataProvider.ShouldAbort())
            {
                fsm.PopState();
                CreateIdle();
                fsm.PushState(idleState);
                dataProvider.PlanAborted(action);
                return;
            }
		};
	}

	private void CreatePerformAction()
    {
		performActionState = (fsm, obj) => 
        {
            // If we dont have a plan, go to idle and tell provider
			if (!HasActionPlan())
            {
				fsm.PopState();
				fsm.PushState(idleState);
				dataProvider.ActionsComplete();
				return;
			}

			GOAPAction action = currentActions.Peek();
			if (action.IsCompleted ())
            {
				currentActions.Dequeue ();
			}

            // If we have a plan
			if (HasActionPlan())
            {
				action = currentActions.Peek();
				bool inRange = action.RequiresInRange() ? action.IsInRange() : true;

                // If our action we want to perform is in range
				if (inRange)
                {
                    // Try run our action
					bool success = action.RunAction(obj);
					if (!success)
                    {
                        // If the action failed, go idle and notify provider
						fsm.PopState();
						fsm.PushState(idleState);
						CreateIdle();
						dataProvider.PlanAborted(action);
					} 
				}
                else
                {
                    // If we're not in range, move to target first
					fsm.PushState(moveToState);
				}
			}
		};
	}

	private void FindDataProvider()
    {
        // Loop through all components attached until a provider is found (An Guard/Spy script)
		foreach (Component component in gameObject.GetComponents(typeof(Component)))
        {
			if (typeof(IGOAP).IsAssignableFrom(component.GetType()))
            {
				dataProvider = (IGOAP)component;
				return;
			}
		}
	}

	private void GetActions()
    {
		GOAPAction[] actions = gameObject.GetComponents<GOAPAction>();
		foreach (GOAPAction action in actions)
        {
			availableActions.Add(action);
		}
	}
}
