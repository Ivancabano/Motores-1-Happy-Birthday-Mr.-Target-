using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private InputActionReference lookAction;

    [Header("Altura")]
    [SerializeField] private float normalTargetHeight = 1.5f;
    [SerializeField] private float crouchTargetHeight = 1.05f;
    [SerializeField] private float heightTransitionSpeed = 10f;

    [Header("Mouse")]
    [SerializeField] private float sensitivity = 0.15f;

    [Header("Rotación vertical")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    private float yaw;
    private float pitch;

    private float currentTargetHeight;

    private void Awake()
    {
        yaw = transform.eulerAngles.y;

        currentTargetHeight = normalTargetHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (lookAction?.action != null)
            lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction?.action != null)
            lookAction.action.Disable();
    }

    private void Update()
    {
        RotateCameraTarget();
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    private void RotateCameraTarget()
    {
        if (lookAction?.action == null)
            return;

        Vector2 mouseInput = lookAction.action.ReadValue<Vector2>();

        yaw += mouseInput.x * sensitivity;
        pitch -= mouseInput.y * sensitivity;

        pitch = Mathf.Clamp(pitch,minPitch,maxPitch);

        transform.rotation =Quaternion.Euler(pitch,yaw,0f);
    }

    private void FollowPlayer()
    {
        if (player == null)
            return;

        float desiredHeight = normalTargetHeight;

        if (playerMovement != null &&
            playerMovement.IsCrouching)
        {
            desiredHeight = crouchTargetHeight;
        }

        currentTargetHeight =
            Mathf.Lerp(currentTargetHeight,desiredHeight,heightTransitionSpeed * Time.deltaTime);

        transform.position =player.position +Vector3.up * currentTargetHeight;
    }
}