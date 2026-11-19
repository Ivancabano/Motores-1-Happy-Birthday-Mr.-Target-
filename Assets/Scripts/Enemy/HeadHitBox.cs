using UnityEngine;

public class HeadHitbox : MonoBehaviour, IHeadshot
{
    [SerializeField] private TargetHealth targetHealth;

    private void Awake()
    {
      
        if (targetHealth == null)
            targetHealth = GetComponentInParent<TargetHealth>();
    }

    public void OnHeadshot()
    {
        Debug.Log("¡Disparo crítico en la CABEZA!");

        if (targetHealth != null)
        {
            targetHealth.TakeHeadshot();
        }
    }
}