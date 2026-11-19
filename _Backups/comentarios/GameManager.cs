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

    [Tooltip("Segundos que se espera tras la muerte del jugador antes de mostrar la derrota, para que se vea la animacion de muerte.")]
    [SerializeField] private float defeatDelay = 3f;

    [SerializeField] private PlayerAmmo playerAmmo;

    private bool victoryPending;
    private bool defeatPending;

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
        if (defeatPending || victoryPending || CurrentState != GameState.Playing)
            return;

        defeatPending = true;

        // Cortamos el control del jugador ya, pero sin congelar el juego para que se vea la animacion
        DisablePlayerInput();
        StartCoroutine(DefeatAfterDeathAnimation());
    }

    // Espera a que termine la animacion de muerte del jugador (o el tiempo maximo) y recien ahi pierde
    private IEnumerator DefeatAfterDeathAnimation()
    {
        DeathSequence sequence = playerHealth != null ? playerHealth.GetComponent<DeathSequence>() : null;
        float elapsed = 0f;

        while (elapsed < defeatDelay)
        {
            if (sequence != null && sequence.IsFinished)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        defeatPending = false;
        GameOver();
    }

    private void HandleVictoryTargetDeath()
    {
        if (victoryPending || defeatPending || CurrentState != GameState.Playing)
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

    //NUEVO: Calcular el rango final
    RankManager.Instance?.FinishRun();

    LockPlayerControls();

        Debug.Log("¡VICTORIA!");

        if (playerAmmo != null)
            RunData.SaveAmmo(playerAmmo.CurrentMagazine, playerAmmo.CurrentReserve);
        else
            Debug.LogWarning("[GameManager] playerAmmo está vacío, no se guardaron las balas");

        OnVictory?.Invoke();
    Juego.SceneManager.Ganar();
}

    // Derrota inmediata (por ejemplo, cuando se termina el tiempo)
    public void TriggerGameOver()
    {
        if (victoryPending || defeatPending)
            return;

        GameOver();
    }

    private void GameOver()
{
    if (CurrentState != GameState.Playing)
        return;
    CurrentState = GameState.GameOver;
    LockPlayerControls();
    Debug.Log("GAME OVER");
    OnGameOver?.Invoke();
    Juego.SceneManager.Perder();
}

    private void LockPlayerControls()
    {
        DisablePlayerInput();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    private void DisablePlayerInput()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }
    }

    // Reiniciar siempre vuelve a empezar desde el Nivel Inicial
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Juego.SceneManager.ReiniciarJuego();
    }
}