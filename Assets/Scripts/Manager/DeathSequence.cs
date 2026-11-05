using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class DeathSequence : MonoBehaviour
{
    [Header("Animacion")]
    [SerializeField] private Animator animator;

    [Header("Final de la muerte")]
    [Tooltip("Activar en enemigos normales. Desactivar en protagonista y jefe.")]
    [SerializeField] private bool destroyOnFinish = true;

    [Header("Otros scripts que deban detenerse")]
    [Tooltip("Opcional: otros comportamientos de juego. No agregar scripts de animacion ni de salud.")]
    [SerializeField] private MonoBehaviour[] additionalBehaviours;

    private PlayerHealth playerHealth;
    private TargetHealth targetHealth;

    public bool IsDying { get; private set; }
    public bool IsFinished { get; private set; }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        targetHealth = GetComponent<TargetHealth>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath += BeginDeath;

        if (targetHealth != null)
            targetHealth.OnDeath += BeginDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= BeginDeath;

        if (targetHealth != null)
            targetHealth.OnDeath -= BeginDeath;
    }

    private void BeginDeath()
    {
        if (IsDying)
            return;

        IsDying = true;

        
        foreach (MonoBehaviour behaviour in
                 GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is PlayerMovement ||behaviour is PlayerShooting ||behaviour is PlayerAim ||behaviour is EnemyBase ||behaviour is EnemyBossBase ||behaviour is EnemyContactDamage ||behaviour is RangedEnemyAttack ||behaviour is BossShotgunAttack)
            {
                StopBehaviour(behaviour);
            }
        }

        if (additionalBehaviours != null)
        {
            foreach (MonoBehaviour behaviour in additionalBehaviours)
                StopBehaviour(behaviour);
        }

       
        foreach (NavMeshAgent agent in GetComponentsInChildren<NavMeshAgent>(true))
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            agent.enabled = false;
        }

        
        foreach (Rigidbody body in
                 GetComponentsInChildren<Rigidbody>(true))
        {
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = true;
        }

      
        foreach (Collider col in
                 GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

      
        foreach (AudioSource source in
                 GetComponentsInChildren<AudioSource>(true))
        {
            source.Stop();
        }

        // NO desactivar el Animator ni sus controladores.
        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
        else
        {
            Debug.LogError("Falta asignar el Animator de muerte.", this);
        }
    }

    private void StopBehaviour(MonoBehaviour behaviour)
    {
        if (behaviour == null ||behaviour == this || behaviour is PlayerHealth ||behaviour is TargetHealth ||behaviour is PlayerAnimationController ||behaviour is EnemyAnimationController ||behaviour is BossAnimationController)
        {
            return;
        }

        
        behaviour.StopAllCoroutines();
        behaviour.CancelInvoke();
        behaviour.enabled = false;
    }

    public void FinishDeath()
    {
        if (!IsDying || IsFinished)
            return;

        IsFinished = true;
        Debug.Log(name + ": animacion de muerte terminada.", this);

        if (destroyOnFinish)
            Destroy(gameObject);
    }
}
