

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroduccionController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource introAudio;
    [SerializeField] private AudioClip primerAudio;
    [SerializeField] private AudioClip segundoAudio;

    [Header("Botones")]
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject playButton;

    [Header("Escena del juego")]
    [SerializeField] private string gameSceneName = "Nivel1";

    private bool puedeContinuar = false;

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        continueButton.SetActive(false);
        playButton.SetActive(false);

        StartCoroutine(ReproducirPrimerAudio());
    }

    private IEnumerator ReproducirPrimerAudio()
    {
        if (introAudio != null && primerAudio != null)
        {
            introAudio.clip = primerAudio;
            introAudio.loop = false;
            introAudio.Play();

            while (introAudio.isPlaying)
                yield return null;
        }
        else
        {
            Debug.LogWarning("Falta configurar el primer audio.");
        }

        // Aparece CONTINUE.
        puedeContinuar = true;
        continueButton.SetActive(true);
    }

    public void Continuar()
    {
        if (!puedeContinuar)
            return;

        puedeContinuar = false;
        continueButton.SetActive(false);

        StartCoroutine(ReproducirSegundoAudio());
    }

    private IEnumerator ReproducirSegundoAudio()
    {
        if (introAudio != null && segundoAudio != null)
        {
            introAudio.clip = segundoAudio;
            introAudio.loop = false;
            introAudio.Play();

            while (introAudio.isPlaying)
                yield return null;
        }
        else
        {
            Debug.LogWarning("Falta configurar el segundo audio.");
        }

        // Aparece PLAY.
        playButton.SetActive(true);
    }

    public void ComenzarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}
