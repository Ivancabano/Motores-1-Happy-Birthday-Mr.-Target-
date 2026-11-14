using UnityEngine;

// Barra de vida del jefe. Se muestra solo cuando el jefe esta activo.
// Requiere un RectTransform "fill" con el pivot en el borde izquierdo (X = 0).
public class BossHealthBarUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TargetHealth bossHealth;   // Si queda vacio, lo busca solo en la escena
    [SerializeField] private RectTransform fill;
    [SerializeField] private CanvasGroup visibilityGroup; // Opcional: se usa para ocultar la barra

    [Header("Animacion")]
    [SerializeField] private float smoothSpeed = 8f;

    private BossSniperAttack sniper;
    private float target = 1f;
    private float shown = 1f;

    private void Start()
    {
        sniper = FindFirstObjectByType<BossSniperAttack>();

        if (bossHealth == null && sniper != null)
            bossHealth = sniper.GetComponent<TargetHealth>();

        SetVisible(false);
    }

    private void Update()
    {
        if (bossHealth == null || fill == null)
            return;

        bool active = sniper != null && sniper.IsActive;
        SetVisible(active);

        target = bossHealth.MaxHealth > 0f
            ? Mathf.Clamp01(bossHealth.CurrentHealth / bossHealth.MaxHealth)
            : 0f;

        shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime));
        fill.localScale = new Vector3(shown, 1f, 1f);
    }

    private void SetVisible(bool visible)
    {
        if (visibilityGroup == null)
            return;

        visibilityGroup.alpha = visible ? 1f : 0f;
    }
}
