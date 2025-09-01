using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class InjuredNPC : MonoBehaviour, IInteractable
{
    [Header("Ownership")]
    public Transform houseRoot;          

    [Header("Behaviour")]
    public float stopAtSafeDistance = 1.2f;
    public Transform[] safePoints;

    private Animator animator;
    private NavMeshAgent agent;

    // Animator params
    private static readonly int SpeedHash     = Animator.StringToHash("Speed");
    private static readonly int IsInjuredHash = Animator.StringToHash("IsInjured");
    private static readonly int IsPanicHash   = Animator.StringToHash("IsPanicking");
    private static readonly int HelpedHash    = Animator.StringToHash("Helped");

    private bool isInjured   = false;   
    private bool isPanicking = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent    = GetComponent<NavMeshAgent>();

        animator.applyRootMotion = false;
        agent.updateRotation = true;

        animator.SetBool(IsInjuredHash, isInjured);
        animator.SetBool(IsPanicHash,   isPanicking);
    }

    void Start()
    {
        
        InjuredManager.Instance?.Register(this);

        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;

        
        float speed = agent.velocity.magnitude;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);

      
        if (!isInjured && HouseOnFire())
        {
            isInjured   = true;
            isPanicking = true;
            animator.SetBool(IsInjuredHash, true);
            animator.SetBool(IsPanicHash,   true);
        }

        if (isInjured)
        {
            if (isPanicking)
            {
               
                Vector3 target = GetBestSafePoint();
                if ((agent.destination - target).sqrMagnitude > 0.25f)
                    agent.SetDestination(target);

                
                if (!agent.pathPending &&
                    agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, stopAtSafeDistance))
                {
                    isPanicking = false;
                    animator.SetBool(IsPanicHash, false);
                    agent.ResetPath();
                }
            }
            else
            {
             
                if (agent.hasPath) agent.ResetPath();
            }

            return; 
        }
    }

   
    bool HouseOnFire()
    {
        if (houseRoot == null) return false;
        var fires = houseRoot.GetComponentsInChildren<FireSource>(includeInactive: true);
        foreach (var f in fires)
        {
            if (!f || f.IsExtinguished()) continue;
            return true;
        }
        return false;
    }

    
    Vector3 GetBestSafePoint()
    {
        if (safePoints == null || safePoints.Length == 0)
            return transform.position;

        Transform best = safePoints[0];
        float bestD = Vector3.Distance(transform.position, best.position);
        for (int i = 1; i < safePoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, safePoints[i].position);
            if (d < bestD) { best = safePoints[i]; bestD = d; }
        }

        return NavMesh.SamplePosition(best.position, out var hit, 2f, NavMesh.AllAreas)
            ? hit.position : best.position;
    }



    
    public void OnInteract()
    {

        if (!isInjured) return;

        isInjured = false;
        isPanicking = false;

        animator.SetBool(IsInjuredHash, false);
        animator.SetBool(IsPanicHash, false);
        animator.SetTrigger(HelpedHash);

        if (agent.hasPath) agent.ResetPath();
        agent.isStopped = true;

        InjuredManager.Instance?.NotifyHelped(this);
    }
}
