using UnityEngine;

public class CS_GFlee : GOAPAction
{
    private bool bHasLostEnemy = false;

    public CS_GFlee()
    {
        AddPrecondition("seenEnemy", true);
        AddEffect("seenEnemy", false);
    }

    public override bool IsCompleted()
    {
        return bHasLostEnemy;
    }

    public override bool RequiresInRange()
    {
        return true;
    }

    public override void Reset()
    {
        bHasLostEnemy = false;
        Destroy(target);
    }

    public override bool RunAction(GameObject a_agent)
    {
        if (target != null)
        {
            
            // If the target is still valid
            bHasLostEnemy = true;
            a_agent.GetComponent<CS_Red>().knowledge.SetEvent("seeThreat", false, 0);

            a_agent.transform.LookAt(target.transform.position);

            Vector3 checkDir = a_agent.transform.position - (a_agent.transform.forward * 1);
            a_agent.transform.LookAt(checkDir);
            Destroy(target);
            return true;
        }
        return false;
    }

    public override bool CheckPreconditions(GameObject a_agent)
    {
        if (a_agent.GetComponent<CS_Red>().knowledge.GetEvent("seeThreat").value.Equals(false))
        {
            if(target != null)
            {
                Destroy(target);
            }
            return false;
        }

        if(target == null)
        {
            // Raycast in all directions exept guards and find the route that gives longest travel
            Ray[] directions = new Ray[4];
            RaycastHit[] outHits = new RaycastHit[4];
            Vector2 longestResult = new Vector2(); // x stores the distance and y stores the iteration of the longest
            longestResult.x = -1;

            directions[0] = new Ray(a_agent.transform.position, new Vector3(0, 0, 1));
            directions[1] = new Ray(a_agent.transform.position, new Vector3(0, 0, -1));
            directions[2] = new Ray(a_agent.transform.position, new Vector3(1, 0, 0));
            directions[3] = new Ray(a_agent.transform.position, new Vector3(-1, 0, 0));

            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(directions[i], out outHits[i]))
                {
                    if(outHits[i].transform.gameObject.layer == LayerMask.NameToLayer("Guard"))
                    {
                        continue;
                    }

                    if (outHits[i].distance > longestResult.x)
                    {
                        longestResult = new Vector2(outHits[i].distance, i);
                    }
                }
            }



            // Create a point at the longest distance
            target = new GameObject();
            target.name = "GOAP Flee Location";
            target.transform.position = a_agent.transform.position + (directions[(int)longestResult.y].direction * (longestResult.x - 1));

            Debug.DrawLine(a_agent.transform.position, target.transform.position, Color.blue, 10);
        }

        return true;
    }
}
