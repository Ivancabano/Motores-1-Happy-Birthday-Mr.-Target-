using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTarget;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference dashAction;

    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float aimSpeed = 3.5f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Movimiento")]
    [SerializeField] private float acceleration = 20f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Gravedad")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedForce = -2f;

    [Header("Agacharse")]
    [SerializeField] private float crouchHeight = 1.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector3 horizontalVelocity;

    private float verticalVelocity;

    private bool isAiming;
    private bool isSprinting;
    private bool isCrouching;
    private bool isDashing;

    private Vector3 dashDirection;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;

    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 crouchingCenter;

    private readonly Collider[] overlapResults = new Collider[10];

    public bool IsAiming => isAiming;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsDashing => isDashing;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        standingHeight = controller.height;
        standingCenter = controller.center;

        crouchingCenter = standingCenter;

        crouchingCenter.y = standingCenter.y - (standingHeight - crouchHeight) * 0.5f;
    }

    private void OnEnable()
    {
        if (moveAction?.action != null)
            moveAction.action.Enable();

        if (aimAction?.action != null)
            aimAction.action.Enable();

        if (sprintAction?.action != null)
            sprintAction.action.Enable();

        if (crouchAction?.action != null)
            crouchAction.action.Enable();

        if (dashAction?.action != null)
            dashAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction?.action != null)
            moveAction.action.Disable();

        if (aimAction?.action != null)
            aimAction.action.Disable();

        if (sprintAction?.action != null)
            sprintAction.action.Disable();

        if (crouchAction?.action != null)
            crouchAction.action.Disable();

        if (dashAction?.action != null)
            dashAction.action.Disable();
    }

    private void Update()
    {
        ReadInput();

        CalculateMoveDirection();

        HandleCrouch();

        HandleDashInput();

        HandleGravity();

        if (isDashing)
        {
            PerformDash();
        }
        else
        {
            Move();
        }

        RotatePlayer();

        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }
    }

    // --------------------------------------------------
    // INPUT
    // --------------------------------------------------

    private void ReadInput()
    {
        if (moveAction?.action != null)
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        isAiming = aimAction?.action != null && aimAction.action.IsPressed();

        bool sprintPressed = sprintAction?.action != null && sprintAction.action.IsPressed();

        isSprinting =  sprintPressed && !isAiming && !isCrouching && !isDashing && moveInput.sqrMagnitude > 0.01f;
    }

    // --------------------------------------------------
    // DIRECCIÓN RELATIVA A CÁMARA
    // --------------------------------------------------

    private void CalculateMoveDirection()
    {
        if (cameraTarget == null)
        {
            moveDirection = Vector3.zero;
            return;
        }

        Vector3 forward = cameraTarget.forward;
        Vector3 right = cameraTarget.right;

        // No queremos que mirar arriba/abajo
        // afecte al movimiento.
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        moveDirection = forward * moveInput.y + right * moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }

    // --------------------------------------------------
    // MOVIMIENTO
    // --------------------------------------------------

    private void Move()
    {
        float targetSpeed = GetCurrentSpeed();

        Vector3 targetVelocity = moveDirection * targetSpeed;

        // Da aceleración/desaceleración progresiva.
        horizontalVelocity = Vector3.MoveTowards(  horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

        Vector3 finalMovement = horizontalVelocity + Vector3.up * verticalVelocity;

        controller.Move( finalMovement * Time.deltaTime);
    }

    private float GetCurrentSpeed()
    {
        if (isCrouching)
            return crouchSpeed;

        if (isAiming)
            return aimSpeed;

        if (isSprinting)
            return sprintSpeed;

        return walkSpeed;
    }

    // --------------------------------------------------
    // GRAVEDAD
    // --------------------------------------------------

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedForce;
            }
        }
        else
        {
            verticalVelocity +=
                gravity * Time.deltaTime;
        }
    }

    // --------------------------------------------------
    // ROTACIÓN
    // --------------------------------------------------

    private void RotatePlayer()
    {
        if (cameraTarget == null)
            return;

        Vector3 directionToFace;

        if (isAiming)
        {
            // En Aim miramos siempre hacia la cámara.
            directionToFace = cameraTarget.forward;
            directionToFace.y = 0f;
        }
        else
        {
            // Fuera de Aim miramos hacia donde caminamos.
            if (moveDirection.sqrMagnitude < 0.01f)
                return;

            directionToFace = moveDirection;
        }

        RotateTowards(directionToFace);
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation( direction,Vector3.up);

        transform.rotation = Quaternion.RotateTowards( transform.rotation, targetRotation,rotationSpeed * Time.deltaTime);
    }

    // --------------------------------------------------
    // CROUCH
    // --------------------------------------------------

    private void HandleCrouch()
    {
        if (crouchAction?.action == null)
            return;

        if (!crouchAction.action.WasPressedThisFrame())
            return;

        if (isCrouching)
        {
            if (CanStandUp())
            {
                SetCrouching(false);
            }
        }
        else
        {
            SetCrouching(true);
        }
    }

    private void SetCrouching(bool crouching)
    {
        isCrouching = crouching;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            controller.center = crouchingCenter;

            isSprinting = false;
        }
        else
        {
            controller.height = standingHeight;
            controller.center = standingCenter;
        }
    }

    private bool CanStandUp()
    {
        Vector3 worldCenter = transform.TransformPoint(standingCenter);

        float radius = controller.radius * Mathf.Max( transform.lossyScale.x, transform.lossyScale.z);

        float height = standingHeight * transform.lossyScale.y;

        float halfSegment = Mathf.Max( 0f,height * 0.5f - radius);

        Vector3 bottom = worldCenter - Vector3.up * halfSegment;

        Vector3 top = worldCenter + Vector3.up * halfSegment;

        int hits = Physics.OverlapCapsuleNonAlloc(bottom,top,radius * 0.95f, overlapResults, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits; i++)
        {
            Collider hit = overlapResults[i];

            if (hit == null)
                continue;

            if (hit == controller)
                continue;

            if (hit.transform.IsChildOf(transform))
                continue;

            return false;
        }

        return true;
    }

    // --------------------------------------------------
    // DASH
    // --------------------------------------------------

    private void HandleDashInput()
    {
        if (dashAction?.action == null)
            return;

        if (!dashAction.action.WasPressedThisFrame())
            return;

        TryStartDash();
    }

    private void TryStartDash()
    {
        if (isDashing)
            return;

        if (dashCooldownRemaining > 0f)
            return;

        if (isCrouching)
            return;

        if (cameraTarget == null)
            return;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            dashDirection = moveDirection.normalized;
        }
        else
        {
            dashDirection = cameraTarget.forward;

            dashDirection.y = 0f;
            dashDirection.Normalize();
        }

        isDashing = true;

        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;

        horizontalVelocity = Vector3.zero;
    }

    private void PerformDash()
    {
        Vector3 dashMovement =
            dashDirection * dashSpeed;

        dashMovement.y = verticalVelocity;

        controller.Move(dashMovement * Time.deltaTime);

        dashTimeRemaining -= Time.deltaTime;

        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;
        }
    }
}