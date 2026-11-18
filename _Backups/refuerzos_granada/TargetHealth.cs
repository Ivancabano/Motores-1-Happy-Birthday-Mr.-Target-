using System;
using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private AlertManager alertManager;
    private Transform targetTransform;
    [Header("Muerte")]
    [Header("Vida")]
    [Header("Sistema de rangos")]
    [SerializeField] private bool countsForRank = true;
    [SerializeField] private float normalHealth = 25f;
    [SerializeField] private float alertHealth = 75f;
    
     
    private float maxHealth;
    private float currentHealth;
     public bool IsDead => isDead;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public bool IsChasing => throw new NotImplementedException();

    // Avisará cuando este enemigo muera
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        alertManager = UnityEngine.Object.FindFirstObjectByType<AlertManager>();
    }

    private void Awake()
    {
        
        maxHealth = normalHealth;
        currentHealth = normalHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log(gameObject.name + " recibió " + damage + " de daño. Vida restante: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }
        else
        {
            if (!isDead && alertManager != null)
            {
                alertManager.EnemyOnSight(0f);
            }
        }
    }
    public void TakeHeadshot()
    {
        if (isDead) return;

        // Directamente lo matamos o le restamos toda la vida necesaria
        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log(gameObject.name + " recibió un tiro en la CABEZA. Muerte instantánea.");
        Die();
        // Fíjate que aquí NO llamamos a alertManager, por lo que nunca sonará la alarma.
    }
    public void SetAlertState(bool alertActive)
    {
        if (isDead)
            return;

        if (alertActive)
        {
            maxHealth = alertHealth;
            currentHealth = alertHealth;
        }
        else
        {
            maxHealth = normalHealth;

            currentHealth = Mathf.Min(currentHealth,maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        Debug.Log( gameObject.name + " cambió su vida a " + currentHealth +"/" + maxHealth );
    }

    public void Die()
{
    if (isDead)
        return;

    isDead = true;

    Debug.Log(gameObject.name + " murio.");

    if (countsForRank)
        RankManager.Instance?.RegisterKill();

    OnDeath?.Invoke();

    
}

}