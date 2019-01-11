using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class CS_Shoot : GOAPAction
{
    private bool isComplete = false;

    public CS_Shoot()
    {
        AddPrecondition("seeThreat", true);
        AddEffect("seeThreat", false);
        AddEffect("searchArea", false);
    }

    public override bool IsCompleted()
    {
        return isComplete;
    }

    public override bool RequiresInRange()
    {
        return false;
    }

    public override void Reset()
    {
        target = null;
        isComplete = false;
    }

    public override bool RunAction(GameObject a_agent)
    {
        a_agent.GetComponent<CS_Red>().OnShoot();
        isComplete = true;

        return true;
    }

    public override bool CheckPreconditions(GameObject a_agent)
    {
        target = a_agent.GetComponent<CS_Red>().enemy.gameObject;
        if (target != null && a_agent.GetComponent<CS_Red>().knowledge.GetEvent("seeThreat").value.Equals(true))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
