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

    [Header("Referencias Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooting playerShooting;

    [Header("Objetivo de Victoria")]
    [SerializeField] private TargetHealth victoryTarget;

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
        // Escuchamos la muerte del Player
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        // Escuchamos la muerte del enemigo objetivo
        if (victoryTarget != null)
        {
            victoryTarget.OnDeath += HandleVictoryTargetDeath;
        }
    }

    private void OnDisable()
    {
        // Dejamos de escuchar la muerte del Player
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }

        // Dejamos de escuchar la muerte del enemigo objetivo
        if (victoryTarget != null)
        {
            victoryTarget.OnDeath -= HandleVictoryTargetDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        TriggerGameOver();
    }

    private void HandleVictoryTargetDeath()
    {
        TriggerVictory();
    }

    public void TriggerVictory()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.Victory;

        LockPlayerControls();

        Debug.Log("¡VICTORIA!");

        OnVictory?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing)
            return;

        CurrentState = GameState.GameOver;

        LockPlayerControls();

        Debug.Log("GAME OVER");

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