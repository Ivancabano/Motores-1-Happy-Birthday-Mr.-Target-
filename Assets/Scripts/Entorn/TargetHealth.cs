using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private float normalHealth = 25f;
    [SerializeField] private float alertHealth = 75f;

    private float maxHealth;
    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        // Empieza fuera de alerta
        maxHealth = normalHealth;
        currentHealth = normalHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth,0f,maxHealth);

        Debug.Log(gameObject.name + " recibió " + damage + " de daño. Vida restante: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void SetAlertState(bool alertActive)
    {
        if (alertActive)
        {
            maxHealth = alertHealth;
            currentHealth = alertHealth;
        }
        else
        {
            maxHealth = normalHealth;currentHealth = Mathf.Min(currentHealth,maxHealth);
        }

        Debug.Log(gameObject.name +" cambió su vida a " +currentHealth + "/" + maxHealth);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " murió.");

        Destroy(gameObject);
    }
}