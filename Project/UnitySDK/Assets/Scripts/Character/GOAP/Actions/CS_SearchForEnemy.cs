using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class CS_SearchForEnemy : GOAPAction
{
    [SerializeField] private float fSearchRadius = 10;
    private bool bHasSearched = false;

    public CS_SearchForEnemy()
    {
        AddPrecondition("searchArea", false);
        AddEffect("searchArea", true);
    }

    public override bool IsCompleted()
    {
        return bHasSearched;
    }

    public override bool RequiresInRange()
    {
        return true;
    }

    public override void Reset()
    {
        bHasSearched = false;
        Destroy(target);
    }

    public override bool RunAction(GameObject a_agent)
    {
        if (target != null)
        {
            // If the target is still valid
            bHasSearched = true;
            Destroy(target);
            return true;
        }
        return false;
    }

    public override bool CheckPreconditions(GameObject a_agent)
    { 
        // Crude, but keep testing random points until we have a valid one
        for(int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = (Random.insideUnitSphere * fSearchRadius);
            randomPoint += transform.position;
            randomPoint.y = a_agent.transform.position.y + 1;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, fSearchRadius, NavMesh.AllAreas))
            {
                Debug.DrawRay(hit.position, Vector3.up, Color.black, 10);
                target = new GameObject("GOAP Search Target");
                target.transform.position = hit.position;
                return true;
            }
        }



        if (target == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
