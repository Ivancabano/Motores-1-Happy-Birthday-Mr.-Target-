using UnityEngine;

public class EnemyBullet : MonoBehaviour
 
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb= GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private  void OnCollisionEnter(Collision collision) 
    {
        IDamageable damageable = collision.gameObject.GetComponent <IDamageable> ();
        {
            damageable.TakeDamage(damage);
        }

         Destroy(gameObject);
         return;
    }
}
