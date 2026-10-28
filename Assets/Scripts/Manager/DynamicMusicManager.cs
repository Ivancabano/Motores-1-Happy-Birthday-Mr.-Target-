
using System.Collections;
using UnityEngine;

public class DynamicMusicManager : MonoBehaviour
{
    public static DynamicMusicManager Instance { get; private set; }

    [Header("Canciones")]
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip alertMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("Transiciones")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;
    private Coroutine transition;

    private bool bossActive;
    private bool alertActive;

    private void Awake()
    {
        Instance = this;

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        sourceA.loop = true;
        sourceB.loop = true;
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;

        activeSource = sourceA;
    }

    private void Start()
    {
        explorationMusic?.LoadAudioData();
        alertMusic?.LoadAudioData();
        bossMusic?.LoadAudioData();

    PlayMusic(explorationMusic);
    }

    public void StartAlert()
    {
        if (bossActive) return;

        alertActive = true;
        PlayMusic(alertMusic);
    }

    public void StartBossMusic()
    {
        if (bossActive) return;

        bossActive = true;
        PlayMusic(bossMusic);
    }

    public void EndBossMusic()
    {
        bossActive = false;
        PlayMusic(alertActive ? alertMusic : explorationMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (activeSource.clip == clip && activeSource.isPlaying) return;

        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(Crossfade(clip));
    }

    private IEnumerator Crossfade(AudioClip newClip)
    {
        AudioSource oldSource = activeSource;
        AudioSource newSource = activeSource == sourceA ? sourceB : sourceA;

        newSource.Stop();
        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.Play();

        float startVolume = oldSource.volume;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            oldSource.volume = Mathf.Lerp(startVolume, 0f, t);
            newSource.volume = Mathf.Lerp(0f, volume, t);

            yield return null;
        }

        oldSource.Stop();
        oldSource.volume = 0f;
        newSource.volume = volume;

        activeSource = newSource;
        transition = null;
    }
}

