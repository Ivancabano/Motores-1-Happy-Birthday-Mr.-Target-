using UnityEngine;

[RequireComponent(typeof(TargetHealth))]
public class BossHealthEvents : MonoBehaviour
{
    private TargetHealth health;
    private BossSniperAttack sniper;

    private bool lastActive;
    private bool activeInitialized;

    private void Awake()
    {
        health = GetComponent<TargetHealth>();
        sniper = GetComponent<BossSniperAttack>();
    }

    private void OnEnable() => health.OnHealthChanged += Relay;
    private void OnDisable() => health.OnHealthChanged -= Relay;

    private void Relay(float current, float max)
        => GameEvents.RaiseBossHealthChanged(current, max);

    private void Update()
    {
        if (sniper == null || health.IsDead) return;

        if (!activeInitialized || sniper.IsActive != lastActive)
        {
            activeInitialized = true;
            lastActive = sniper.IsActive;
            GameEvents.RaiseBossActiveChanged(lastActive);
        }
    }

}