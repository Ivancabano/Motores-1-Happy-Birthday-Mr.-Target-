using UnityEngine;

public class DefeatScreenUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject defeatPanel;

    private void Start()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowDefeatScreen;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowDefeatScreen;
        }
    }

    private void ShowDefeatScreen()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
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
