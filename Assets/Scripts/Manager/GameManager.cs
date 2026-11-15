using System;
using System.Collections;
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

    [Tooltip("Segundos que se espera tras la muerte del objetivo antes de mostrar la victoria, para que se vea la animacion de muerte.")]
    [SerializeField] private float victoryDelay = 3.5f;

    private bool victoryPending;

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
            GameEvents.OnTimeFinished += TriggerGameOver;
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
            GameEvents.OnTimeFinished -= TriggerGameOver;
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
        if (victoryPending || CurrentState != GameState.Playing)
            return;

        victoryPending = true;
        StartCoroutine(VictoryAfterDeathAnimation());
    }

    // Espera a que termine la animacion de muerte del objetivo (o el tiempo maximo) y recien ahi gana
    private IEnumerator VictoryAfterDeathAnimation()
    {
        DeathSequence sequence = victoryTarget != null ? victoryTarget.GetComponent<DeathSequence>() : null;
        float elapsed = 0f;

        while (elapsed < victoryDelay)
        {
            if (sequence != null && sequence.IsFinished)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        victoryPending = false;
        TriggerVictory();
    }

    public void TriggerVictory()
{
    if (CurrentState != GameState.Playing)
        return;

    CurrentState = GameState.Victory;

    // NUEVO: Calcular el rango final
    RankManager.Instance?.FinishRun();

    LockPlayerControls();

    Debug.Log("¡VICTORIA!");

    OnVictory?.Invoke();
    Juego.SceneManager.Ganar();
}

    public void TriggerGameOver()
{
    if (CurrentState != GameState.Playing || victoryPending)
        return;
    CurrentState = GameState.GameOver;
    LockPlayerControls();
    Debug.Log("GAME OVER");
    OnGameOver?.Invoke();
    Juego.SceneManager.Perder();
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