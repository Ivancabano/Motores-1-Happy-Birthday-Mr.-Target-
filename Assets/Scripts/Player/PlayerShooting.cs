using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private PlayerAmmo playerAmmo;
    [SerializeField] private Transform muzzle;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    [Header("Bala")]
    [SerializeField] private Bullet bulletPrefab;

    [Header("Input")]
    [SerializeField] private InputActionReference shootAction;

    [Header("Arma")]
    [SerializeField] private float fireCooldown = 0.2f;

    private float nextFireTime;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        if (playerAmmo == null)
            playerAmmo = GetComponent<PlayerAmmo>();
    }

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
        if (playerMovement == null || playerAim == null || muzzle == null || bulletPrefab == null)
            return;

        if (!playerMovement.IsAiming)
            return;

        if (Time.time < nextFireTime)
            return;

        if (playerAmmo != null && !playerAmmo.TryConsume())
            return;

        Shoot();

        nextFireTime = Time.time + fireCooldown;
    }

    private void Shoot()
    {
        Vector3 shootDirection = (playerAim.AimPoint - muzzle.position).normalized;

        Bullet newBullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(shootDirection));
        newBullet.Initialize(shootDirection, gameObject);

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);
    }
}