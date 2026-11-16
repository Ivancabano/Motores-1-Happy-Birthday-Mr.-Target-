using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float totalTime = 600f;
    [SerializeField] private bool continueFromPreviousLevel = false;

    private float remainingTime;
    private bool timeIsUp;
    private int lastWholeSecond = -1;

    public float RemainingTime => remainingTime;

    private void Start()
    {
        Debug.Log($"[Timer] continuar={continueFromPreviousLevel} | hayDatos={RunData.HasData} | tiempoGuardado={RunData.RemainingTime:0.0}");

        remainingTime = (continueFromPreviousLevel && RunData.HasData)
            ? RunData.RemainingTime
            : totalTime;

        NotifyIfSecondChanged();
    }

    private void Update()
    {
        if (timeIsUp) return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timeIsUp = true;
            GameEvents.RaiseTimeChanged(0f);
            GameEvents.RaiseTimeFinished();
            return;
        }

        NotifyIfSecondChanged();
    }

    private void NotifyIfSecondChanged()
    {
        int whole = Mathf.FloorToInt(remainingTime);
        if (whole == lastWholeSecond) return;
        lastWholeSecond = whole;
        GameEvents.RaiseTimeChanged(remainingTime);
    }
}