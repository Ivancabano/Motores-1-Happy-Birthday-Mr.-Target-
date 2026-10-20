using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyAttack : MonoBehaviour
{
    [Header("Ranged Attack")]
    [SerializeField] private float shootingRange = 10f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform target;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float rotationSpeed = 4f;
    [SerializeField] private float bulletDamage = 5f;
    private EnemyController enemyController;
    private NavMeshAgent agent;
    private float nextFireTime = 0f;

    void Start()
    {
        enemyController = GetComponent<EnemyController>();
        agent = GetComponent<NavMeshAgent>();
    }

    
    void LateUpdate()
    {
        if (enemyController == null || target == null) return;
        if (!enemyController.IsChasing) return;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= shootingRange)
        {
          
            agent.isStopped = true;
            agent.ResetPath();

            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
            }

            if (Time.time >= nextFireTime)
            {
                Shoot(target);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    private void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector3 aimPoint = target.position + Vector3.up * 1f;
        Vector3 shootDirection = (aimPoint - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));

        EnemyBullet enemyBullet = bulletObj.GetComponent<EnemyBullet>();

        if (enemyBullet != null)
        {
            enemyBullet.SetDamage(bulletDamage);
        }

        Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * bulletSpeed;
        }
    }
}
