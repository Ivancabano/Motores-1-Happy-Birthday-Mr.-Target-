using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image fillImage;    // BarraGuia
    [SerializeField] private Image trailImage;   // EstelaVida (opcional)

    [Header("Animación de la barra")]
    [SerializeField] private float smoothSpeed = 3f;

    [Header("Estela de daño")]
    [SerializeField] private float trailDelay = 0.35f;  // espera antes de bajar
    [SerializeField] private float trailSpeed = 0.5f;   // qué tan rápido baja

    [Header("Shake al recibir daño")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 8f;

    [Header("Vida baja")]
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.25f;
    [SerializeField] private Color lowHealthColor = new Color(1f, 0.55f, 0.55f, 1f);
    [SerializeField] private float pulseSpeed = 6f;

    private float target = 1f;
    private float trailTimer;
    private bool initialized;

    private RectTransform rect;
    private Vector2 originalPos;
    private Color normalColor;

    private Coroutine anim;
    private Coroutine shake;

    private void Awake()
    {
        rect = (RectTransform)transform;
        originalPos = rect.anchoredPosition;
        normalColor = fillImage != null ? fillImage.color : Color.white;
    }

    private void OnEnable() => GameEvents.OnPlayerHealthChanged += HandleHealthChanged;

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;

        if (anim != null) { StopCoroutine(anim); anim = null; }
        if (shake != null) { StopCoroutine(shake); shake = null; }

        // dejar todo como estaba
        if (rect != null) rect.anchoredPosition = originalPos;
        if (fillImage != null) fillImage.color = normalColor;
    }

    private void HandleHealthChanged(float current, float max)
    {
        float newTarget = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        bool tookDamage = initialized && newTarget < target;
        target = newTarget;

        if (!initialized)
        {
            initialized = true;
            fillImage.fillAmount = target;
            if (trailImage != null) trailImage.fillAmount = target;
        }
        else if (tookDamage)
        {
            trailTimer = trailDelay;
            if (shake != null) StopCoroutine(shake);
            shake = StartCoroutine(Shake());
        }

        if (anim == null) anim = StartCoroutine(Animate());
    }

    // Corre solo mientras haya algo que animar y se apaga sola al terminar
    private IEnumerator Animate()
    {
        while (true)
        {
            float dt = Time.unscaledDeltaTime;

            // barra principal
            fillImage.fillAmount = Mathf.Lerp(
                fillImage.fillAmount, target, 1f - Mathf.Exp(-smoothSpeed * dt));
            if (Mathf.Abs(fillImage.fillAmount - target) < 0.001f)
                fillImage.fillAmount = target;

            bool trailDone = true;
            if (trailImage != null)
            {
                if (trailTimer > 0f) trailTimer -= dt;
                else trailImage.fillAmount = Mathf.MoveTowards(
                    trailImage.fillAmount, fillImage.fillAmount, trailSpeed * dt);

                if (trailImage.fillAmount < fillImage.fillAmount)
                    trailImage.fillAmount = fillImage.fillAmount;

                trailDone = trailTimer <= 0f &&
                            Mathf.Abs(trailImage.fillAmount - fillImage.fillAmount) < 0.001f;
            }

            // pulso con vida baja
            bool low = target > 0f && target <= lowHealthThreshold;
            if (low)
            {
                float p = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed);
                fillImage.color = Color.Lerp(normalColor, lowHealthColor, p);
            }
            else fillImage.color = normalColor;

            bool fillDone = Mathf.Abs(fillImage.fillAmount - target) < 0.001f;
            if (fillDone && trailDone && !low) break;

            yield return null;
        }

        fillImage.color = normalColor;
        anim = null;
    }

    private IEnumerator Shake()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float strength = shakeMagnitude * (1f - t / shakeDuration);
            rect.anchoredPosition = originalPos + Random.insideUnitCircle * strength;
            yield return null;
        }
        rect.anchoredPosition = originalPos;
        shake = null;
    }

    // Para probar 
    [ContextMenu("Probar: 100%")] private void Probar100() => HandleHealthChanged(100f, 100f);
    [ContextMenu("Probar: 50%")] private void Probar50() => HandleHealthChanged(50f, 100f);
    [ContextMenu("Probar: 20% (vida baja)")] private void Probar20() => HandleHealthChanged(20f, 100f);
}