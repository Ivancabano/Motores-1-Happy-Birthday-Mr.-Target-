using UnityEngine;
public class RunDataRecorder : MonoBehaviour
{
    private float health = -1f;
    private float time = -1f;

    private void OnEnable()
    {
        GameEvents.OnPlayerHealthChanged += HandleHealth;
        GameEvents.OnTimeChanged += HandleTime;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHealthChanged -= HandleHealth;
        GameEvents.OnTimeChanged -= HandleTime;
    }

    private void HandleHealth(float current, float max)
    {
        health = current;
        TrySave();
    }

    private void HandleTime(float remaining)
    {
        time = remaining;
        TrySave();
    }

    private void TrySave()
    {
        if (health < 0f || time < 0f) return;
        RunData.Save(health, time);
    }
}