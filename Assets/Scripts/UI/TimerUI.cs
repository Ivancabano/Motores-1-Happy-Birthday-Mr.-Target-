using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Aviso (color fijo)")]
    [SerializeField] private float warningTime = 60f;
    [SerializeField] private Color warningColor = Color.red;

    [Header("Parpadeo final")]
    [SerializeField] private float blinkTime = 10f;
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkSpeed = 6f;

    private Color normalColor;
    private float remaining = float.MaxValue;
    private bool finished;

    private void Awake()
    {
        if (timerText != null) normalColor = timerText.color;
    }

    private void OnEnable()
    {
        GameEvents.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnTimeChanged -= HandleTimeChanged;
        if (timerText != null) timerText.color = normalColor;
    }

    private void HandleTimeChanged(float time)
    {
        if (timerText == null) return;

        remaining = time;
        finished = time <= 0f;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        timerText.color = time <= warningTime ? warningColor : normalColor;
    }

    private void Update()
    {
        if (timerText == null || finished || remaining > blinkTime) return;

        float p = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * blinkSpeed);
        timerText.color = Color.Lerp(warningColor, blinkColor, p);
    }
}