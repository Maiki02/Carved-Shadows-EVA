using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private PlayerController playerController;

    [Header("Referencias Inventario")]
    [SerializeField] private BookUIController bookUIController;

    // Secciones del inventario (por ahora solo Book / Other)
    public enum InventorySection { None, Book, Other }
    [SerializeField] private InventorySection seccionActiva = InventorySection.Book;

    [Header("Input System")]
    // Gameplay/Inventory (Button): abre/cierra inventario (Q / Y)
    [SerializeField] private InputActionReference inventoryAction;
    // Gameplay/Cancel (Button, opcional): cierra inventario (Esc / B)
    [SerializeField] private InputActionReference cancelAction;

    [Header("Comportamiento")]
    [Tooltip("Cerrar inventario automáticamente si se activa la pausa.")]
    [SerializeField] private bool closeOnPause = true;

    [Tooltip("Bloquear movimiento y mirada del jugador mientras el inventario está abierto.")]
    [SerializeField] private bool blockLookAndMove = true;

    private bool isInventoryOpen = false;

    void Awake()
    {
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    void OnEnable()
    {
        inventoryAction?.action.Enable();
        cancelAction?.action.Enable();
    }

    void OnDisable()
    {
        inventoryAction?.action.Disable();
        cancelAction?.action.Disable();
    }

    void Start()
    {
        // Asegurar estado inicial cerrado
        SetInventoryVisible(false, applyPlayerControl: false);
    }

    void Update()
    {
        // Si el juego está en pausa, no abrir inventario. Si ya estaba abierto, cerrarlo.
        if (closeOnPause && GameController.Instance != null && GameController.Instance.IsPaused())
        {
            if (isInventoryOpen) ForceClose();
            return;
        }

        // Toggle inventario
        if (inventoryAction != null && inventoryAction.action.WasPressedThisFrame())
        {
            ToggleInventory();
            return;
        }

        // Cerrar con Cancel (opcional)
        if (isInventoryOpen && cancelAction != null && cancelAction.action.WasPressedThisFrame())
        {
            ForceClose();
        }
    }

    public void ToggleInventory()
    {
        if (GameController.Instance != null && GameController.Instance.IsPaused())
            return; // no abrir si está pausado

        bool show = !isInventoryOpen;
        SetInventoryVisible(show, applyPlayerControl: true);
    }

    public void ForceClose()
    {
        SetInventoryVisible(false, applyPlayerControl: true);
    }

    private void SetInventoryVisible(bool show, bool applyPlayerControl)
    {
        isInventoryOpen = show;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(show);

        // Manejo de sección activa (Libro)
        if (bookUIController != null && seccionActiva == InventorySection.Book)
        {
            if (show) bookUIController.AbrirLibro();
            else      bookUIController.CerrarLibro();
        }

        // Bloquear/permitir controles del jugador (mirada/movimiento/cursor)
        if (applyPlayerControl && playerController != null && blockLookAndMove)
        {
            playerController.SetControlesActivos(!show);
        }
    }
}
