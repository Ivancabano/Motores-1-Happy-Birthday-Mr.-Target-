using System;
using UnityEngine;

public class TargetHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private AlertManager alertManager;
    private Transform targetTransform;
    [Header("Muerte")]
    [Header("Vida")]
    [Header("Sistema de rangos")]
    [SerializeField] protected bool countsForRank = true;
    [SerializeField] protected float normalHealth = 25f;
    [SerializeField] protected float alertHealth = 75f;
    [SerializeField] protected AudioSource hitAudioSource;
    [SerializeField] protected AudioClip nonLethalHitClip;


    protected float maxHealth;
    protected float currentHealth;
     public bool IsDead => isDead;
    protected bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public bool IsChasing => throw new NotImplementedException();

    // Avisará cuando este enemigo muera
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged;
    protected virtual void Awake()
    {

        maxHealth = normalHealth;
        currentHealth = normalHealth;
        if (hitAudioSource == null)
        {
            hitAudioSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
        }

        if (hitAudioSource != null)
        {
            hitAudioSource.playOnAwake = false;
            hitAudioSource.loop = false;
        }
    }
    protected virtual void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        alertManager = UnityEngine.Object.FindFirstObjectByType<AlertManager>();
    }

    public virtual void TakeDamage(float damage)
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
            PlayNonLethalHitSound();
            if (!isDead && alertManager != null)
            {
                alertManager.EnemyOnSight(0f);
            }
        }
    }
    protected virtual void PlayNonLethalHitSound()
    {
        hitAudioSource.PlayOneShot(nonLethalHitClip);
    }
    public virtual void TakeHeadshot()
    {
        if (isDead) return;

        // Directamente lo matamos o le restamos toda la vida necesaria
        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log(gameObject.name + " recibió un tiro en la CABEZA. Muerte instantánea.");
        Die();
        // Fíjate que aquí NO llamamos a alertManager, por lo que nunca sonará la alarma.
    }
    public void SetCountsForRank(bool value)
    {
        countsForRank = value;
    }

    public virtual void SetAlertState(bool alertActive)
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

    public virtual void Die()
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