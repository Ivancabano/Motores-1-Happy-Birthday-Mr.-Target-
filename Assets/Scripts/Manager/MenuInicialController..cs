
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicialController : MonoBehaviour
{
    [Header("Escena de introduccion")]
    [SerializeField] private string introSceneName = "Introduccion";

    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Jugar()
    {
        SceneManager.LoadScene(introSceneName);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
