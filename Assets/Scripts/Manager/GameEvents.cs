using System;

public static class GameEvents
{
    // Vida
    public static event Action<float, float> OnPlayerHealthChanged;
    public static void RaisePlayerHealthChanged(float current, float max)
        => OnPlayerHealthChanged?.Invoke(current, max);

    // Tiempo restante 
    public static event Action<float> OnTimeChanged;
    public static void RaiseTimeChanged(float remaining)
        => OnTimeChanged?.Invoke(remaining);

    public static event Action OnTimeFinished;
    public static void RaiseTimeFinished()
        => OnTimeFinished?.Invoke();
}