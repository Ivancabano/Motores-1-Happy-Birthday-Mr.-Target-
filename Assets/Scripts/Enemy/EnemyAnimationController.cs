using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyContactDamage contactDamage;
    [SerializeField] private TargetHealth targetHealth;
    [SerializeField] private EnemyBase enemyBase;

    [Header("Configuracion")]
    [SerializeField] private float animationSmooth = 0.1f;
    [SerializeField] private float bodyFallSoundDelay = 0.6f;
    [SerializeField] private float meleeHitSoundDelay = 0.2f;

    private Animator animator;
    private bool isDead = false;

    private static readonly int Speed =
        Animator.StringToHash("Speed");

    private static readonly int Attack =
        Animator.StringToHash("Attack");

    private static readonly int Die =
        Animator.StringToHash("Die");

    private static readonly int Aiming =
        Animator.StringToHash("Aiming");

   
    private RangedEnemyAttack rangedAttack;
    private bool lastAiming;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();

        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();

        if (contactDamage == null)
            contactDamage = GetComponentInParent<EnemyContactDamage>();

        if (contactDamage == null)
            contactDamage = GetComponentInChildren<EnemyContactDamage>();

        if (enemyBase == null)
            enemyBase = GetComponentInParent<EnemyBase>();

        rangedAttack = GetComponentInParent<RangedEnemyAttack>();
    }

    private void Start()
    {
        if (enemyBase is EnemyRanged)
            Debug.Log("[EnemyAnimationController] " + transform.root.name + " ataque a distancia encontrado: " + (rangedAttack != null) + ", agente: " + (agent != null), this);
    }

    private void OnEnable()
    {
        if (contactDamage != null)
            contactDamage.OnAttack += HandleAttack;

        if (targetHealth != null)
            targetHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (contactDamage != null)
            contactDamage.OnAttack -= HandleAttack;

        if (targetHealth != null)
            targetHealth.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (isDead || agent == null || !agent.isOnNavMesh)
            return;

        
        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

     
        animator.SetFloat(Speed, speed,animationSmooth,Time.deltaTime);

        
        if (rangedAttack != null)
        {
            bool aiming = rangedAttack.IsAiming;
            animator.SetBool(Aiming, aiming);

            if (aiming != lastAiming)
            {
                lastAiming = aiming;
            }
        }
    }

    private void HandleAttack()
    {
        if (isDead)
            return;

        // Reproducimos la piña
        animator.SetTrigger(Attack);
        if (enemyBase != null)
        {
            Invoke(nameof(PlayMeleeAudio), meleeHitSoundDelay);
        }
    }

    private void PlayMeleeAudio()
    {
        if (enemyBase != null && !isDead)
        {
            enemyBase.PlayMeleeHitSound();
        }
    }

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.ResetTrigger(Attack);
        animator.SetTrigger(Die);

        if (enemyBase != null)
        {
            Invoke(nameof(PlayFallAudio), bodyFallSoundDelay);
        }
    }

    private void PlayFallAudio()
    {
        if (enemyBase != null)
        {
            enemyBase.PlayBodyFallSound();
        }
    }
    public void OnDeathAnimationFinished()
{
    DeathSequence sequence = GetComponentInParent<DeathSequence>();

    if (sequence == null)
    {
        //Debug.LogError("No se encontro DeathSequence en el personaje.",this);
        return;
    }

    sequence.FinishDeath();
}

}



