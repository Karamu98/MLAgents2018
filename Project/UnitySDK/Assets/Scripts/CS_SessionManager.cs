using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class CS_SessionManager : MonoBehaviour
{
    [SerializeField] public GameObject floor;
    /*[HideInInspector]*/ public GameObject mlAgent;
    /*[HideInInspector]*/ public GameObject goapAgent;


    public void GetAgents()
    {
        mlAgent = GetComponentInChildren<MLAgent>().gameObject;
        goapAgent = GetComponentInChildren<CS_Red>().gameObject;
    }

    public void ResetGame()
    {
        Respawn(mlAgent);
        Respawn(goapAgent);
    }

    public void Respawn(GameObject a_toRespawn)
    {
        Vector3 floorSize = floor.transform.lossyScale;
        float maxSize = floorSize.x;
        if(floorSize.z > maxSize)
        {
            maxSize = floorSize.z;
        }
        // Crude, but keep testing random points until we have a valid one
        for (int i = 0; i < 30; i++)
        {
            float x = Random.Range(floor.transform.position.x - (floorSize.x * 0.5f), floor.transform.position.x + (floorSize.x * 0.5f));
            float y = a_toRespawn.transform.position.y;
            float z = Random.Range(floor.transform.position.z - (floorSize.y * 0.5f), floor.transform.position.z + (floorSize.y * 0.5f));
            Vector3 randomPoint = new Vector3(x, y, z);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1, NavMesh.AllAreas))
            {
                Debug.DrawRay(hit.position, Vector3.up, Color.black, 10);
                a_toRespawn.transform.position = randomPoint;
            }
        }
    }
}
