using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class CS_GInvestigate : GOAPAction
{
    public float fSearchRadius = 10;
    private bool bReachedDestination = false;

    public CS_GInvestigate()
    {
        AddPrecondition("investigating", true);
        AddEffect("investigating", false);
    }

    public override bool IsCompleted()
    {
        return bReachedDestination;
    }

    public override bool RequiresInRange()
    {
        return true;
    }

    public override void Reset()
    {
        Destroy(target);
        
        bReachedDestination = false;
    }

    public override bool RunAction(GameObject a_agent)
    {
        bReachedDestination = true;

        Destroy(target);
        return true;
    }

    public override bool CheckPreconditions(GameObject a_agent)
    {
        if (a_agent.GetComponent<CS_Red>().knowledge.GetEvent("investigating").IsValid() &&
            a_agent.GetComponent<CS_Red>().knowledge.GetEvent("investigating").value.Equals(true))
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = (Random.insideUnitSphere * fSearchRadius);
                randomPoint += a_agent.GetComponent<CS_Red>().InterestLocation.transform.position;
                randomPoint.y = a_agent.transform.position.y + 1;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, fSearchRadius, NavMesh.AllAreas))
                {
                    Debug.DrawRay(hit.position, Vector3.up, Color.green, 10);
                    target = new GameObject("Guard Search Target");
                    target.transform.position = hit.position;
                    return true;
                }
            }

        }
        else if(!a_agent.GetComponent<CS_GOAPCharacter>().knowledge.GetEvent("investigating").IsValid() && a_agent.GetComponent<CS_GOAPCharacter>().knowledge.GetEvent("investigating").value.Equals(true))
        {
            a_agent.GetComponent<CS_Red>().knowledge.SetEvent("investigating", false, 0);
            Destroy(a_agent.GetComponent<CS_Red>().InterestLocation);
            return false;
        }

        if (target != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
