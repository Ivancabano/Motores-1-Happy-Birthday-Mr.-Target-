using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Scene = UnityEngine.SceneManagement.Scene;
using LoadSceneMode = UnityEngine.SceneManagement.LoadSceneMode;

namespace Juego
{
public class SceneManager : MonoBehaviour
{
    public const string MENU = "Menu";
    public const string INTRODUCCION = "Introduccion";
    public const string VICTORIA = "Victoria";
    public const string DERROTA = "Derrota";
    public const string NIVEL_INICIAL = "Nivel Inicial";
    public const string SALA_JEFE = "Sala de Jefe";
    public const string ANTESALA = "Antesala";

    [SerializeField] private GameObject canvasMenu;
    [SerializeField] private GameObject canvasInstrucciones;

    private static string ultimoNivel = NIVEL_INICIAL;

    private void Start()
    {
        if (canvasInstrucciones != null) canvasInstrucciones.SetActive(false);
        if (canvasMenu != null) canvasMenu.SetActive(true);
    }

    public void AbrirInstrucciones()
    {
        if (canvasMenu != null) canvasMenu.SetActive(false);
        if (canvasInstrucciones != null) canvasInstrucciones.SetActive(true);
    }

    public void CerrarInstrucciones()
    {
        if (canvasInstrucciones != null) canvasInstrucciones.SetActive(false);
        if (canvasMenu != null) canvasMenu.SetActive(true);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Inicializar()
    {
        UnitySceneManager.sceneLoaded += AlCargarEscena;
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        Time.timeScale = 1f;

        if (escena.name == NIVEL_INICIAL || escena.name == SALA_JEFE)
            ultimoNivel = escena.name;

        bool esPantallaUI = escena.name == MENU || escena.name == VICTORIA || escena.name == DERROTA;
        if (esPantallaUI)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Jugar()
    {
        UnitySceneManager.LoadScene(INTRODUCCION);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void IrAlMenu()
    {
        UnitySceneManager.LoadScene(MENU);
    }

    public void Reintentar()
    {
        UnitySceneManager.LoadScene(ultimoNivel);
    }

    public static void CargarEscena(string nombre)
    {
        UnitySceneManager.LoadScene(nombre);
    }

    public static void Ganar()  { UnitySceneManager.LoadScene(VICTORIA); }
    public static void Perder() { UnitySceneManager.LoadScene(DERROTA); }
}
}