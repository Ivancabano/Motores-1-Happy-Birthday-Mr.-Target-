
using UnityEngine;

public class BossShotgunAttack : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossEnemy boss;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Escopeta")]
    [SerializeField] private float shootingRange = 8f;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float pelletDamage = 4f;
    [SerializeField] private float spreadAngle = 12f;
    [SerializeField] private float bulletSpeed = 25f;

    private Transform target;
    private float nextFireTime;

    private void Start()
    {
        if (boss == null)
            boss = GetComponent<BossEnemy>();

        PlayerDetectorTarget player = FindFirstObjectByType<PlayerDetectorTarget>();

        if (player != null)
            target = player.transform;
    }

    private void Update()
    {
        if (boss == null || !boss.IsActivated || target == null || firePoint == null || bulletPrefab == null)
            return;

        float distance = Vector3.Distance(transform.position,target.position);

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
        // Apuntamos aproximadamente al torso.
        Vector3 aimPoint = target.position + Vector3.up * 1f;

        Vector3 direction = (aimPoint - firePoint.position).normalized;

        Quaternion centerRotation = Quaternion.LookRotation(direction);

        
        for (int i = 0; i < pelletCount; i++)
        {
            Vector2 spread = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);

            Vector3 localDirection = new Vector3(spread.x, spread.y, 1f);

            Vector3 pelletDirection = (centerRotation * localDirection).normalized;

            GameObject pellet = Instantiate(bulletPrefab,firePoint.position,Quaternion.LookRotation(pelletDirection)
            );

            EnemyBullet bullet = pellet.GetComponent<EnemyBullet>();

            if (bullet != null)
                bullet.SetDamage(pelletDamage);

            Rigidbody rb = pellet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity =  pelletDirection * bulletSpeed;
            }
        }

        Debug.Log("El jefe disparo su escopeta.");
    }
}

