using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject inventoryPanel;

    [SerializeField] private PlayerController playerController;
    [Header("Referencias Inventario")]
    [SerializeField] private BookUIController bookUIController;

    // Enum para saber en qué sección del inventario estamos
    public enum InventorySection { None, Book, Other }
    [SerializeField] private InventorySection seccionActiva = InventorySection.Book;


    private bool isInventoryOpen = false;

    void Awake()
    {
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        // Abrir/Cerrar libro si la sección activa es Book
        if (bookUIController != null && seccionActiva == InventorySection.Book)
        {
            if (isInventoryOpen)
                bookUIController.AbrirLibro();
            else
                bookUIController.CerrarLibro();
        }

        if (isInventoryOpen && playerController != null)
        {
            Debug.Log("Desactivando controles del jugador al abrir inventario.");
            playerController.SetControlesActivos(false);
        }
        else if (!isInventoryOpen && playerController != null)
        {
            playerController.SetControlesActivos(true);
        }
    }
}
