using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;


public class MLAgent : Agent
{
    CS_SessionManager manager;
    BlueAcademy academy;

    /// Settings
    [SerializeField] private int MaxHealth;
    [SerializeField] private int Damage;
    [SerializeField] private float fovAngle;
    [SerializeField] private float fovRadius;
    [SerializeField] private float rayRadius;
    [SerializeField] private float[] rayAngles;
    [SerializeField] private string[] detectableObjects;
    [SerializeField] private float AgentSpeed;
    [SerializeField] private float HitChance;
    [SerializeField] private float FireRate;

    /// Observations (17 Obs)
    private bool bCanSeeEnemy = false;
    private float enemyDir;
    private RayPerception perception;
    private int Health;
    //private float[] healthSightData = new float[4]; // Storing the boolean with the dir, this theoretically will allow for faster learning (0 = bool, 123 = dir)

    /// Constants
    private GameObject enemy;
    private Rigidbody rBody;

    /// Cache
    private List<GameObject> healthPacks;
    private Material groundMaterial;
    private float fireTimer = 0;

    private void Awake()
    {
        manager = GetComponentInParent<CS_SessionManager>();
        manager.GetAgents();
        groundMaterial = manager.floor.GetComponent<Renderer>().material;
        academy = FindObjectOfType<BlueAcademy>();
        enemy = manager.goapAgent;
        perception = GetComponent<RayPerception>();
        rBody = GetComponent<Rigidbody>();
        Health = MaxHealth;
    }

    private void FixedUpdate()
    {
        fireTimer -= Time.fixedDeltaTime;
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        manager.floor.GetComponent<Renderer>().material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        manager.floor.GetComponent<Renderer>().material = groundMaterial;
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();

    }

    private void UpdateLOSOnEnemy()
    {
        Vector3 dirToTarget = (enemy.transform.position - transform.position).normalized;



        if (Vector3.Angle(transform.forward, dirToTarget) < fovAngle * 0.5f)
        {
            RaycastHit outHit;
            if (Physics.Raycast(transform.position, dirToTarget, out outHit, fovRadius + 1))
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.green, 1);
                if (outHit.transform.gameObject.tag == enemy.transform.transform.gameObject.tag)
                {
                    bCanSeeEnemy = true;
                    Quaternion newQ = Quaternion.LookRotation(dirToTarget, Vector3.up);
                    enemyDir = newQ.eulerAngles.y / 360;
                    return;
                }

            }
        }
        bCanSeeEnemy = false;
        enemyDir = -1;
    }

    private void UpdateHealthLOS()
    {
        // TODO
    }

    private void UpdateObservations()
    {
        UpdateLOSOnEnemy();
    }

    public override void CollectObservations()
    {
        UpdateObservations();
        AddVectorObs(bCanSeeEnemy);
        AddVectorObs((enemyDir));
        float rotDir = transform.rotation.eulerAngles.y / 360;
        //Debug.Log("Enemy: " + enemyDir);
        //Debug.Log("Ours: " + rotDir);
        AddVectorObs(rotDir);
        AddVectorObs(perception.Perceive(rayRadius, rayAngles, detectableObjects, 0, 0));
        AddVectorObs(Health/MaxHealth);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        base.AgentAction(vectorAction, textAction);

        Vector3 force = Vector3.zero;
        Vector3 dir = Vector3.zero;

        int action = Mathf.FloorToInt(vectorAction[0]);
        //Debug.Log(action);

        switch (action)
        {
            case 0:
                {
                    break;
                }
            case 1:
                {
                    force = transform.forward * 1f;
                    break;
                }
            case 2:
                {
                    force = transform.forward * -1f;
                    break;
                }
            case 3:
                {
                    force = transform.right * -1f;
                    break;
                }
            case 4:
                {
                    force = transform.right * 1f;
                    break;
                }
            case 5:
                {
                    dir = transform.up * -0.75f;
                    break;
                }
            case 6:
                {
                    dir = transform.up * 0.75f;
                    break;
                }
            case 7:
                {
                    Shoot();
                    break;
                }
        }

        transform.Rotate(dir, Time.fixedDeltaTime * 200f);
        rBody.AddForce(force * academy.agentRunSpeed, ForceMode.Impulse);

        if(rBody.velocity.magnitude > academy.agentRunSpeed)
        {
            rBody.velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, academy.agentRunSpeed);
        }
    }

    public void Shoot()
    {
        if (fireTimer <= 0)
        {
            RaycastHit outHit;
            Vector3 end = transform.position + (new Vector3(transform.forward.x, 0, transform.forward.z) * 1000);
            Debug.DrawLine(transform.position, end, Color.blue, 10);
            if (Physics.Linecast(transform.position, transform.position + (transform.forward * 1000), out outHit))
            {
                if (outHit.transform.tag == enemy.transform.tag)
                {
                    enemy.GetComponent<CS_Red>().TakeDamage(Damage);
                    AddReward(0.2f);
                }

            }
            fireTimer = FireRate;
        }
    }

    public override void AgentReset()
    {
        base.AgentReset();
        Health = MaxHealth;
        enemy.GetComponent<CS_Red>().OnGameReset();
        manager.ResetGame();
        

    }


    public void ScorePoint()
    {
        // Give the agent a reward for scoring a point
        AddReward(1f);

        Done();
        manager.ResetGame();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
    }

    public void TakeDamage(int a_damageToTake)
    {
        Health -= a_damageToTake;

        AddReward(-0.1f);

        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        AddReward(-1f);
        Done();
        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(academy.failMaterial, 0.5f));
    }

    public void Heal()
    {
        Health += 30;

        if(Health > 100)
        {
            Health = 100;
        }
    }
}
