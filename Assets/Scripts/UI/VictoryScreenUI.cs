
using UnityEngine;
using TMPro;

public class VictoryScreenUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Resultados")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI stealthText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory += ShowVictoryScreen;
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory -= ShowVictoryScreen;
        }
    }

    private void ShowVictoryScreen()
    {
        RankManager rank = RankManager.Instance;

        if (rank != null)
        {
            if (rankText != null)
                rankText.text = "Rango: " + rank.FinalRank;

            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(rank.ElapsedTime / 60f);
                int seconds = Mathf.FloorToInt(rank.ElapsedTime % 60f);

                timeText.text = $"Tiempo: {minutes:00}:{seconds:00}";
            }

            if (killsText != null)
                killsText.text = "Bajas: " + rank.EnemyKills;

            if (stealthText != null)
                stealthText.text = rank.WasDetected
                 ? "Detección: Detectado"
                 : "Detección: Sin detectar";
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void OnRetryButtonPressed()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.RestartLevel();
    }
}
}
