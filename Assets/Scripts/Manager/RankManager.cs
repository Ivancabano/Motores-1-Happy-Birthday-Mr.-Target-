using UnityEngine;

public class RankManager : MonoBehaviour
{
    public static RankManager Instance { get; private set; }

    [Header("Configuracion de rangos")]
    [SerializeField] private float fastTime = 240f;
    [SerializeField] private float mediumTime = 420f;
    [SerializeField] private int maxLowKills = 2;

    public bool WasDetected { get; private set; }
    public int EnemyKills { get; private set; }
    public float ElapsedTime { get; private set; }
    public string FinalRank { get; private set; } = "";

    private float startTime;
    private bool finished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        startTime = Time.time;
    }

    private void Update()
    {
        if (finished) return;

        ElapsedTime = Time.time - startTime;
    }

    public void RegisterDetection()
    {
        if (finished) return;

        WasDetected = true;
    }

    public void RegisterKill()
    {
        if (finished) return;

        EnemyKills++;
    }

    public void FinishRun()
    {
        if (finished) return;

        ElapsedTime = Time.time - startTime;
        finished = true;
        FinalRank = CalculateRank();

        Debug.Log(
            "RANGO: " + FinalRank +
            " | Detectado: " + WasDetected +
            " | Bajas: " + EnemyKills +
            " | Tiempo: " + ElapsedTime.ToString("F1")
        );
    }

    private string CalculateRank()
    {
        bool fewKills = EnemyKills <= maxLowKills;
        bool fast = ElapsedTime <= fastTime;
        bool medium = ElapsedTime <= mediumTime;

        if (!WasDetected && fewKills && fast)
            return "S";

        if (fewKills && fast)
            return "A";

        if (fewKills && medium)
            return "B";

        return "C";
    }
}
