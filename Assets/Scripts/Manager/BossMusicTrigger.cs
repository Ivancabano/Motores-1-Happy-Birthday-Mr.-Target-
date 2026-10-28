
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossMusicTrigger : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (DynamicMusicManager.Instance != null)
        {
            DynamicMusicManager.Instance.StartBossMusic();
        }
    }
}

