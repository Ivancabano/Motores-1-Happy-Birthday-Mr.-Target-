using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RectTransform fill;

    [Header("Animación")]
    [SerializeField] private float smoothSpeed = 8f;

    private float target = 1f;
    private float shown = 1f;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += SetHealth;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= SetHealth;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            shown = target;
            ApplyFill();
        }
    }

    public void SetHealth(float current, float max)
    {
        target = max > 0f ? Mathf.Clamp01(current / max) : 0f;
    }

    private void Update()
    {
        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime));
        ApplyFill();
    }

    private void ApplyFill()
    {
        float width = fill.rect.width;
        fill.anchoredPosition = new Vector2(-(1f - shown) * width, 0f);
    }

    [ContextMenu("Probar: daño de 10")]
    private void ProbarDanio()
    {
        if (playerHealth != null) playerHealth.TakeDamage(10f);
    }

    [ContextMenu("Probar: curar 10")]
    private void ProbarCuracion()
    {
        if (playerHealth != null) playerHealth.Heal(10f);
    }
}