using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VehicleAutoResponder : MonoBehaviour
{
    [Header("Navigation")]
    public float stoppingDistance = 8f;       // distance to stop from the fire
    public float sampleRadius = 6f;           // radius to sample NavMesh near the fire
    public float recheckFiresEvery = 0.5f;    // every few seconds, recheck for new fires
    public float waitAtDestination = 2f;      // wait time at fire before searching for next

    [Header("Smoothing")]
    public float maxSpeed = 5.5f;              // car max speed
    public float accel = 5f;                  // cceleration (for smoothness)
    public float decel = 10f;                 // deceleration (for smoothness)

    private NavMeshAgent agent;
    private FireSource currentTarget;
    private float nextSearchTime;
    private float currentSpeed;               
    private float waitUntilTime;             
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        
        agent.updateRotation = true;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.baseOffset = 0f;

        agent.speed = 0f;
        agent.acceleration = 100f; 
        agent.angularSpeed = 120f; 
    }

    void Start()
    {
        
        if (NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    void Update()
    {
        
        if (Time.time < waitUntilTime)
        {
            SmoothSpeed(0f);
            return;
        }

        
        if (currentTarget != null && (!currentTarget.gameObject.activeInHierarchy || currentTarget.IsExtinguished()))
        {
            ClearTarget();
        }

        if (currentTarget == null && Time.time >= nextSearchTime)
        {
            currentTarget = FindNearestActiveFire();
            nextSearchTime = Time.time + recheckFiresEvery;

            if (currentTarget != null)
            {
                Vector3 navPoint = GetReachablePointNear(currentTarget, sampleRadius);
                SetDestinationSmart(navPoint);
            }
        }

        if (currentTarget != null)
        {
            Vector3 navPoint = GetReachablePointNear(currentTarget, sampleRadius);
            if (!agent.pathPending)
            {
                if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    ClearTarget();
                    return;
                }
            }

            if ((agent.destination - navPoint).sqrMagnitude > 1.0f)
                SetDestinationSmart(navPoint);

           
            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stoppingDistance))
            {
                
                waitUntilTime = Time.time + waitAtDestination;
                ClearTarget(keepSearchCooldown: true);
                SmoothSpeed(0f);
                return;
            }

          
            SmoothSpeed(maxSpeed);
        }
        else
        {
            SmoothSpeed(0f);
        }
    }

    void SetDestinationSmart(Vector3 point)
    {
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(point);
    }

    void SmoothSpeed(float target)
    {
        float rate = (target > currentSpeed) ? accel : decel;
        currentSpeed = Mathf.MoveTowards(currentSpeed, target, rate * Time.deltaTime);
        agent.speed = currentSpeed;
    }

    void ClearTarget(bool keepSearchCooldown = false)
    {
        currentTarget = null;
        agent.ResetPath();
        if (!keepSearchCooldown) nextSearchTime = Time.time + recheckFiresEvery;
    }


    FireSource FindNearestActiveFire()
    {
        return FireManager.Instance?.FindNearestActiveFire(transform.position);
    }


    Vector3 GetReachablePointNear(FireSource fire, float radius)
    {
        Vector3 target = (fire && fire.fireParticles) ? fire.fireParticles.transform.position : fire.transform.position;

        
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
        dir.Normalize();

        Vector3 probe = target - dir * Mathf.Max(stoppingDistance + 1.5f, 3f);

        if (NavMesh.SamplePosition(probe, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;

        if (NavMesh.SamplePosition(target, out hit, radius, NavMesh.AllAreas))
            return hit.position;

       
        return target;
    }


}
