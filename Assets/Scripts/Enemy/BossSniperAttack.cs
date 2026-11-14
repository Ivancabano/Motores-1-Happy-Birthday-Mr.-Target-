using System.Collections;
using UnityEngine;

public class BossSniperAttack : MonoBehaviour
{
    public event System.Action OnShoot;

    [Header("Referencias")]
    [SerializeField] private Transform firePoint;               
    [SerializeField] private Transform bodyRoot;                  
    [SerializeField] private LayerMask lineOfSightMask = ~0;      
    [SerializeField] private Material laserMaterial;              

    [Header("Ciclo de combate (segundos)")]
    [SerializeField] private float reloadDuration = 3f;           
    [SerializeField] private float crouchDuration = 0.4f;         
    [SerializeField] private float standUpDuration = 0.5f;        
    [SerializeField] private float aimDuration = 2.5f;            
    [SerializeField] private float warningDuration = 0.8f;        
    [SerializeField] private float afterShotDuration = 0.4f;      

    [Header("Danio")]
    [SerializeField] private float sniperDamage = 40f;
    [SerializeField] private float maxRange = 100f;

    [Header("Linea de mira")]
    [SerializeField] private Color aimColor = Color.red;
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private float slowBlinkInterval = 0.25f;     
    [SerializeField] private float fastBlinkInterval = 0.05f;     

    [Header("Postura")]
    [SerializeField] private float crouchDrop = 1f;               
    [SerializeField] private float rotationSpeed = 6f;

    private bool activated;
    private TargetHealth health;
    private PlayerDetectorTarget player;
    private LineRenderer laser;

    private float standY;
    private float crouchY;
    private bool faceTarget;

    
    public bool IsActive => activated && health != null && !health.IsDead;

    private bool IsReady => IsActive && player != null;

    
    public void Activate()
    {
        if (activated)
            return;

        activated = true;

        
        bool wasDetected = RankManager.Instance != null && RankManager.Instance.WasDetected;

        if (health != null)
            health.SetAlertState(wasDetected);

        Debug.Log("[BossSniperAttack] Jefe activado. Jugador detectado: " + wasDetected);
    }

    private void Awake()
    {
        health = GetComponentInParent<TargetHealth>();

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

            
            faceTarget = false;
            laser.enabled = false;
            yield return MoveBody(crouchY, crouchDuration);
            yield return Wait(reloadDuration);

            if (!IsReady) continue;

            
            faceTarget = true;
            yield return MoveBody(standY, standUpDuration);

            
            yield return Aim();

            if (!IsReady) continue;

            
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

        IDamageable victim = TraceShot(origin, direction, out Vector3 endPoint);

        laser.enabled = true;
        laser.SetPosition(0, origin);
        laser.SetPosition(1, endPoint);

        if (victim != null)
            victim.TakeDamage(sniperDamage);
    }

    
    private IDamageable TraceShot(Vector3 origin, Vector3 direction, out Vector3 endPoint)
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

        
        if (best.collider.GetComponentInParent<PlayerDetectorTarget>() != null)
            return best.collider.GetComponentInParent<IDamageable>();

        return null;
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
