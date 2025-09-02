using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private PlayerController playerController;


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
        if (isInventoryOpen && playerController != null)
        {
            playerController.SetControlesActivos(false);
        }
        // Si quieres reactivar controles al cerrar inventario, descomenta:
        else if (!isInventoryOpen && playerController != null)
        {
            playerController.SetControlesActivos(true);
        }
    }
}
