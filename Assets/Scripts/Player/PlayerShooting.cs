using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private Transform muzzle;

    [Header("Input")]
    [SerializeField] private InputActionReference shootAction;

    [Header("Disparo")]
    [SerializeField] private float shootDistance = 100f;
    [SerializeField] private LayerMask shootMask = ~0;

    private void OnEnable()
    {
        if (shootAction != null && shootAction.action != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShoot;
        }
    }

    private void OnDisable()
    {
        if (shootAction != null && shootAction.action != null)
        {
            shootAction.action.performed -= OnShoot;
            shootAction.action.Disable();
        }
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (playerMovement == null ||
            playerAim == null ||
            muzzle == null)
        {
            return;
        }

        if (!playerMovement.IsAiming)
            return;

        Shoot();
    }

    private void Shoot()
    {
        Vector3 direction =
            (playerAim.AimPoint - muzzle.position).normalized;

        Ray ray = new Ray(
            muzzle.position,
            direction
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            shootDistance,
            shootMask
        ))
        {
            Debug.Log("Impactamos contra: " + hit.collider.name);

            Debug.DrawLine(
                muzzle.position,
                hit.point,
                Color.red,
                1f
            );
        }
        else
        {
            Debug.DrawRay(
                muzzle.position,
                direction * shootDistance,
                Color.yellow,
                1f
            );
        }
    }
}