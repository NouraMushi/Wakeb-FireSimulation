using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIResponder : MonoBehaviour
{
    public float followDistance = 5f;
    public float fireDetectRange = 14f;
    public float assistRange = 2f;
    public float recheckFiresEvery = 0.5f;
    public float interactCooldown = 0.75f;

    private enum State { Follow, Assist }
    private State state = State.Follow;

    private NavMeshAgent agent;
    private Transform player;
    private FireSource currentTarget;
    private float nextSearchTime;
    private float nextInteractTime;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        agent.isStopped = false;
        agent.stoppingDistance = followDistance * 0.9f;

    }

    void Update()
    {
        if (player == null) return;

        if (currentTarget != null &&
            (!currentTarget.gameObject.activeInHierarchy || currentTarget.IsExtinguished()))
        {
            ClearTargetAndFollow();
        }

        if (Time.time >= nextSearchTime)
        {
            nextSearchTime = Time.time + recheckFiresEvery;
            if (state == State.Follow || currentTarget == null)
            {
                var f = FindNearestFire();
                if (f != null) SetAssistTarget(f);
            }
        }

        if (state == State.Assist && currentTarget != null) AssistBehaviour();
        else FollowBehaviour();
    }

    void FollowBehaviour()
    {
        agent.stoppingDistance = followDistance * 0.9f;
        float dp = Vector3.Distance(transform.position, player.position);
        if (dp > followDistance) agent.SetDestination(player.position);
        else if (agent.hasPath) agent.ResetPath();
    }

    void AssistBehaviour()
    {
        if (currentTarget == null || currentTarget.IsExtinguished())
        {
            ClearTargetAndFollow();
            return;
        }

        Vector3 firePos = currentTarget.GetWorldFirePos();
        Vector3 navPoint = GetReachablePointNear(firePos, 3.0f);

        agent.stoppingDistance = assistRange * 0.9f;
        agent.SetDestination(navPoint);


        if (!agent.pathPending)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                ClearTargetAndFollow();
                return;
            }
        }


        float d = Vector3.Distance(transform.position, navPoint);
        if (d <= assistRange && Time.time >= nextInteractTime)
        {
            currentTarget.OnInteract();
            nextInteractTime = Time.time + interactCooldown;
            ClearTargetAndFollow();
        }
    }


    void SetAssistTarget(FireSource f)
    {
        if (f != null && !f.IsExtinguished())
        {
            currentTarget = f;
            state = State.Assist;
            agent.ResetPath();
        }
    }

    void ClearTargetAndFollow()
    {
        currentTarget = null;
        state = State.Follow;
        agent.ResetPath();
        nextSearchTime = Time.time + recheckFiresEvery;
    }


    FireSource FindNearestFire()
    {
        return FireManager.Instance?.FindNearestActiveFire(transform.position, fireDetectRange);
    }

    Vector3 GetReachablePointNear(Vector3 target, float maxSampleRadius = 3f)
    {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, maxSampleRadius, NavMesh.AllAreas))
            return hit.position;
        return target;
    }

}

