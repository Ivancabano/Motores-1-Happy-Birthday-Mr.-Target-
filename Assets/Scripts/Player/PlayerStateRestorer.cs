using UnityEngine;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStateRestorer : MonoBehaviour
{
    [SerializeField] private bool continueFromPreviousLevel = false;

    private void Start()
    {
        if (continueFromPreviousLevel && RunData.HasData)
            GetComponent<PlayerHealth>().SetHealth(RunData.Health);
    }
}