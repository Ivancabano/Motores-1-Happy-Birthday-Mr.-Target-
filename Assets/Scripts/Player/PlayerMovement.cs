using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
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
    [SerializeField] private float acceleration = 25f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Agacharse")]
    [SerializeField] private float crouchHeight = 1.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    private bool isAiming;
    private bool isSprinting;
    private bool isCrouching;
    private bool isDashing;

    private float dashTimeRemaining;
    private float dashCooldownRemaining;

    private Vector3 dashDirection;

    // Valores originales del collider
    private float standingHeight;
    private Vector3 standingCenter;
    private Vector3 crouchingCenter;

    private readonly Collider[] standCheckResults = new Collider[10];

    // Propiedades públicas
    public bool IsAiming => isAiming;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        standingHeight = capsule.height;
        standingCenter = capsule.center;

        crouchingCenter = standingCenter;

        // Hace que la parte inferior del collider
        // permanezca en el mismo sitio al agacharse.
        crouchingCenter.y =
            standingCenter.y -
            (standingHeight - crouchHeight) * 0.5f;
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

        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            PerformDash();
            return;
        }

        Move();
        RotatePlayer();
    }

    // --------------------------------------------------
    // INPUT
    // --------------------------------------------------

    private void ReadInput()
    {
        if (moveAction?.action != null)
        {
            moveInput =
                moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            moveInput = Vector2.zero;
        }

        isAiming =
            aimAction?.action != null &&
            aimAction.action.IsPressed();

        bool sprintButton =
            sprintAction?.action != null &&
            sprintAction.action.IsPressed();

        // No corremos apuntando ni agachados.
        isSprinting =
            sprintButton &&
            !isAiming &&
            !isCrouching &&
            moveInput.sqrMagnitude > 0.01f;
    }

    // --------------------------------------------------
    // DIRECCIÓN
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

        // Movimiento únicamente sobre X/Z.
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        moveDirection =
            forward * moveInput.y +
            right * moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }
    }

    // --------------------------------------------------
    // MOVIMIENTO NORMAL
    // --------------------------------------------------

    private void Move()
    {
        float currentSpeed = GetCurrentSpeed();

        Vector3 desiredVelocity =
            moveDirection * currentSpeed;

        Vector3 currentVelocity =
            rb.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

        Vector3 velocityChange =
            desiredVelocity - horizontalVelocity;

        velocityChange =
            Vector3.ClampMagnitude(
                velocityChange,
                acceleration * Time.fixedDeltaTime
            );

        rb.AddForce(
            velocityChange,
            ForceMode.VelocityChange
        );
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
    // ROTACIÓN
    // --------------------------------------------------

    private void RotatePlayer()
    {
        if (cameraTarget == null)
            return;

        Vector3 directionToFace;

        if (isAiming)
        {
            // Al apuntar siempre mira hacia la cámara.
            directionToFace = cameraTarget.forward;
            directionToFace.y = 0f;
        }
        else
        {
            // Normalmente mira hacia donde camina.
            if (moveDirection.sqrMagnitude < 0.01f)
                return;

            directionToFace = moveDirection;
        }

        RotateTowards(directionToFace);
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.y = 0f;
        direction.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        Quaternion newRotation =
            Quaternion.RotateTowards(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(newRotation);
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
            // Solo se levanta si hay espacio arriba.
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
            capsule.height = crouchHeight;
            capsule.center = crouchingCenter;

            isSprinting = false;
        }
        else
        {
            capsule.height = standingHeight;
            capsule.center = standingCenter;
        }
    }

    private bool CanStandUp()
    {
        Vector3 worldCenter =
            transform.TransformPoint(standingCenter);

        float radius =
            capsule.radius *
            Mathf.Max(
                transform.lossyScale.x,
                transform.lossyScale.z
            );

        float worldHeight =
            standingHeight *
            transform.lossyScale.y;

        float halfHeight =
            Mathf.Max(
                worldHeight * 0.5f,
                radius
            );

        Vector3 top =
            worldCenter +
            Vector3.up * (halfHeight - radius);

        Vector3 bottom =
            worldCenter -
            Vector3.up * (halfHeight - radius);

        int hitCount =
            Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius * 0.95f,
                standCheckResults,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = standCheckResults[i];

            if (hit == null)
                continue;

            // Ignorar nuestro propio collider.
            if (hit == capsule)
                continue;

            // Ignorar colliders hijos del jugador.
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

        // Por ahora no permitimos dash agachados.
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
            // Sin WASD, el dash va hacia adelante.
            dashDirection = cameraTarget.forward;
            dashDirection.y = 0f;
            dashDirection.Normalize();
        }

        isDashing = true;

        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;
    }

    private void PerformDash()
    {
        Vector3 currentVelocity =
            rb.linearVelocity;

        rb.linearVelocity =
            new Vector3(
                dashDirection.x * dashSpeed,
                currentVelocity.y,
                dashDirection.z * dashSpeed
            );

        RotateTowards(dashDirection);

        dashTimeRemaining -= Time.fixedDeltaTime;

        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;
        }
    }
}