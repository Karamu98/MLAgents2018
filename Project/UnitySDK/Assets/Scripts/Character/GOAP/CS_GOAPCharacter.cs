using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



/// This exists because Guards need to inheriet from character but may be using BehaviourTrees and not GOAP
public class CS_GOAPCharacter : CS_Character, IGOAP
{
    [SerializeField] private float fHearingRange = 20; // Anything inside this will be tested
    [SerializeField] private float fWallDampening = 5; // How much walls dampen sound

    [SerializeField] protected LayerMask targetMask;
    [SerializeField] protected LayerMask obstacleMask;
    [SerializeField] protected float viewRadius = 3;
    [SerializeField] [Range(0, 360)] protected float viewAngle = 180;
    public AgentKnowledge knowledge;
    private int meshResolution = 1;
    protected List<Transform> visibleTargets;
    private MeshFilter fovMesh;
    private Mesh viewMesh;
    private bool bOnce = false;
    public bool AbortFlag = false;

    private int edgeResolveIterations = 4;
    private float edgeDstThreshold = 0.5f;
    private float maskCutawayDst = .1f;

    public void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, fHearingRange);
    }

    protected override void Awake()
    {
        base.Awake();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        fovMesh = GetComponent<MeshFilter>();
        fovMesh.mesh = viewMesh;
        visibleTargets = new List<Transform>();
        knowledge = new AgentKnowledge();
    }

    protected bool CanSee(Transform a_target)
    {
        Vector3 dirToTarget = (a_target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)
        {
            RaycastHit outHit;
            if (Physics.Raycast(transform.position, dirToTarget, out outHit, viewRadius + 1))
            {
                Debug.DrawLine(transform.position, a_target.position, Color.green, 1);
                if (outHit.transform.gameObject.tag == a_target.transform.gameObject.tag)
                {
                    return true;
                }
                
            }
        }
        return false;
    }

    protected bool CanHear(Transform a_target)
    {
        float distanceBetween = Vector3.Distance(transform.position, a_target.position);
        if (distanceBetween < fHearingRange)
        {
            Vector3 direction = (a_target.position - transform.position).normalized;
            Debug.DrawLine(transform.position, a_target.position, Color.red, 1);

            // Raycast to see how many walls we hit
            Ray ray = new Ray(transform.position, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, fHearingRange);

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    distanceBetween += fWallDampening;
                }

                if (distanceBetween < fHearingRange)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    protected void UpdateTarget()
    {
        bOnce = false;
    }


    private void LateUpdate()
    {
        DrawFieldOfView();
    }


#region FOVDrawing(Sebastian Lague)


    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle * 0.5f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

#endregion

#region GOAPInterface

public virtual Dictionary<string, object> GetWorldState()
    {
        Debug.Log("Derived not set 'GetWorldState'");
        return null;
    }


    public virtual Dictionary<string, object> CreateGoalState()
    {
        Debug.Log("Derived not set 'CreateGoalState'");
        return null;
    }

    public virtual bool ShouldAbort()
    {
        return false;
    }

    public virtual bool MoveAgent(GOAPAction a_action)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        if (!bOnce)
        {
            agent.SetDestination(a_action.target.transform.position);
            agent.isStopped = false;
            bOnce = true;
        }

        // We're not at our destination
        float dist = Vector3.Distance(a_action.target.transform.position, transform.position);
        if (dist > 1)
        {
            return false;
        }
        else
        {
            // Clear our desination and tell action we're in range
            agent.SetDestination(Vector3.zero);
            agent.isStopped = true;
            a_action.SetInRange(true);
            bOnce = false;
            return true;
        }
    }

    public virtual void PlanAborted(GOAPAction a_action)
    {
        Debug.Log("Derived not set 'PlanAborted'");
    }

    public virtual void ActionsComplete()
    {
        //Debug.Log("Derived not set 'ActionsComplete'");
    }

    public virtual void PlanFound(Dictionary<string, object> goal, Queue<GOAPAction> a_action)
    {
        Debug.Log("Derived not set 'PlanFound'");
    }

    public virtual void PlanFailed(Dictionary<string, object> a_failedGoal)
    {
        Debug.Log("Derived not set 'PlanFailed'");
    }

    #endregion

}
