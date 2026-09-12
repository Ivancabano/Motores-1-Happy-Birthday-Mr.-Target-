using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Bala")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody rb;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, GameObject bulletOwner)
    {
        owner = bulletOwner;

        IgnoreOwnerCollisions();

        rb.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, lifeTime);
    }

    private void IgnoreOwnerCollisions()
    {
        Collider bulletCollider = GetComponent<Collider>();

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        foreach (Collider ownerCollider in ownerColliders)
        {
            Physics.IgnoreCollision(bulletCollider,ownerCollider);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}