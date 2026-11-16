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
    
    // Jefe
    public static event Action<float, float> OnBossHealthChanged;
    public static void RaiseBossHealthChanged(float current, float max)
        => OnBossHealthChanged?.Invoke(current, max);

    public static event Action<bool> OnBossActiveChanged;
    public static void RaiseBossActiveChanged(bool active)
        => OnBossActiveChanged?.Invoke(active);
}