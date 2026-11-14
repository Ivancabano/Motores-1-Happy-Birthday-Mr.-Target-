using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHealthEvents : MonoBehaviour
{
    private PlayerHealth health;

    private void Awake() => health = GetComponent<PlayerHealth>();

    private void OnEnable() => health.OnHealthChanged += Relay;
    private void OnDisable() => health.OnHealthChanged -= Relay;

    private void Start() => Relay(health.CurrentHealth, health.MaxHealth);

    private void Relay(float current, float max)
        => GameEvents.RaisePlayerHealthChanged(current, max);
}