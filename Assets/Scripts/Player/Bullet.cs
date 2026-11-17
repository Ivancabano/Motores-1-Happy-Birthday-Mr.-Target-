using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bala")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float damage = 10f;
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
        // 1. ¿Es la cabeza?
        if (collision.gameObject.TryGetComponent<IHeadshot>(out IHeadshot headshot))
        {
            headshot.OnHeadshot(); // Llama exclusivamente a la cabeza (TakeHeadshot)
            Destroy(gameObject);   // Destruimos la bala para que no siga
            return;                // <--- AQUÍ CORTAMOS. NUNCA DEBE LLEGAR A TakeDamage.
        }

        // 2. ¿Es el cuerpo u otro componente con vida normal?
        if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage); // Aquí sí va el TakeDamage normal que activa la alarma
            Destroy(gameObject);   // Destruimos la bala
            return;
        }
    }

}