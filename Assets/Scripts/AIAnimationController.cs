using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class AIAnimationController : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 10f;
    public AudioSource audioSource;
    public AudioClip[] footstepClips;
    public AudioClip landClip;

    private Animator animator;
    private NavMeshAgent agent;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
    private static readonly int GroundedHash = Animator.StringToHash("Grounded");
    private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        animator.applyRootMotion = false;
        agent.updateRotation = false;
    }

    void Update()
    {

        Vector3 vel = agent.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        float motionSpeed = (agent.speed > 0.01f) ? Mathf.Clamp01(speed / agent.speed) : 0f;

        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetFloat(MotionSpeedHash, motionSpeed, 0.1f, Time.deltaTime);

        animator.SetBool(GroundedHash, true);
        animator.SetBool(FreeFallHash, false);
        animator.SetBool(JumpHash, false);


        if (speed > 0.05f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }


    public void OnFootstep(AnimationEvent evt)
    {
        if (audioSource != null && footstepClips != null && footstepClips.Length > 0)
        {
            var clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }

    }


    public void OnLand(AnimationEvent evt)
    {
        if (audioSource != null && landClip != null)
        {
            audioSource.PlayOneShot(landClip);
        }

    }
}

