using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Victory,
        GameOver
    }

    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooting playerShooting;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public event Action OnVictory;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        TriggerGameOver();
    }
    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Victory;

        LockPlayerControls();

        OnVictory?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.GameOver;

        LockPlayerControls();

        OnGameOver?.Invoke();
    }

    private void LockPlayerControls()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}