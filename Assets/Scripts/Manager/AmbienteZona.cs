using UnityEngine;

public class AmbienteZona : MonoBehaviour
{
    [Header("Ambientes (2D, loop)")]
    public AudioSource ambienteAlmacen;
    public AudioSource ambienteJefe;

    [Header("Volúmenes")]
    [Range(0f, 1f)] public float volumenAlmacen = 0.35f;
    [Range(0f, 1f)] public float volumenJefe = 0.35f;

    [Header("Transición")]
    public float duracionFade = 2f;

    float objetivoAlmacen;
    float objetivoJefe;

    void Start()
    {
        objetivoAlmacen = volumenAlmacen;
        objetivoJefe = 0f;
        ambienteAlmacen.volume = objetivoAlmacen;
        ambienteJefe.volume = objetivoJefe;
        if (!ambienteAlmacen.isPlaying) ambienteAlmacen.Play();
        if (!ambienteJefe.isPlaying) ambienteJefe.Play();
    }

    void Update()
    {
        float paso = Time.deltaTime / Mathf.Max(0.01f, duracionFade);
        ambienteAlmacen.volume = Mathf.MoveTowards(ambienteAlmacen.volume, objetivoAlmacen, paso);
        ambienteJefe.volume = Mathf.MoveTowards(ambienteJefe.volume, objetivoJefe, paso);
    }

    public void EntrarOficinaJefe()
    {
        objetivoAlmacen = 0f;
        objetivoJefe = volumenJefe;
    }

    public void SalirOficinaJefe()
    {
        objetivoAlmacen = volumenAlmacen;
        objetivoJefe = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) EntrarOficinaJefe();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) SalirOficinaJefe();
    }
}