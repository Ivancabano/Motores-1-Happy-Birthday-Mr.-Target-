
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Configuracion")]
    [SerializeField] private float animationSmooth = 0.1f;

    private Animator animator;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private static readonly int Die = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        animator.SetFloat(Speed,speed,animationSmooth,Time.deltaTime);
    }
}
