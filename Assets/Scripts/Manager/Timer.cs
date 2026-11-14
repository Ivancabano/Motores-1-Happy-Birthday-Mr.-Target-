using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float totalTime = 600f;

    private float remainingTime;
    private bool timeIsUp;
    private int lastWholeSecond = -1;

    public float RemainingTime => remainingTime;

    private void Start()
    {
        remainingTime = totalTime;
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