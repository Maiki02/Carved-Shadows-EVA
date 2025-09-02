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

    private int paginaActual = 0; // Índice de la página izquierda actual

    void Start()
    {
        ActualizarPaginas();
        nextButton.onClick.AddListener(PasarPaginaAdelante);
        prevButton.onClick.AddListener(PasarPaginaAtras);
    }

    // Actualiza las imágenes de las páginas y la visibilidad de las flechas
    void ActualizarPaginas()
    {
        // Página izquierda
        if (paginaActual < paginas.Length)
            leftPageImage.sprite = paginas[paginaActual];
        else
            leftPageImage.sprite = null;

        // Página derecha
        if (paginaActual + 1 < paginas.Length)
            rightPageImage.sprite = paginas[paginaActual + 1];
        else
            rightPageImage.sprite = null;

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
        }
    }

    // Retrocede dos páginas
    void PasarPaginaAtras()
    {
        if (paginaActual - 2 >= 0)
        {
            paginaActual -= 2;
            ActualizarPaginas();
        }
    }
}
