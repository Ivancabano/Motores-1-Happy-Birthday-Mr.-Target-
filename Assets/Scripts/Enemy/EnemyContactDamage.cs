using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float damageCooldown = 1f; // segundos entre golpes
    [SerializeField] private string playerTag = "Player";

    private float lastDamageTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EnemyContactDamage] Trigger detectado con: {other.gameObject.name} (tag: {other.gameObject.tag})");
        TryDamage(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[EnemyContactDamage] {other.name} no tiene el tag '{playerTag}', se ignora.");
            return;
        }

        if (Time.time - lastDamageTime < damageCooldown)
        {
            Debug.Log("[EnemyContactDamage] En cooldown, todavía no se puede volver a golpear.");
            return;
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            Debug.LogWarning($"[EnemyContactDamage] {other.name} tiene tag '{playerTag}' pero no tiene ningún componente IDamageable.");
            return;
        }

        damageable.TakeDamage(damageAmount);
        lastDamageTime = Time.time;

        Debug.Log($"[EnemyContactDamage] {gameObject.name} le hizo {damageAmount} de daño a {other.name}.");
    }
}