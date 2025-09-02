using UnityEngine;
using UnityEngine.UI;

public class BookUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image leftPageImage; // Imagen de la página izquierda
    [SerializeField] private Image rightPageImage; // Imagen de la página derecha
    [SerializeField] private Button nextButton; // Botón para avanzar página
    [SerializeField] private Button prevButton; // Botón para retroceder página

    [SerializeField] private Sprite[] paginas; // Array de sprites de las páginas

    [Header("Audio")]
    [SerializeField] private AudioClip sfxAbrirLibro; // Sonido al abrir/cerrar el libro
    [SerializeField] private AudioClip sfxPasarPagina; // Sonido al pasar de página

    private int paginaActual = 0; // Índice de la página izquierda actual


    void Start()
    {
        ActualizarPaginas();
        nextButton.onClick.AddListener(PasarPaginaAdelante);
        prevButton.onClick.AddListener(PasarPaginaAtras);
    }

    // Llama este método al abrir el libro
    public void AbrirLibro()
    {
        // Reproducir sonido de abrir libro
        if (sfxAbrirLibro != null && AudioController.Instance != null)
            AudioController.Instance.PlaySFXClip(sfxAbrirLibro);
    }

    // Llama este método al cerrar el libro
    public void CerrarLibro()
    {
        // Reproducir sonido de cerrar libro (puedes usar el mismo clip)
        if (sfxAbrirLibro != null && AudioController.Instance != null)
            AudioController.Instance.PlaySFXClip(sfxAbrirLibro);
    }

    // Actualiza las imágenes de las páginas y la visibilidad de las flechas
    void ActualizarPaginas()
    {
        // Página izquierda
        if (paginaActual < paginas.Length && paginas[paginaActual] != null)
        {
            leftPageImage.sprite = paginas[paginaActual];
            Color c = leftPageImage.color;
            c.a = 1f;
            leftPageImage.color = c;
        }
        else
        {
            leftPageImage.sprite = null;
            Color c = leftPageImage.color;
            c.a = 0f;
            leftPageImage.color = c;
        }

        // Página derecha
        if (paginaActual + 1 < paginas.Length && paginas[paginaActual + 1] != null)
        {
            rightPageImage.sprite = paginas[paginaActual + 1];
            Color c = rightPageImage.color;
            c.a = 1f;
            rightPageImage.color = c;
        }
        else
        {
            rightPageImage.sprite = null;
            Color c = rightPageImage.color;
            c.a = 0f;
            rightPageImage.color = c;
        }

        // Mostrar/ocultar flechas según si hay más páginas
        prevButton.gameObject.SetActive(paginaActual > 0);
        nextButton.gameObject.SetActive(paginaActual + 2 < paginas.Length);
    }

    // Avanza dos páginas

    void PasarPaginaAdelante()
    {
        if (paginaActual + 2 < paginas.Length)
        {
            paginaActual += 2;
            ActualizarPaginas();
            // Reproducir sonido de pasar página
            if (sfxPasarPagina != null && AudioController.Instance != null)
                AudioController.Instance.PlaySFXClip(sfxPasarPagina);
        }
    }

    // Retrocede dos páginas
    void PasarPaginaAtras()
    {
        if (paginaActual - 2 >= 0)
        {
            paginaActual -= 2;
            ActualizarPaginas();
            // Reproducir sonido de pasar página
            if (sfxPasarPagina != null && AudioController.Instance != null)
                AudioController.Instance.PlaySFXClip(sfxPasarPagina);
        }
    }
}
