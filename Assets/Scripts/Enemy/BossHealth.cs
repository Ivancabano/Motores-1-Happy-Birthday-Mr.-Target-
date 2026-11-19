using UnityEngine;

public class BossHealth : TargetHealth
{
    [Header("Audio del Jefe")]
    [SerializeField] protected AudioSource DeathAudioSource;
    [SerializeField] private AudioClip deathClip;
    protected override void Awake()
    {
        base.Awake();
        if (DeathAudioSource == null)
        {
            DeathAudioSource = GetComponent<AudioSource>() ?? GetComponentInChildren<AudioSource>();
        }

        if (DeathAudioSource != null)
        {
            DeathAudioSource.playOnAwake = false;
            DeathAudioSource.loop = false;
        }
    }
    public override void TakeHeadshot()
    {
        if (isDead) return;

        // El jefe no recibe muerte instantánea por headshot
        Debug.Log(gameObject.name + " es inmune a la muerte instantánea por headshot.");
    }

    public override void Die()
    {
        if (isDead)
            return;
        if (deathClip != null)
        {
            // Sigue sonando aunque el jefe se destruya o se desactive después
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 1f);
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": falta asignar deathClip en el Inspector.", this);
        }
        base.Die();
    }
}