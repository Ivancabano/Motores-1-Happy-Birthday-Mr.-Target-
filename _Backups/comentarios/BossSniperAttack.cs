using System.Collections;
using UnityEngine;

// Ciclo del jefe francotirador:
// agachado y recargando -> se para -> apunta con linea roja (titila al final) -> dispara -> vuelve a agacharse.
// Reemplaza a BossShotgunAttack para este jefe.
public class BossSniperAttack : MonoBehaviour
{
    public event System.Action OnShoot;

    // Avisan al Animator cuando cambia la postura o el apuntado (true = activo)
    public event System.Action<bool> OnCrouchedChanged;
    public event System.Action<bool> OnAimingChanged;

    [Header("Referencias")]
    [SerializeField] private Transform firePoint;                 // Punto desde donde sale la linea y el disparo
    [SerializeField] private Transform bodyRoot;                  // Modelo que baja al agacharse (opcional)
    [SerializeField] private LayerMask lineOfSightMask = ~0;      // Capas que bloquean la mira (escritorio, paredes)
    [SerializeField] private Material laserMaterial;              // Opcional: si esta vacio se crea uno basico
    [SerializeField] private BossGrenadeAttack grenadeAttack;     // Opcional: granada durante la recarga (se busca sola)

    [Header("Ciclo de combate (segundos)")]
    [SerializeField] private float reloadDuration = 3f;           // Agachado recargando
    [SerializeField] private float crouchDuration = 0.4f;         // Tiempo que tarda en agacharse
    [SerializeField] private float standUpDuration = 0.5f;        // Tiempo que tarda en pararse
    [SerializeField] private float aimDuration = 2.5f;            // Tiempo apuntando antes de disparar
    [SerializeField] private float warningDuration = 0.8f;        // Ultimo tramo del apuntado donde la linea titila
    [SerializeField] private float afterShotDuration = 0.4f;      // Pausa de pie despues del disparo
    [SerializeField] private float grenadeDelayAfterCrouch = 0.5f; // Espera agachado antes de tirar la granada

    [Header("Danio")]
    [SerializeField] private float sniperDamage = 40f;
    [SerializeField] private float maxRange = 100f;

    [Header("Linea de mira")]
    [SerializeField] private Color aimColor = Color.red;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private float slowBlinkInterval = 0.25f;     // Titileo al empezar la alerta
    [SerializeField] private float fastBlinkInterval = 0.05f;     // Titileo rapido justo antes del disparo

    [Header("Postura")]
    [SerializeField] private float crouchDrop = 1f;               // Cuanto baja el cuerpo al agacharse
    [SerializeField] private float rotationSpeed = 6f;

    [Header("Activacion")]
    [Tooltip("Si el jugador se acerca a esta distancia, el jefe se activa aunque no haya pasado por el trigger. 0 = desactivado.")]
    [SerializeField] private float autoActivateDistance = 20f;

    private bool activated;
    private TargetHealth health;
    private PlayerDetectorTarget player;
    private LineRenderer laser;

    private float standY;
    private float crouchY;
    private bool faceTarget;

    // Se activa solo cuando el jugador entra a la sala (ver BossRoomTrigger)
    public bool IsActive => activated && health != null && !health.IsDead;

    private bool IsReady => IsActive && player != null;

    // Llamado por BossRoomTrigger cuando el jugador entra a la sala
    public void Activate()
    {
        if (activated)
            return;

        activated = true;

        // Misma regla que BossEnemy: la vida depende de si el jugador fue detectado
        bool wasDetected = RankManager.Instance != null && RankManager.Instance.WasDetected;

        if (health != null)
            health.SetAlertState(wasDetected);

        Debug.Log("[BossSniperAttack] Jefe activado. Jugador detectado: " + wasDetected);
    }

    private void Awake()
    {
        health = GetComponentInParent<TargetHealth>();

        if (grenadeAttack == null)
            grenadeAttack = GetComponent<BossGrenadeAttack>();

        if (bodyRoot != null)
        {
            standY = bodyRoot.localPosition.y;
            crouchY = standY - crouchDrop;
        }

        SetupLaser();
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerDetectorTarget>();

        if (firePoint == null)
            Debug.LogError("[BossSniperAttack] Falta asignar el FirePoint en " + gameObject.name);

        if (bodyRoot == null || bodyRoot == firePoint)
            Debug.LogWarning("[BossSniperAttack] Body Root no asignado a un modelo separado: el jefe no se vera agacharse.");

        StartCoroutine(CombatLoop());
    }

    private void Update()
    {
        // Respaldo: si el trigger de la sala no lo activo, se activa al acercarse el jugador
        if (!activated && autoActivateDistance > 0f && player != null &&
            Vector3.Distance(transform.position, player.transform.position) <= autoActivateDistance)
        {
            Activate();
        }

        if (!IsReady || !faceTarget)
            return;

        Vector3 flat = player.GetCenterPosition() - transform.position;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator CombatLoop()
    {
        while (true)
        {
            if (!IsReady || firePoint == null)
            {
                laser.enabled = false;
                yield return null;
                continue;
            }

            // 1. Se agacha a recargar
            //    (antes de agacharse mira si el jugador esta a la vista, desde el punto de disparo)
            Debug.Log("[BossSniperAttack] Fase: agacharse y recargar");
            bool playerVisible = CanSeePlayer();
            Vector3 standingFirePoint = firePoint.position;

            faceTarget = false;
            laser.enabled = false;
            OnAimingChanged?.Invoke(false);
            OnCrouchedChanged?.Invoke(true);
            yield return MoveBody(crouchY, crouchDuration);

            float reloadStart = Time.time;

            // 2. Agachado: si el jugador esta cubierto, le tira una granada
            if (grenadeAttack != null && grenadeAttack.ShouldThrow(player.transform.position, playerVisible))
            {
                // Deja terminar la animacion de agacharse antes de lanzar
                yield return Wait(grenadeDelayAfterCrouch);

                Debug.Log("[BossSniperAttack] Fase: tirar granada");
                faceTarget = true;
                yield return grenadeAttack.ThrowRoutine(player, standingFirePoint);
                faceTarget = false;
            }

            // Termina de recargar (la granada cuenta dentro del tiempo de recarga)
            yield return Wait(Mathf.Max(0f, reloadDuration - (Time.time - reloadStart)));

            if (!IsReady) continue;

            // 3. Se para
            faceTarget = true;
            OnCrouchedChanged?.Invoke(false);
            yield return MoveBody(standY, standUpDuration);

            // 4. Apunta
            Debug.Log("[BossSniperAttack] Fase: apuntar");
            OnAimingChanged?.Invoke(true);
            yield return Aim();

            if (!IsReady) continue;

            // 5. Dispara
            OnAimingChanged?.Invoke(false);
            Fire();
            yield return Wait(afterShotDuration);
        }
    }

    private IEnumerator Aim()
    {
        float elapsed = 0f;
        float blinkTimer = 0f;
        bool blinkOn = true;

        laser.enabled = true;

        while (elapsed < aimDuration && IsReady)
        {
            elapsed += Time.deltaTime;
            UpdateLaser();

            float timeLeft = aimDuration - elapsed;

            if (timeLeft <= warningDuration)
            {
                // Cuanto mas cerca del disparo, mas rapido titila
                float t = 1f - Mathf.Clamp01(timeLeft / warningDuration);
                float interval = Mathf.Lerp(slowBlinkInterval, fastBlinkInterval, t);

                blinkTimer += Time.deltaTime;
                if (blinkTimer >= interval)
                {
                    blinkTimer = 0f;
                    blinkOn = !blinkOn;
                }
            }
            else
            {
                blinkOn = true;
            }

            laser.enabled = blinkOn;
            yield return null;
        }
    }

    private void Fire()
    {
        OnShoot?.Invoke();

        Vector3 origin = firePoint.position;
        Vector3 direction = (player.GetCenterPosition() - origin).normalized;

        ITakeDamage victim = TraceShot(origin, direction, out Vector3 endPoint);

        laser.enabled = true;
        laser.SetPosition(0, origin);
        laser.SetPosition(1, endPoint);

        if (victim != null)
            victim.ITakeDamage(sniperDamage);
    }

    // Lanza un rayo y devuelve el dano del jugador si no hay nada que lo tape.
    // endPoint es donde termina la linea: el jugador, o el objeto que la bloquea.
    private ITakeDamage TraceShot(Vector3 origin, Vector3 direction, out Vector3 endPoint)
    {
        endPoint = origin + direction * maxRange;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxRange, lineOfSightMask, QueryTriggerInteraction.Ignore);

        float bestDistance = float.MaxValue;
        RaycastHit best = default;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            // Ignora al propio jefe (su cuerpo y el punto de disparo)
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                best = hit;
                found = true;
            }
        }

        if (!found)
            return null;

        endPoint = best.point;

        // Solo cuenta como impacto si lo primero que toca es el jugador
        if (best.collider.GetComponentInParent<PlayerDetectorTarget>() != null)
            return best.collider.GetComponentInParent<ITakeDamage>();

        return null;
    }

    // True si el jefe tiene al jugador a la vista (nada lo tapa)
    private bool CanSeePlayer()
    {
        if (firePoint == null || player == null)
            return false;

        Vector3 origin = firePoint.position;
        Vector3 direction = (player.GetCenterPosition() - origin).normalized;
        return TraceShot(origin, direction, out _) != null;
    }

    private void UpdateLaser()
    {
        if (firePoint == null || player == null)
            return;

        Vector3 origin = firePoint.position;
        Vector3 direction = (player.GetCenterPosition() - origin).normalized;

        TraceShot(origin, direction, out Vector3 endPoint);

        laser.SetPosition(0, origin);
        laser.SetPosition(1, endPoint);
    }

    private IEnumerator MoveBody(float targetY, float duration)
    {
        if (bodyRoot == null || duration <= 0f)
            yield break;

        float startY = bodyRoot.localPosition.y;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            Vector3 p = bodyRoot.localPosition;
            p.y = Mathf.Lerp(startY, targetY, t / duration);
            bodyRoot.localPosition = p;
            yield return null;
        }

        Vector3 end = bodyRoot.localPosition;
        end.y = targetY;
        bodyRoot.localPosition = end;
    }

    // Espera, pero corta si el jefe deja de estar en combate
    private IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds && IsReady)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void SetupLaser()
    {
        laser = GetComponent<LineRenderer>();
        if (laser == null)
            laser = gameObject.AddComponent<LineRenderer>();

        laser.useWorldSpace = true;
        laser.positionCount = 2;
        laser.startWidth = lineWidth;
        laser.endWidth = lineWidth;
        laser.startColor = aimColor;
        laser.endColor = aimColor;
        laser.material = laserMaterial != null ? laserMaterial : new Material(Shader.Find("Sprites/Default"));
        laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        laser.receiveShadows = false;
        laser.enabled = false;
    }
}
