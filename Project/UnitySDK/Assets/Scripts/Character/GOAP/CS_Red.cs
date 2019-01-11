using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class CS_Red : CS_GOAPCharacter
{
    CS_SessionManager manager;

    // This is used to test what our agent should be when it reaches its goal
    private AgentKnowledge goalState;

    // These are used to store positions for different actions
    [HideInInspector] public GameObject InterestLocation;
    [HideInInspector] public GameObject LKP;

    // Since this is a one-off test, no need to make it OO
    public GameObject enemy;

    /// Settings
    [Range(0, 1f)] [SerializeField] private float HitChance;
    [SerializeField] private float FireRate;
    [SerializeField] private int MaxHealth;
    [SerializeField] private int Damage;

    private int Health;
    private float fireTimer = 0;

    protected override void Awake()
    {
        base.Awake();
        manager = GetComponentInParent<CS_SessionManager>();
        manager.GetAgents(); // Clunky way of solving calling issues
        enemy = manager.mlAgent.gameObject;
        Health = MaxHealth;

        // Goal state setup, here we also define all of the events
        goalState = new AgentKnowledge();
        goalState.AddKnowledge(new AgentEvent("seeThreat", true, 0, 0));
        goalState.AddKnowledge(new AgentEvent("seekEnemy", false, 0, 0));
        goalState.AddKnowledge(new AgentEvent("searchArea", true, 0, 0));

        // Setting the inital state of the agent, copy and modify the goal state
        knowledge = goalState;
        knowledge.SetEvent("seeThreat", false, 0);
        knowledge.SetEvent("searchArea", false, 0);
    }

    private void FixedUpdate()
    {
        KnowledgeUpdate();

        fireTimer -= Time.fixedDeltaTime;

        if (CanSee(enemy.transform))
        {
            OnShouldAttack();
        }
        else
        {
            if (knowledge.GetEvent("seeThreat").value.Equals(true))
            {
                // If we could see the enemy, seek
                OnShouldSeek();
            }
        }
    }

    private void KnowledgeUpdate()
    {
        // Test if our agent still cares about this information
        if (knowledge.GetEvent("seekEnemy").value.Equals(true))
        {
            if(!knowledge.GetEvent("seekEnemy").IsValid())
            {
                OnStopBehaviour("seekEnemy");
            }
        }

        // Test if our agent still cares about this information
        if (knowledge.GetEvent("investigating").value.Equals(true))
        {
            if (!knowledge.GetEvent("investigating").IsValid())
            {
                OnStopBehaviour("investigating");
            }
        }
    }

    private void OnStopBehaviour(string a_behaviour)
    {
        knowledge.SetEvent(a_behaviour, false, 0);
        AbortFlag = true;
    }

    public void OnGameReset()
    {
        Health = MaxHealth;
    }

    public void OnShoot()
    {
        if(fireTimer <= 0)
        {
            if (Random.Range(0f, 1f) <= HitChance)
            {
                RaycastHit outHit;
                if (Physics.Linecast(transform.position, enemy.transform.position, out outHit))
                {
                    if (outHit.transform.tag == enemy.transform.tag)
                    {
                        enemy.GetComponent<MLAgent>().TakeDamage(Damage);
                    }
                }
            }
            fireTimer = FireRate;
        }

    }

    private void OnShouldAttack()
    {
        if(knowledge.GetEvent("seeThreat").value.Equals(false))
        {
            knowledge.SetEvent("seeThreat", true, 0);
            knowledge.SetEvent("seekEnemy", false, 0);
            AbortFlag = true;
        }

        GetComponent<NavMeshAgent>().isStopped = true;
    }

    // This is called when this agent should be attacking the enemy
    private void OnShouldSeek()
    {
        // We can no longer see the enemy change state. Set the timestamp to add a time before the info isnt important
        knowledge.SetEvent("seeThreat", false, 0);
        knowledge.SetEvent("seekEnemy", true, 5);

        Destroy(LKP);
        LKP = new GameObject
        {
            name = name + ": GOAP LKP",
            tag = "LKP"
        };

        LKP.transform.position = enemy.transform.position;

        // Call for a new plan
        AbortFlag = true;
    }

    public void TakeDamage(int a_damageToTake)
    {
        Health -= a_damageToTake;

        if(Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        enemy.GetComponent<MLAgent>().ScorePoint();
    }

    public void Heal()
    {
        Health += 30;

        if (Health > 100)
        {
            Health = 100;
        }
    }

    #region GOAPInterface

    public override Dictionary<string, object> GetWorldState()
    {
        Dictionary<string, object> worldData = new Dictionary<string, object>();

        List<AgentEvent> All = knowledge.GetKnowledge();

        foreach(AgentEvent aEvent in  All)
        {
            worldData.Add(aEvent.keyValue, aEvent.value);
        }

        return worldData;
    }

    public override void PlanFound(Dictionary<string, object> goal, Queue<GOAPAction> a_action)
    {
        //Debug.Log("GOAP plan: " + a_action.Peek());
        UpdateTarget();
    }

    public override void PlanAborted(GOAPAction a_action)
    {
        UpdateTarget();
    }


    public override bool ShouldAbort()
    {
        if (AbortFlag)
        {
            GetComponent<NavMeshAgent>().destination = Vector3.zero;
            AbortFlag = false;
            return true;
        }
        return false;
    }


    public override Dictionary<string, object> CreateGoalState()
    {
        Dictionary<string, object> goalState = new Dictionary<string, object>();

        List<AgentEvent> All = knowledge.GetKnowledge();

        foreach (AgentEvent aEvent in All)
        {
            goalState.Add(aEvent.keyValue, aEvent.value);
        }

        return goalState;
    }

    #endregion
}
