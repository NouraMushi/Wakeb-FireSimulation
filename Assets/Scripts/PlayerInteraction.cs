using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 5f;
    public LayerMask interactLayerMask;

    private Camera playerCamera;
    private NavMeshAgent agent;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction helpAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        // Get actions from the same "Player" action map used by StarterAssets
        var map = playerInput.actions.FindActionMap("Player", true);
        interactAction = map.FindAction("Interact", true);
        helpAction     = map.FindAction("Help", true);

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.speed = 0f;
        agent.acceleration = 0f;
        agent.angularSpeed = 0f;
        agent.autoBraking = true;

        agent.radius = 0.5f;
        agent.height = 2.0f;
        agent.avoidancePriority = 20;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    void OnEnable()
    {
        interactAction.performed += OnInteractPerformed;
        helpAction.performed     += OnHelpPerformed;
        interactAction.Enable();
        helpAction.Enable();
    }

    void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        helpAction.performed     -= OnHelpPerformed;
        interactAction.Disable();
        helpAction.Disable();
    }

    void Start()
    {
        playerCamera = Camera.main;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx) => TryInteract();
    private void OnHelpPerformed(InputAction.CallbackContext ctx)     => TryHelp();

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactLayerMask))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log("Interacted with: " + hit.collider.name);
              
                if (interactable is InjuredNPC) return; 
                interactable.OnInteract();
            }
        }
    }

    void TryHelp()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            var injured = hit.collider.GetComponentInParent<InjuredNPC>();
            if (injured != null)
            {
                Debug.Log("Helping: " + injured.name);
                injured.OnInteract();
            }
        }
    }
}
