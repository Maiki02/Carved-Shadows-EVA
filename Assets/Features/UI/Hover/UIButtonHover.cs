using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HoverController hoverController;

    void Start()
    {
        // Buscar el HoverController en la UI
        hoverController = FindObjectOfType<HoverController>();
        
        if (hoverController == null)
        {
            Debug.LogWarning("No se encontró HoverController en la escena. Asegúrate de agregarlo como componente en un GameObject de la UI.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverController != null)
        {
            hoverController.PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Aquí puedes agregar lógica para cuando el mouse salga del botón si es necesario
    }
}