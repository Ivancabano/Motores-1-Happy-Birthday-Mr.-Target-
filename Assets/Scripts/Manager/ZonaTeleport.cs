using UnityEngine;

public class ZonaTeleport : MonoBehaviour
{
    [SerializeField] private string escenaDestino = Juego.SceneManager.ANTESALA;
    [SerializeField] private string tagJugador = "Player";

    private bool usado = false;

    private void OnTriggerEnter(Collider other)
    {
        Teletransportar(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Teletransportar(other.gameObject);
    }

    private void Teletransportar(GameObject obj)
    {
        if (usado || !obj.CompareTag(tagJugador)) return;

        usado = true;
        Juego.SceneManager.CargarEscena(escenaDestino);
    }
}