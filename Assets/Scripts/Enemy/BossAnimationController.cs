using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossAnimationController : MonoBehaviour
{
    [Header("Referencias (Autogestionadas)")]
    private EnemyBossBase bossBase;
    private NavMeshAgent agent;
    private BossSniperAttack shotgun;
    private TargetHealth targetHealth;

    [Header("Configuración de Animación")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;
    private bool isDead = false;

    // Hashes del Animator para optimizar rendimiento
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int Die = Animator.StringToHash("Die");
    private static readonly int Crouched = Animator.StringToHash("Crouched");
    private static readonly int Aiming = Animator.StringToHash("Aiming");
    private static readonly int ThrowGrenade = Animator.StringToHash("ThrowGrenade");

    private BossGrenadeAttack grenadeAttack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        SearchReferences();
    }

    private void SearchReferences()
    {
        // Busca en el mismo GameObject o en los padres (por si el script está en el objeto de la malla 3D)
        if (bossBase == null)
            bossBase = GetComponentInParent<EnemyBossBase>();

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();

        if (shotgun == null)
            shotgun = GetComponentInParent<BossSniperAttack>();

        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();

        if (grenadeAttack == null)
            grenadeAttack = GetComponentInParent<BossGrenadeAttack>();
    }

    private void OnEnable()
    {
        if (shotgun != null)
        {
            shotgun.OnShoot += HandleShoot;
            shotgun.OnCrouchedChanged += HandleCrouched;
            shotgun.OnAimingChanged += HandleAiming;
        }

        if (grenadeAttack != null)
            grenadeAttack.OnThrowStarted += HandleThrowGrenade;

        if (targetHealth != null)
            targetHealth.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (shotgun != null)
        {
            shotgun.OnShoot -= HandleShoot;
            shotgun.OnCrouchedChanged -= HandleCrouched;
            shotgun.OnAimingChanged -= HandleAiming;
        }

        if (grenadeAttack != null)
            grenadeAttack.OnThrowStarted -= HandleThrowGrenade;

        if (targetHealth != null)
            targetHealth.OnDeath -= HandleDeath;
    }

    private void HandleThrowGrenade()
    {
        if (isDead)
            return;

        animator.SetTrigger(ThrowGrenade);
    }

    private void HandleCrouched(bool crouched)
    {
        if (isDead)
            return;

        animator.SetBool(Crouched, crouched);
    }

    private void HandleAiming(bool aiming)
    {
        if (isDead)
            return;

        animator.SetBool(Aiming, aiming);
    }

    private void Update()
    {
        if (isDead)
            return;

        float speed = 0f;

        // Validamos utilizando la propiedad IsCombatActive de EnemyBossBase
        if (bossBase != null && bossBase.IsCombatActive && agent != null && agent.isOnNavMesh)
        {
            speed = agent.velocity.magnitude / Mathf.Max(agent.speed, 0.01f);
        }

        float currentSmooth = (speed == 0f) ? animationSmooth * 0.5f : animationSmooth;
        animator.SetFloat(Speed, Mathf.Clamp01(speed), currentSmooth, Time.deltaTime);
    }

    private void HandleShoot()
    {
        if (isDead)
            return;

        animator.SetTrigger(Shoot);
    }

    private void HandleDeath()
    {
        if (isDead)
            return;

        isDead = true;

        animator.SetFloat(Speed, 0f);
        animator.ResetTrigger(Shoot);
        animator.SetTrigger(Die);

        Debug.Log("Animación de muerte del jefe activada.");
    }
    public void OnDeathAnimationFinished()
{
    DeathSequence sequence = GetComponentInParent<DeathSequence>();

    if (sequence == null)
    {
        Debug.LogError("No se encontro DeathSequence en el personaje.", this );
        return;
    }

    sequence.FinishDeath();
}

}