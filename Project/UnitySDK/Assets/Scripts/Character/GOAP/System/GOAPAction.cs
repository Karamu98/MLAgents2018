using UnityEngine;
using System.Collections.Generic;

public abstract class GOAPAction : MonoBehaviour
{

	private Dictionary<string,object> preconditions;
	private Dictionary<string,object> effects;

	private bool bInRange = false;

	public float fCost = 1f;

    public GameObject target;

	public GOAPAction()
    {
        // Initialise our effects and preconditions Dictionarys'
		preconditions = new Dictionary<string, object>();
		effects = new Dictionary<string, object>();
	}

	public void OnReset()
    {
		bInRange = false;
		Reset();
	}

	public virtual void Reset()
    {
        target = null;
    }

	public abstract bool IsCompleted();

	public abstract bool CheckPreconditions(GameObject a_agent);

	public abstract bool RunAction(GameObject a_agent);

	public abstract bool RequiresInRange();

	public bool IsInRange()
    {
		return bInRange;
	}

	public void SetInRange(bool a_isInRange)
    {
		bInRange = a_isInRange;
	}

	public void AddPrecondition(string a_key, object a_value)
    {
        preconditions.Add(a_key, a_value);
	}

	public void RemovePrecondition(string a_key)
    {
        preconditions.Remove(a_key);
	}

    public void ModifyPrecondition(string a_key, object a_newValue)
    {
        if(preconditions.ContainsKey(a_key))
        {
            preconditions[a_key] = a_newValue;
        }
    }

	public void AddEffect(string a_key, object a_value)
    {
        effects.Add(a_key, a_value);
	}

	public void RemoveEffect(string a_key)
    {
        effects.Remove(a_key);
	}

	public Dictionary<string, object> Preconditions
    {
        get { return preconditions; }
	}

    public Dictionary<string, object> Effects
    {
        get { return effects; }
    }
}
