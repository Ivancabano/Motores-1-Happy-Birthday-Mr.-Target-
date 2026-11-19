
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] private BossEnemy bossEnemy;

    private bool activated;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        activated = true;

        if (bossEnemy != null)
        {
            bossEnemy.ActivateBoss();
            return;
        }

        BossSniperAttack sniper = FindFirstObjectByType<BossSniperAttack>();
        if (sniper != null)
        {
            sniper.Activate();
            return;
        }

        Debug.LogWarning("[BossRoomTrigger] No se encontro ningun jefe para activar.");
    }
}

