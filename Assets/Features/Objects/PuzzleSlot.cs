using UnityEngine;

public class PuzzleSlot : ObjectInteract
{
    public int slotIndexCorrecto;
    [HideInInspector] public PuzzlePiece piezaColocada;

    private MeshRenderer meshRenderer;

    [Header("Hotbar para guardar la pieza")]
    [SerializeField] private InventoryHotbar inventoryHotbar;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = GetComponent<MeshRenderer>();

        this.inventoryHotbar = GameObject.FindGameObjectWithTag("InventoryHotbar").GetComponent<InventoryHotbar>();
        if (this.inventoryHotbar == null)
        {
            Debug.LogError("No se encontró el InventoryHotbar en la escena.");
        }
    }

    public bool ColocarPieza(PuzzlePiece pieza)
    {
        if (piezaColocada != null)
        {
            Debug.LogWarning("Este slot ya tiene una pieza.");
            return false;
        }

        Debug.Log("QUEREMOS COLOCAR LA PIEZA: " + pieza.name);

        //pieza.transform.SetParent(null);
        pieza.transform.SetParent(transform);  // la anida al slot
        pieza.transform.position = transform.position;
        //pieza.transform.rotation = Quaternion.Euler(0, 0, 0);
        pieza.transform.localEulerAngles = Vector3.zero;
        pieza.transform.localScale       = Vector3.one;
        //--------- ESTO LO AGREGUE PARA QUE LA PIEZA ENCAJE, SE ME IBA PARA CUALQUIER LADO ----------------\\

        pieza.gameObject.SetActive(true);
        pieza.recogida = false; // Marcamos la pieza como recogida
        pieza.SetPuzzleSlot(this); // Asignamos el slot a la pieza
        piezaColocada = pieza;

        // Ocultar visual del slot
        if (meshRenderer != null)
        {
            Debug.Log("Ocultando visual del slot");
            meshRenderer.enabled = false;
        }

        PuzzleManager.Instancia?.VerificarYValidarPuzzle();

        return true;
    }


    public PuzzlePiece QuitarPieza()
    {
        if (piezaColocada == null)
            return null;

        if (PuzzleManager.Instancia != null && PuzzleManager.Instancia.PuzzleFueResuelto())
        {
            Debug.Log("No se puede quitar la pieza: puzzle ya resuelto.");
            return null;
        }

        Debug.Log("QUITANDO LA PIEZA: " + piezaColocada.name);

        PuzzlePiece pieza = piezaColocada;
        piezaColocada = null;
        pieza.recogida = true; // Marcamos la pieza como recogida

        // Restaurar visual del slot
        if (meshRenderer != null)
        {
            Debug.Log("Restaurando visual del slot");
            meshRenderer.enabled = true;
            
        }

        PuzzleManager.Instancia?.VerificarYValidarPuzzle();

        return pieza;
    }

    public bool EsCorrecta()
    {
        return piezaColocada != null && piezaColocada.GetSlotIndex() == slotIndexCorrecto;
    }

    public override void OnInteract()
    {
        InventoryHotbar inv = FindObjectOfType<InventoryHotbar>();
        PuzzlePiece piezaSeleccionada = inv.GetSelectedPiece();

        //Si existe una pieza seleccionada, intenta colocarla en el slot
        if (piezaSeleccionada != null)
        {
            bool colocado = ColocarPieza(piezaSeleccionada);
            if (colocado)
            {
                inv.RemoveSelectedPiece();
                Debug.Log($"Colocaste la pieza {piezaSeleccionada.name} en {name}");
            }
            else
            {
                Debug.Log("Slot ya ocupado.");
            }
        }
        else
        {
            // Si no hay pieza seleccionada, intenta quitar la pieza del slot
            PuzzlePiece piezaRemovida = QuitarPieza();
            if (piezaRemovida != null)
            {
                piezaRemovida.gameObject.SetActive(false);
                inv.AddPiece(piezaRemovida);
                Debug.Log($"Recogiste nuevamente la pieza: {piezaRemovida.name}");
            }
        }
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        // Aquí puedes agregar lógica adicional al entrar en el slot
    }
    
    public override void OnHoverExit()
    {
        base.OnHoverExit();
        // Aquí puedes agregar lógica adicional al salir del slot
    }
}