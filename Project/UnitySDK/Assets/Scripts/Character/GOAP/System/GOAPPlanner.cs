using System;
using System.Collections.Generic;
using UnityEngine;


public class GOAPPlanner
{
	 /// Returns null if a plan could not be found, or a list of the actions
	 /// that must be performed, in order, to fulfill the goal.
	public Queue<GOAPAction> MakePlan(GameObject agent, HashSet<GOAPAction> availableActions, Dictionary<string, object> worldState, Dictionary<string, object> goal) 
	{
		// Reset the actions so we can start fresh with them
		foreach (GOAPAction action in availableActions)
        {
			action.OnReset();
		}

		// Check what actions can run testing each preconditions
		HashSet<GOAPAction> validActions = new HashSet<GOAPAction>();
		foreach (GOAPAction action in availableActions)
        {
			if (action.CheckPreconditions(agent))
            {
                validActions.Add(action);
            }
		}
		
		// All the performable actions are now in "validActions". As we're using a pathfinding algoritm these can be visualized as our open spaces on a navmesh

		List<Node> leaves = new List<Node>();

		// Create tree
		Node start = new Node (null, 0, worldState, null);
		bool success = BuildGraph(start, leaves, validActions, goal);

		if (!success)
        {
			Debug.Log("NO PLAN");
			return null;
		}

		// If our tree is valid, test all leaves for shortest/best solution
		Node cheapest = null;
		foreach (Node leaf in leaves)
        {
			if (cheapest == null)
            {
                cheapest = leaf;
            }
			else
            {
				if (leaf.m_fRunCost < cheapest.m_fRunCost)
                {
                    cheapest = leaf;
                }
			}
		}

		// Add all of the nodes from the leaf to the root to List
		List<GOAPAction> result = new List<GOAPAction> ();
		Node cheapNode = cheapest;
		while (cheapNode != null)
        {
			if (cheapNode.m_action != null)
            {
                // Add the action to the front to keep the correct order
                result.Insert(0, cheapNode.m_action); 
			}
			cheapNode = cheapNode.m_parent;
		}

        // Push each action into the queue in the now correct order
		Queue<GOAPAction> queue = new Queue<GOAPAction>();
		foreach (GOAPAction a in result) 
{
			queue.Enqueue(a);
		}

		return queue;
	}

    /// Returns true if at least one solution was found.
    /// The possible paths are stored in the leaves list. Each leaf has a
    /// 'RunCost' value where the lowest cost will be the best action sequence.
	protected bool BuildGraph (Node a_parent, List<Node> a_leaves, HashSet<GOAPAction> a_validActions, Dictionary<string, object> a_goal)
	{
		bool bPathFound = false;

		// Check each avaliable action to see if it is still valid here
		foreach (GOAPAction action in a_validActions)
        {
            // Test if the current actions preconditions are met by parent (If actions.Preconditions are met by m_parents' states)
            if (InState(action.Preconditions, a_parent.m_state))
            {
                // Apply the action's effects to the parent state
                Dictionary<string, object> currentState = UpdateState(a_parent.m_state, action.Effects);

                // Create a new node 
				Node node = new Node(a_parent, a_parent.m_fRunCost + action.fCost, currentState, action);

				if (GoalInState(a_goal, currentState))
                {
					// Found a solution
					a_leaves.Add (node);
					bPathFound = true;
				}
                else
                {
					// Test all the remaining actions and branch out the tree
					HashSet<GOAPAction> subset = CreateActionSubset(a_validActions, action);
					bool bFound = BuildGraph(node, a_leaves, subset, a_goal);
					if (bFound)
                    {
                        bPathFound = true;
                    }	
				}
			}
		}
		return bPathFound;
	}


    /// Checks if atleast one goal is met TODO: Figure out how to return paths that complete more goals
    protected bool GoalInState(Dictionary<string, object> test, Dictionary<string, object> state)
    {
        bool match = false;
        foreach (KeyValuePair<string, object> t in test)
        {
            foreach (KeyValuePair<string, object> s in state)
            {
                if (s.Equals(t))
                {
                    match = true;
                    break;
                }
            }
        }
        return match;
    }


    /// Creates a new list of actions excluding the ones that failed to be recursivly tested
	protected HashSet<GOAPAction> CreateActionSubset(HashSet<GOAPAction> a_actions, GOAPAction a_actionToRemove)
    {
        HashSet<GOAPAction> subset = new HashSet<GOAPAction>();
        foreach (GOAPAction a in a_actions)
        {
            if (!a.Equals(a_actionToRemove))
            {
                subset.Add(a);
            } 
        }
        return subset;
    }

    /// Check that all items in 'test' are in 'state'. If just one does not match or is not there then this returns false.
    protected bool InState(Dictionary<string, object> a_testState, Dictionary<string, object> a_state)
    {
        bool allMatch = true;
        foreach (KeyValuePair<string, object> t in a_testState)
        {
            bool match = false;
            foreach (KeyValuePair<string, object> s in a_state)
            {
                if (s.Key == t.Key)
                {
                    if(s.Value.Equals(t.Value))
                    {
                        match = true;
                        break;
                    }
                }
            }
            if (!match)
            {
                allMatch = false;
            }
                
        }
        return allMatch;
    }
	

/// Updates a_currentState to the changes (a_stateChange)
	protected Dictionary<string, object> UpdateState(Dictionary<string, object> a_currentState, Dictionary<string, object> a_stateChange)
    {
        Dictionary<string, object> state = new Dictionary<string, object>();
        // copy the KVPs over as new objects
        foreach (KeyValuePair<string,object> curState in a_currentState)
        {
			state.Add(curState.Key, curState.Value);
		}


        foreach (KeyValuePair<string, object> change in a_stateChange)
        {
            if(state.ContainsKey(change.Key))
            {
                // If the key exists already, update.
                state[change.Key] = change.Value;
            }
            else
            {
                // If not, add it
                state.Add(change.Key, change.Value);
            }
        }
		return state;
	}


    // To represent our actions as nodes for pathfinding
	protected class Node
    {
		public Node m_parent;
		public float m_fRunCost;
		public Dictionary<string, object> m_state;
		public GOAPAction m_action;

		public Node(Node a_parent, float a_runCost, Dictionary<string, object> a_state, GOAPAction a_action)
        {
			this.m_parent = a_parent;
			this.m_fRunCost = a_runCost;
			this.m_state = a_state;
			this.m_action = a_action;
		}
	}

}

