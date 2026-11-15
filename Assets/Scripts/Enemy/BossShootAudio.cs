using UnityEngine;

[RequireComponent(typeof(BossSniperAttack))]
public class BossShootAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private BossSniperAttack sniperAttack;

    private void Awake()
    {
        sniperAttack = GetComponent<BossSniperAttack>();

        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();
    }

    private void OnEnable()
    {
        if (sniperAttack != null)
            sniperAttack.OnShoot += PlayShootSound;
    }

    private void OnDisable()
    {
        if (sniperAttack != null)
            sniperAttack.OnShoot -= PlayShootSound;
    }

    private void PlayShootSound()
    {
        if (audioSource == null || shootClip == null)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(shootClip, volume);
    }

    [ContextMenu("Probar sonido de disparo")]
    private void TestShootSound() => PlayShootSound();
}