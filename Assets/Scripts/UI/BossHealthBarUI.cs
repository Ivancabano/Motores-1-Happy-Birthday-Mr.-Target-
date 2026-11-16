using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image trailImage;
    [SerializeField] private CanvasGroup visibilityGroup;

    [Header("Animación")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Estela de daño")]
    [SerializeField] private float trailDelay = 0.5f;
    [SerializeField] private float trailSpeed = 0.3f;

    private float target = 1f;
    private float trailTimer;
    private bool initialized;
    private Coroutine anim;
    private Coroutine fade;

    private void Awake()
    {
        if (visibilityGroup != null) visibilityGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        GameEvents.OnBossHealthChanged += HandleHealthChanged;
        GameEvents.OnBossActiveChanged += HandleActiveChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnBossHealthChanged -= HandleHealthChanged;
        GameEvents.OnBossActiveChanged -= HandleActiveChanged;
        anim = null;
        fade = null;
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
            return;
        }

        if (tookDamage) trailTimer = trailDelay;

        if (anim == null) anim = StartCoroutine(Animate());
    }

    private void HandleActiveChanged(bool active)
    {
        if (visibilityGroup == null) return;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(Fade(active ? 1f : 0f));
    }

    private IEnumerator Animate()
    {
        while (true)
        {
            float dt = Time.unscaledDeltaTime;

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

            bool fillDone = Mathf.Abs(fillImage.fillAmount - target) < 0.001f;
            if (fillDone && trailDone) break;

            yield return null;
        }
        anim = null;
    }

    private IEnumerator Fade(float to)
    {
        float from = visibilityGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            visibilityGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        visibilityGroup.alpha = to;
        fade = null;
    }
}