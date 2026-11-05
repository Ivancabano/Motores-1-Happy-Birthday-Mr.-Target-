
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Asegurarnos de que la bala no sea afectada por la gravedad si quieres que vayan completamente rectas
        if (rb != null)
        {
            rb.useGravity = false;
        }

        Destroy(gameObject, lifeTime);

        // EVITA QUE LAS BALAS CHOQUEN ENTRE SÍ:
        // Ignoramos la colisión con cualquier otra bala que ya esté en la escena al nacer
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            EnemyBullet[] allBullets = FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
            foreach (EnemyBullet otherBullet in allBullets)
            {
                if (otherBullet != this)
                {
                    Collider otherCollider = otherBullet.GetComponent<Collider>();
                    if (otherCollider != null)
                    {
                        Physics.IgnoreCollision(myCollider, otherCollider);
                    }
                }
            }
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Si choca contra el propio jefe, ignoramos la colisión para que no explote al salir
        if (collision.gameObject.GetComponentInParent<EnemyBossBase>() != null)
        {
            return;
        }

        // 2. Buscamos si lo que tocamos tiene la interfaz de daño (el jugador)
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            // Si es el jugador, aplicamos daño
            damageable.TakeDamage(damage);
        }

        // 3. Al ser una colisión física normal, tanto si choca con el jugador como con una pared o suelo, se destruye
        Destroy(gameObject);
    }
}