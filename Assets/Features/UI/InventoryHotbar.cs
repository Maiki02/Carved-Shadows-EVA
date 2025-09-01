using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasGroup))] //Para manejar la opacidad del inventario.
public class InventoryHotbar : MonoBehaviour
{
    [SerializeField] private Image[] slotImages;
    private PuzzlePiece[] pieces = new PuzzlePiece[5];
    private int selectedSlot = -1;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [Tooltip("Tiempo que el inventario está visible antes de ocultarse.")]
    [SerializeField] private float showDuration = 3.0f;
    [Tooltip("Valor de la opacidad cuando el inventario está visible (0 a 1).")]
    [Range(0f, 1f)]
    [SerializeField] private float visibleAlpha = 1f;
    [Tooltip("Valor de la opacidad cuando el inventario está oculto (0 a 1).")]
    [Range(0f, 1f)]
    [SerializeField] private float hiddenAlpha = 0.2f;
    private Coroutine showAndHideCoroutine; // Referencia a la corutina para evitar varias

    void Awake()
    {
        // Obtenemos la referencia al CanvasGroup en este mismo GameObject.
        inventoryCanvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        // Establece la opacidad inicial del inventario al empezar el juego.
        inventoryCanvasGroup.alpha = hiddenAlpha;
    }

    void Update()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SelectPiece(i);

                // Si se selecciona una pieza válida, muestra el inventario.
                if (selectedSlot != -1)
                {
                    // Si ya hay una corutina en ejecución, la detenemos para iniciar una nueva.
                    if (showAndHideCoroutine != null)
                    {
                        StopCoroutine(showAndHideCoroutine);
                    }
                    // Iniciamos la corutina que muestra y luego oculta el inventario.
                    showAndHideCoroutine = StartCoroutine(ShowAndHideInventory());
                }
            }
        }
    }


    void SelectPiece(int index)
    {
        // Si ya está seleccionado, deseleccionamos
        if (selectedSlot == index)
        {
            var currentImage = slotImages[selectedSlot];
            if (currentImage != null)
            {
                var existingOutline = currentImage.GetComponent<UnityEngine.UI.Outline>();
                if (existingOutline != null)
                {
                    Destroy(existingOutline);
                }
            }

            selectedSlot = -1;
            Debug.Log("Slot deseleccionado.");
            // Ocultamos el inventario inmediatamente si se deselecciona.
            if (showAndHideCoroutine != null) StopCoroutine(showAndHideCoroutine);
            showAndHideCoroutine = StartCoroutine(FadeCanvasGroup(inventoryCanvasGroup.alpha, hiddenAlpha, fadeDuration));
            return;
        }

        if (pieces[index] == null) return;

        // Quitar outline de todos los slots
        for (int i = 0; i < slotImages.Length; i++)
        {
            var oldOutline = slotImages[i].GetComponent<UnityEngine.UI.Outline>();
            if (oldOutline != null)
            {
                Destroy(oldOutline);
            }
        }

        // Agregar outline al nuevo slot seleccionado
        var image = slotImages[index];
        if (image != null)
        {
            var outline = image.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.red;
            outline.effectDistance = new Vector2(5f, -5f);
            outline.useGraphicAlpha = true;
        }

        selectedSlot = index;
        Debug.Log("Seleccionaste pieza en slot: " + (index + 1));
    }

    public void AddPiece(PuzzlePiece piece)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                pieces[i] = piece;
                slotImages[i].sprite = piece.GetIconUI();
                slotImages[i].color = new Color(1f, 1f, 1f, 1f); //Aclaramos para que se vea la imagen
                slotImages[i].enabled = true;
                piece.gameObject.SetActive(false);
                Debug.Log($"Pieza {piece.name} agregada al slot {i + 1}");
                return;
            }
        }

        Debug.Log("Inventario lleno, no se puede agregar la pieza.");
    }

    public PuzzlePiece GetSelectedPiece()
    {
        if (selectedSlot >= 0 && selectedSlot < pieces.Length)
        {
            return pieces[selectedSlot];
        }

        return null;
    }

    public void RemoveSelectedPiece()
    {
        if (selectedSlot >= 0 && selectedSlot < pieces.Length)
        {
            // Eliminar el outline antes de resetear el índice
            var outline = slotImages[selectedSlot].GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }

            slotImages[selectedSlot].sprite = null;
            slotImages[selectedSlot].enabled = true;
            slotImages[selectedSlot].color = new Color(0f, 0f, 0f, 0.54f);
            pieces[selectedSlot] = null;
            selectedSlot = -1;
        }
        else
        {
            Debug.LogWarning("No hay slot seleccionado o índice fuera de rango.");
        }
    }


    public void ResetSelection()
    {
        selectedSlot = -1;

        // Eliminar todos los outlines
        foreach (var img in slotImages)
        {
            var outline = img.GetComponent<Outline>();
            if (outline != null) Destroy(outline);
        }
    }

    public bool TieneObjeto(string nombreObjeto)
    {
        foreach (var pieza in pieces)
        {
            if (pieza != null && pieza.name == nombreObjeto)
            {
                return true;
            }
        }
        return false;
    }

    public bool TieneObjetoSeleccionado(PuzzlePiece piezaSeleccionada)
    {
        if (selectedSlot < 0 || selectedSlot >= pieces.Length)
            return false;

        return pieces[selectedSlot] == piezaSeleccionada;
    }

    public bool TieneObjectoSeleccionado(string nombreObjeto)
    {
        if (selectedSlot < 0 || selectedSlot >= pieces.Length)
            return false;

        return pieces[selectedSlot] != null && pieces[selectedSlot].name == nombreObjeto;
    }

    public PuzzlePiece[] ObtenerPiezas()
    {
        return pieces;
    }


    //-------------- UI --------------\\

    // Corutina que gestiona el ciclo completo de mostrar y ocultar el inventario.
    private IEnumerator ShowAndHideInventory()
    {
        yield return StartCoroutine(FadeCanvasGroup(inventoryCanvasGroup.alpha, visibleAlpha, fadeDuration));
        yield return new WaitForSeconds(showDuration);
        yield return StartCoroutine(FadeCanvasGroup(inventoryCanvasGroup.alpha, hiddenAlpha, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            inventoryCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        inventoryCanvasGroup.alpha = endAlpha;
    }
}
