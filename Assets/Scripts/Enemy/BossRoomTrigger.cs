
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

        if (bossEnemy == null)
        {
            Debug.LogWarning("No se asigno el jefe.");
            return;
        }

        activated = true;
        bossEnemy.ActivateBoss();
    }
}

