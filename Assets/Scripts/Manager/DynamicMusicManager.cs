using System.Collections;
using UnityEngine;
public class DynamicMusicManager : MonoBehaviour
{
    public static DynamicMusicManager Instance { get; private set; }

    [Header("Canciones")]
    [SerializeField] private AudioClip explorationMusic;
    [SerializeField] private AudioClip alertMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip victoryMusic;

    [Header("Transiciones")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;
    private Coroutine transition;

    private bool isBossScene = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Sobrevive entre escenas (Nivel ➔ Cómic ➔ Boss ➔ Victoria)

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
        victoryMusic?.LoadAudioData();

        PlayMusic(explorationMusic);
    }

    public void StartAlert()
    {
        if (isBossScene) return;

        PlayMusic(alertMusic);
    }

    public void StartBossMusic()
    {
        isBossScene = true;
        PlayMusic(bossMusic);
    }

    public void StopMusicOnDeath()
    {
        if (transition != null)
            StopCoroutine(transition);

        if (sourceA != null) sourceA.Stop();
        if (sourceB != null) sourceB.Stop();

        isBossScene = false;
    }

    public void PlayVictoryMusic()
    {
        isBossScene = false;
        PlayMusic(victoryMusic);
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