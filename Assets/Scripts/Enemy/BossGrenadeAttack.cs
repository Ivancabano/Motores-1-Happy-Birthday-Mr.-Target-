using System;
using System.Collections;
using UnityEngine;

public class BossGrenadeAttack : MonoBehaviour
{
    public event Action OnThrowStarted;

    [Header("Cuando tira")]
    [Tooltip("Solo tira si el jugador esta escondido (el jefe no lo puede ver).")]
    [SerializeField] private bool onlyWhenPlayerInCover = true;

    [Tooltip("Tira como mucho una vez cada esta cantidad de recargas.")]
    [SerializeField, Min(1)] private int throwEveryReloads = 1;

    [SerializeField] private float minThrowDistance = 4f;
    [SerializeField] private float maxThrowDistance = 30f;

    [Header("Lanzamiento")]
    [Tooltip("Opcional: desde donde sale la granada (por ejemplo, la mano). Si esta vacio, sale desde el punto de disparo.")]
    [SerializeField] private Transform throwPoint;

    [Tooltip("Segundos desde que empieza la animacion hasta que suelta la granada.")]
    [SerializeField] private float releaseDelay = 1.3f;

    [Tooltip("Duracion total del lanzamiento (lo que dura la animacion).")]
    [SerializeField] private float throwDuration = 2.3f;

    [Tooltip("Segundos de vuelo por cada 10 metros de distancia.")]
    [SerializeField] private float flightTimePer10m = 0.9f;
    [SerializeField] private float minFlightTime = 0.6f;
    [SerializeField] private float maxFlightTime = 1.6f;

    [Tooltip("Capas que bloquean la explosion (paredes, coberturas).")]
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [Header("Granada")]
    [Tooltip("Opcional: prefab con el componente BossGrenade. Si esta vacio se crea una granada simple.")]
    [SerializeField] private BossGrenade grenadePrefab;

    [Tooltip("Segundos desde que la tira hasta que explota.")]
    [SerializeField] private float fuseTime = 2.4f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float maxDamage = 35f;
    [SerializeField] private float minDamage = 10f;

    private int reloadsSinceThrow;
    private TargetHealth health;
    private Collider[] ownColliders;

    public bool IsThrowing { get; private set; }

    private bool IsDead => health != null && health.IsDead;

    private void Awake()
    {
        health = GetComponentInParent<TargetHealth>();
        ownColliders = GetComponentsInChildren<Collider>(true);

        reloadsSinceThrow = throwEveryReloads - 1;
    }

    public bool ShouldThrow(Vector3 playerPosition, bool playerVisible)
    {
        reloadsSinceThrow++;

        if (!enabled || IsDead || IsThrowing)
            return false;

        if (reloadsSinceThrow < throwEveryReloads)
            return false;

        Vector3 flat = playerPosition - transform.position;
        flat.y = 0f;
        float distance = flat.magnitude;

        if (distance < minThrowDistance || distance > maxThrowDistance)
            return false;

        if (onlyWhenPlayerInCover && playerVisible)
            return false;

        return true;
    }

    public IEnumerator ThrowRoutine(PlayerDetectorTarget player, Vector3 fallbackOrigin)
    {
        IsThrowing = true;
        reloadsSinceThrow = 0;

        OnThrowStarted?.Invoke();

        float t = 0f;
        while (t < releaseDelay && !IsDead)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (!IsDead && player != null)
            Launch(player, fallbackOrigin);

        while (t < throwDuration && !IsDead)
        {
            t += Time.deltaTime;
            yield return null;
        }

        IsThrowing = false;
    }

    private void Launch(PlayerDetectorTarget player, Vector3 fallbackOrigin)
    {
        Vector3 start = throwPoint != null ? throwPoint.position : fallbackOrigin;
        Vector3 target = player.transform.position;

        float distance = Vector3.Distance(start, target);
        float flightTime = Mathf.Clamp(distance / 10f * flightTimePer10m, minFlightTime, maxFlightTime);

        Vector3 velocity = (target - start) / flightTime - 0.5f * Physics.gravity * flightTime;

        BossGrenade grenade = grenadePrefab != null
            ? Instantiate(grenadePrefab, start, Quaternion.identity)
            : BossGrenade.CreateDefault(start);

        grenade.Launch(velocity, ownColliders, fuseTime, explosionRadius, maxDamage, minDamage, lineOfSightMask);
    }
}
