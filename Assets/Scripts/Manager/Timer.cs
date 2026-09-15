using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Configuración")]
    [SerializeField] private float totalTime = 600f;   
    [SerializeField] private float warningTime = 60f;  

    private float remainingTime;
    private bool timeIsUp = false;

    public float RemainingTime => remainingTime;

    private void Start()
    {
        remainingTime = totalTime;
        UpdateTimerText();
    }

    private void Update()
    {
        if (timeIsUp) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timeIsUp = true;
            UpdateTimerText();
            TimeOver();
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (remainingTime <= warningTime)
        {
            timerText.color = Color.red;
        }
    }

    private void TimeOver()
    {
        Debug.Log("Se acabó el tiempo");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
