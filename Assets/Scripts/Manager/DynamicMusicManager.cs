using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor de música persistente entre escenas.
/// - Dispara los snapshots según la escena y los eventos del juego.
/// - Bloque F: calcula el tiempo de transición según la distancia y
///   controla el cutoff del lowpass según la vida del jugador.
/// </summary>
public class DynamicMusicManager : MonoBehaviour
{
    public static DynamicMusicManager Instance { get; private set; }

    [System.Serializable]
    public class Pista
    {
        public AudioClip clip;
        public AudioMixerGroup grupo;        // grupo del Mixer (Ambiente, Alerta, Boss, Victoria)
        public AudioMixerSnapshot snapshot;  // snapshot que sube este grupo
        [System.NonSerialized] public AudioSource fuente;
    }

    [Header("Mixer")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] string parametroCutoff = "MusicCutoff";
    [SerializeField] AudioMixerSnapshot snapshotSilencio;

    [Header("Pistas")]
    [SerializeField] Pista ambienteNivel;     // almacén
    [SerializeField] Pista ambienteDialogo;   // escena del checkpoint (mismo grupo/snapshot que ambiente)
    [SerializeField] Pista alerta;            // Snapshot 1
    [SerializeField] Pista boss;              // Snapshot 2
    [SerializeField] Pista victoria;          // Snapshot 3

    [Header("Nombres de escena (deben coincidir con Build Settings)")]
    [SerializeField] string escenaNivel = "Almacen";
    [SerializeField] string escenaDialogo = "Dialogo";
    [SerializeField] string escenaJefe = "Jefe";
    [SerializeField] string escenaVictoria = "Victoria";
    [SerializeField] string escenaDerrota = "Derrota";

    [Header("Bloque F - parametros")]
    [SerializeField] float distanciaMaxDeteccion = 15f;
    [SerializeField] float cutoffMin = 800f;
    [SerializeField] float cutoffMax = 22000f;

    Pista actual;
    float vida = 1f;          // 0 a 1
    float ultimoCutoff = -1f;

    void Awake()
    {
        // Evita duplicados al volver a cargar la escena donde está el manager
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CrearFuente(ambienteNivel);
        CrearFuente(ambienteDialogo);
        CrearFuente(alerta);
        CrearFuente(boss);
        CrearFuente(victoria);

        SceneManager.sceneLoaded += AlCargarEscena;
        // Awake corre después de que cargó la primera escena, así que la procesamos a mano
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= AlCargarEscena;
    }

    void CrearFuente(Pista p)
    {
        p.fuente = gameObject.AddComponent<AudioSource>();
        p.fuente.clip = p.clip;
        p.fuente.outputAudioMixerGroup = p.grupo;
        p.fuente.loop = true;
        p.fuente.playOnAwake = false;
    }

    // ---------- Lógica por escena ----------
    void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        vida = 1f; // reinicia la vida al cambiar de escena

        string n = escena.name;
        if (n == escenaNivel) Cambiar(ambienteNivel, 1.5f);
        else if (n == escenaDialogo) Cambiar(ambienteDialogo, 1.5f);
        else if (n == escenaJefe) Cambiar(boss, 1f);
        else if (n == escenaVictoria) Cambiar(victoria, 1f);
        else if (n == escenaDerrota) Silenciar(0.3f);
    }

    // ---------- API pública (la llaman otros scripts) ----------

    /// <summary>Llamar cuando un enemigo o camara descubre al jugador. Solo actúa en el almacén.</summary>
    public void StartAlert(float distancia)
    {
        if (actual != ambienteNivel) return;
        // Más cerca = la alerta entra más rápido (0.3 s a 2 s)
        float t = Mathf.Lerp(0.3f, 2f, Mathf.Clamp01(distancia / distanciaMaxDeteccion));
        Cambiar(alerta, t);
    }

    /// <summary>Llamar cada vez que cambia la vida. Valor de 0 (muerto) a 1 (vida llena).</summary>
    public void SetVida(float vida01)
    {
        vida = Mathf.Clamp01(vida01);
    }

    // ---------- Internos ----------
    void Cambiar(Pista nueva, float tiempo)
    {
        if (actual == nueva) return;

        Pista anterior = actual;
        actual = nueva;

        nueva.fuente.Stop();
        nueva.fuente.Play();              // arranca desde el principio
        nueva.snapshot.TransitionTo(tiempo);

        if (anterior != null)
            StartCoroutine(DetenerLuego(anterior.fuente, tiempo + 0.1f));
    }

    void Silenciar(float tiempo)
    {
        actual = null;
        snapshotSilencio.TransitionTo(tiempo);
        foreach (var p in new[] { ambienteNivel, ambienteDialogo, alerta, boss, victoria })
            StartCoroutine(DetenerLuego(p.fuente, tiempo + 0.1f));
    }

    IEnumerator DetenerLuego(AudioSource fuente, float espera)
    {
        yield return new WaitForSecondsRealtime(espera);
        // Si mientras tanto volvió a ser la pista actual, no la cortamos
        if (actual == null || actual.fuente != fuente) fuente.Stop();
    }

    void Update()
    {
        // Parámetro 2 del Bloque F: cutoff según vida (escala logarítmica, se percibe más natural)
        float cutoff = cutoffMin * Mathf.Pow(cutoffMax / cutoffMin, vida);
        if (Mathf.Abs(cutoff - ultimoCutoff) > 1f)
        {
            mixer.SetFloat(parametroCutoff, cutoff);
            ultimoCutoff = cutoff;
        }
    }
}
