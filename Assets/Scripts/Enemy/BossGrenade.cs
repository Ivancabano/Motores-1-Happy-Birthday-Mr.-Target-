using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossGrenade : MonoBehaviour
{
    [Header("Aviso en el piso")]
    [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.1f, 0.9f);
    [SerializeField] private float ringWidth = 0.08f;
    [SerializeField] private int ringSegments = 48;
    [Tooltip("Segundos antes de explotar en los que el circulo empieza a titilar.")]
    [SerializeField] private float blinkStartBeforeExplosion = 1.2f;
    [SerializeField] private float slowBlinkInterval = 0.25f;
    [SerializeField] private float fastBlinkInterval = 0.05f;

    [Header("Explosion")]
    [Tooltip("Opcional: efecto de particulas de la explosion. Si esta vacio se usa un destello simple.")]
    [SerializeField] private GameObject explosionPrefab;
    [Tooltip("Opcional: sonido de la explosion.")]
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private Color flashColor = new Color(1f, 0.55f, 0.15f, 0.8f);
    [SerializeField] private float flashDuration = 0.35f;

    private Rigidbody rb;
    private LineRenderer ring;
    private float fuseTime;
    private float radius;
    private float maxDamage;
    private float minDamage;
    private LayerMask lineOfSightMask;
    private float timer;
    private float blinkTimer;
    private bool blinkOn = true;
    private bool armed;

    public static BossGrenade CreateDefault(Vector3 position)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Granada Jefe";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.25f;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = new Color(0.15f, 0.22f, 0.12f);

        go.AddComponent<Rigidbody>();
        return go.AddComponent<BossGrenade>();
    }

    public void Launch(Vector3 velocity, Collider[] ignoreColliders, float fuse, float explosionRadius,
                       float damageAtCenter, float damageAtEdge, LayerMask losMask)
    {
        fuseTime = fuse;
        radius = explosionRadius;
        maxDamage = damageAtCenter;
        minDamage = damageAtEdge;
        lineOfSightMask = losMask;

        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.angularDamping = 2f;
        rb.linearVelocity = velocity;

        Collider own = GetComponent<Collider>();
        if (own != null && ignoreColliders != null)
        {
            foreach (Collider other in ignoreColliders)
            {
                if (other != null)
                    Physics.IgnoreCollision(own, other);
            }
        }

        CreateRing();
        armed = true;
    }

    private void Update()
    {
        if (!armed)
            return;

        timer += Time.deltaTime;
        UpdateRing();

        if (timer >= fuseTime)
            Explode();
    }

    private void CreateRing()
    {
        GameObject ringObject = new GameObject("Aviso Granada");
        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = ringSegments;
        ring.startWidth = ringWidth;
        ring.endWidth = ringWidth;
        ring.startColor = warningColor;
        ring.endColor = warningColor;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
    }

    private void UpdateRing()
    {
        if (ring == null)
            return;

        Vector3 center = transform.position;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 30f, lineOfSightMask, QueryTriggerInteraction.Ignore))
            center = hit.point;
        center += Vector3.up * 0.05f;

        for (int i = 0; i < ringSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / ringSegments;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius);
        }

        float timeLeft = fuseTime - timer;
        if (timeLeft <= blinkStartBeforeExplosion)
        {
            float t = 1f - Mathf.Clamp01(timeLeft / blinkStartBeforeExplosion);
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

        ring.enabled = blinkOn;
    }

    private void Explode()
    {
        armed = false;
        Vector3 center = transform.position;

        DamagePlayer(center);

        if (explosionClip != null)
            AudioSource.PlayClipAtPoint(explosionClip, center);

        if (ring != null)
            Destroy(ring.gameObject);

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        Collider own = GetComponent<Collider>();
        if (own != null)
            own.enabled = false;

        rb.isKinematic = true;

        if (explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, center, Quaternion.identity);
            Destroy(effect, 5f);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(FlashAndDestroy(center));
        }
    }

    private void DamagePlayer(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, ~0, QueryTriggerInteraction.Ignore);
        HashSet<ITakeDamage> damaged = new HashSet<ITakeDamage>();

        foreach (Collider hit in hits)
        {
            PlayerDetectorTarget player = hit.GetComponentInParent<PlayerDetectorTarget>();
            if (player == null)
                continue;

            ITakeDamage damageable = hit.GetComponentInParent<ITakeDamage>();
            if (damageable == null || damaged.Contains(damageable))
                continue;

            if (!HasLineOfSight(center + Vector3.up * 0.3f, player))
                continue;

            float distance = Vector3.Distance(center, hit.ClosestPoint(center));
            float damage = Mathf.Lerp(maxDamage, minDamage, Mathf.Clamp01(distance / radius));

            damageable.ITakeDamage(damage);
            damaged.Add(damageable);
        }
    }

    private bool HasLineOfSight(Vector3 origin, PlayerDetectorTarget player)
    {
        Vector3 target = player.GetCenterPosition();
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction.normalized, distance, lineOfSightMask, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.collider.GetComponentInParent<PlayerDetectorTarget>() != player)
                return false;
        }

        return true;
    }

    private IEnumerator FlashAndDestroy(Vector3 center)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "Explosion Granada";
        flash.transform.position = center;
        DestroyImmediate(flash.GetComponent<Collider>());

        Renderer flashRenderer = flash.GetComponent<Renderer>();
        flashRenderer.material = new Material(Shader.Find("Sprites/Default"));
        flashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Light light = flash.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = flashColor;
        light.range = radius * 2.5f;

        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / flashDuration);

            flash.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, radius * 2f, p);

            Color c = flashColor;
            c.a = flashColor.a * (1f - p);
            flashRenderer.material.color = c;

            light.intensity = Mathf.Lerp(8f, 0f, p);
            yield return null;
        }

        Destroy(flash);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ring != null)
            Destroy(ring.gameObject);
    }
}
