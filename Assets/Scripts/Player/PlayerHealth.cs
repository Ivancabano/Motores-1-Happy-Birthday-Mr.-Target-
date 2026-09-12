using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    // Nos servirán después para la barra de vida y muerte.
    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        if (damage <= 0f)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth,0f,maxHealth);

        Debug.Log("Player recibió " +damage +" de daño. Vida: " +currentHealth + "/" + maxHealth);

        OnHealthChanged?.Invoke(currentHealth,maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        if (amount <= 0f)
            return;

        currentHealth += amount;

        currentHealth = Mathf.Clamp(currentHealth,0f,maxHealth);

        OnHealthChanged?.Invoke( currentHealth,maxHealth);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("El jugador murió.");

        OnDeath?.Invoke();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke( currentHealth,maxHealth);
    }
}