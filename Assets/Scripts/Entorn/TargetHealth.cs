using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log(
            gameObject.name +
            " recibió " +
            damage +
            " de daño. Vida restante: " +
            currentHealth
        );

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " murió.");

        Destroy(gameObject);
    }
}