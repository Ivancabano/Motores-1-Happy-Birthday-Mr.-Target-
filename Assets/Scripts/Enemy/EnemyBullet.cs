using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public float damage = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("EnemyMelee") && !other.CompareTag("EnemyDistance"))
        {
            Destroy(gameObject);
        }
    }
}
