using UnityEngine;
using System;

public class BossShotgunAttack : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint; // Asignado directamente desde el Inspector
    private EnemyBossBase boss;
    private Transform target;

    [Header("Escopeta - Configuración")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float pelletDamage = 4f;
    [SerializeField] private float spreadAngle = 12f;
    [SerializeField] private float bulletSpeed = 25f;

    public event Action OnShoot;
    private float nextFireTime;

    private void Start()
    {
        SearchReferences();
    }

    private void SearchReferences()
    {
        // 1. Buscamos el componente del jefe en este mismo GameObject
        if (boss == null)
        {
            boss = GetComponent<EnemyBossBase>();
            if (boss == null)
            {
                Debug.LogError($"[BossShotgunAttack] No se encontró el componente EnemyBossBase en {gameObject.name}");
            }
        }

        // 2. Buscamos al jugador en la escena de forma automática
        if (target == null)
        {
            PlayerDetectorTarget player = FindFirstObjectByType<PlayerDetectorTarget>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("[BossShotgunAttack] No se encontró ningún PlayerDetectorTarget en la escena.");
            }
        }

        // 3. Validamos que el FirePoint esté asignado en el Inspector
        if (firePoint != null)
        {
            Debug.Log("disparo de aca");
        }
        if (firePoint == null)
        {
            Debug.LogError($"[BossShotgunAttack] ¡Falta asignar el FirePoint en el Inspector para {gameObject.name}!");
        }
    }

    private void Update()
    {
        // Validamos que el jefe exista, esté activo en combate, y tengamos todas las referencias clave
        if (boss == null || !boss.IsCombatActive || target == null || firePoint == null || bulletPrefab == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > shootingRange)
            return;

        if (fireRate <= 0f)
            return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Shoot()
    {
        OnShoot?.Invoke();
        Vector3 aimPoint = target.position + Vector3.up * 1f;

        Vector3 direction = (aimPoint - firePoint.position).normalized;
        Quaternion centerRotation = Quaternion.LookRotation(direction);
        
        for (int i = 0; i < pelletCount; i++)
        {
            Vector2 spread = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
            Vector3 localDirection = new Vector3(spread.x, spread.y, 1f);
            Vector3 pelletDirection = (centerRotation * localDirection).normalized;

            GameObject pellet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(pelletDirection));

            EnemyBullet bullet = pellet.GetComponent<EnemyBullet>();
            if (bullet != null)
                bullet.SetDamage(pelletDamage);

            Rigidbody rb = pellet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = pelletDirection * bulletSpeed;
            }
        }

        Debug.Log("El jefe disparó su escopeta.");
    }
}