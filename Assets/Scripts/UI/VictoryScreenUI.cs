using UnityEngine;

public class VictoryScreenUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject victoryPanel;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory += ShowVictoryScreen;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
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
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }
    public void OnRetryButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
    }
}