using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour, IAlertable
{
    protected NavMeshAgent agent;
    protected TargetHealth targetHealth;
    protected AlertManager alertManager;
    protected Transform targetTransform;
    protected IDetectable targetDetectable;

    [Header("Vision Configuration")]
    [SerializeField] protected float visionDistance = 12f;
    [Range(0f, 360f)] protected float visionAngle = 70f;
    [SerializeField] protected LayerMask obstacleLayer;
    protected float visionTolerance = 1.2f;

    [Header("Audio de Pasos (Base para todos los enemigos)")]
    [SerializeField] protected AudioSource footstepAudioSource;
    [SerializeField] protected List<AudioClip> footstepClips = new List<AudioClip>();
    [SerializeField] protected float patrolStepInterval = 0.6f;
    [SerializeField] protected float chaseStepInterval = 0.35f;
    private float stepTimer;
    private int lastClipIndex = -1;
    [Header("Audio de Muerte")]
    [SerializeField] protected AudioSource deathAudioSource;
    [SerializeField] protected AudioClip deathBodyFallClip;
    [SerializeField] protected AudioSource impactAudioSource;
    [SerializeField] protected AudioClip meleeHitClip;
    protected enum AIState { Patrolling, Chasing }
    protected AIState currentState = AIState.Patrolling;

    public event Action<bool> OnChasingChanged;

    public bool IsChasing
    {
        get => currentState == AIState.Chasing;
        internal set
        {
            AIState nuevoEstado = value ? AIState.Chasing : AIState.Patrolling;
            if (currentState != nuevoEstado)
            {
                currentState = nuevoEstado;
                Debug.Log("¡El guardia está intentando invocar el evento OnChasingChanged!");
                OnChasingChanged?.Invoke(IsChasing);
                Debug.Log("Observer notificado");
            }
        }
    }

    public float VisionDistance => visionDistance;
    public float VisionAngle => visionAngle;
    public Vector3 EyePosition => transform.position + Vector3.up * 1.5f;

    protected virtual void Start()
    {
        SearchReferences();
    }
    protected void SearchReferences()
    {
        agent = GetComponent<NavMeshAgent>();

        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponentInChildren<AudioSource>();
        }

        alertManager = UnityEngine.Object.FindFirstObjectByType<AlertManager>();

        if (targetTransform == null)
        {
            PlayerDetectorTarget playerTarget = UnityEngine.Object.FindFirstObjectByType<PlayerDetectorTarget>();
            if (playerTarget != null)
            {
                targetTransform = playerTarget.transform;
            }
        }

        if (targetTransform != null)
        {
            targetDetectable = targetTransform.GetComponent<IDetectable>();
        }
    }

    protected virtual void OnEnable()
    {
        AlertManager.Register(this);
        Debug.Log("Observer on");
    }

    protected virtual void OnDisable()
    {
        AlertManager.Unregister(this);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                if (HasDirectVision())
                {
                    if (alertManager != null) alertManager.EnemyOnSight();
                    IsChasing = true;
                    break;
                }
                HandlePatrol();
                break;

            case AIState.Chasing:
                ExecuteChase();
                break;
        }
        HandleFootsteps();
    }

    protected abstract void HandlePatrol();

    protected virtual void ExecuteChase()
    {
        if (targetTransform != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(targetTransform.position);
        }
    }

    public virtual void ForceChase()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        IsChasing = true;
    }
    private void HandleFootsteps()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.velocity.sqrMagnitude < 0.05f || agent.isStopped)
        {
            stepTimer = 0f;
            return;
        }

        float currentInterval = (currentState == AIState.Chasing) ? chaseStepInterval : patrolStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= currentInterval)
        {
            PlayRandomFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayRandomFootstep()
    {
        if (footstepAudioSource == null || footstepClips == null || footstepClips.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, footstepClips.Count);

        if (footstepClips.Count > 1 && randomIndex == lastClipIndex)
        {
            randomIndex = (randomIndex + 1) % footstepClips.Count;
        }

        lastClipIndex = randomIndex;
        AudioClip clipToPlay = footstepClips[randomIndex];

        if (clipToPlay != null)
        {
            footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            footstepAudioSource.PlayOneShot(clipToPlay);
        }
    }
    protected bool HasDirectVision()
    {
        if (targetTransform == null) return false;

        Vector3 eyePos = EyePosition;
        Vector3 targetCenter = targetDetectable != null ?
            targetDetectable.GetCenterPosition() :
            targetTransform.position + Vector3.up * 1f;

        Vector3 dir = (targetCenter - eyePos).normalized;
        float dist = Vector3.Distance(eyePos, targetCenter);

        if (dist <= visionDistance)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, dir);
            if (angleToPlayer < (visionAngle / 2f) * visionTolerance)
            {
                if (Physics.Raycast(eyePos, dir, out RaycastHit hit, visionDistance + 2f, ~obstacleLayer))
                {
                    if (hit.transform.TryGetComponent<IDetectable>(out _))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public virtual void PlayBodyFallSound()
    {
        if (deathAudioSource != null && deathBodyFallClip != null)
        {
            deathAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            deathAudioSource.PlayOneShot(deathBodyFallClip);
        }
    }
    public virtual void PlayMeleeHitSound()
    {
        if (impactAudioSource != null && meleeHitClip != null)
        {
            impactAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            impactAudioSource.PlayOneShot(meleeHitClip);
        }
    }
}