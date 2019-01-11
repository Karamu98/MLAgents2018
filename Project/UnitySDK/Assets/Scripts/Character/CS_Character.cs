using UnityEngine;
using UnityEngine.AI;

public class CS_Character : MonoBehaviour
{
    [SerializeField] private float Speed = 3;

    // State control
    protected bool bGameActive = false;


    protected virtual void Awake()
    {
        GetComponent<NavMeshAgent>().speed = Speed;
        GetComponent<NavMeshAgent>().autoBraking = true;
    }

    public void OnGameStart()
    {
        bGameActive = true;
    }

    public void OnGameEnd()
    {
        bGameActive = false;
    }
}
