using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private Transform muzzle;

    [Header("Bala")]
    [SerializeField] private Bullet bulletPrefab;

    [Header("Input")]
    [SerializeField] private InputActionReference shootAction;

    [Header("Arma")]
    [SerializeField] private float fireCooldown = 0.2f;

    private float nextFireTime;

    private void OnEnable()
    {
        if (shootAction?.action != null)
        {
            shootAction.action.Enable();
            shootAction.action.performed += OnShoot;
        }
    }

    private void OnDisable()
    {
        if (shootAction?.action != null)
        {
            shootAction.action.performed -= OnShoot;
            shootAction.action.Disable();
        }
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (playerMovement == null ||  playerAim == null || muzzle == null || bulletPrefab == null)
        {
            return;
        }

        // Solo dispara mientras apuntamos
        if (!playerMovement.IsAiming)
            return;

        // Cadencia del arma
        if (Time.time < nextFireTime)
            return;

        Shoot();

        nextFireTime = Time.time + fireCooldown;
    }

    private void Shoot()
    {
        // Dirección desde la punta del arma
        // hacia donde está nuestra retícula.
        Vector3 shootDirection = (playerAim.AimPoint - muzzle.position).normalized;

        // Creamos la bala físicamente en el Muzzle.
        Bullet newBullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(shootDirection));

        // Le indicamos hacia dónde viajar.
        newBullet.Initialize(shootDirection, gameObject);
    }
}