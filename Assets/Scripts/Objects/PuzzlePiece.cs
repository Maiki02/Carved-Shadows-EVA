using Unity.VisualScripting;
using UnityEngine;

public class PuzzlePiece : ObjectInteract
{
    [Header("Datos de la pieza")]
    [SerializeField] private Sprite iconUI;
    [SerializeField] private int slotIndex;
    [SerializeField] private bool estaOculta = false;
    [SerializeField] private PuzzleSlot puzzleSlot;

    [HideInInspector]
    public bool recogida = false;

    [Header("Hotbar para guardar la pieza")]
    [SerializeField] private InventoryHotbar inventoryHotbar;


    protected override void Awake()
    {
        base.Awake();

        this.inventoryHotbar = GameObject.FindGameObjectWithTag("InventoryHotbar").GetComponent<InventoryHotbar>();
        if (this.inventoryHotbar == null)
        {
            Debug.LogError("No se encontró el InventoryHotbar en la escena.");
        }

        if (this.estaOculta)
        {
            this.gameObject.SetActive(false);
        }
    }


    public override void OnInteract()
    {
        Debug.Log("Interactuando con la pieza");

        if (!this.recogida )
        {
            if (puzzleSlot != null)
            {
                var piezaQuitada = puzzleSlot.QuitarPieza();

                if (piezaQuitada == null) return;

                Debug.Log("Agarramos la pieza");
                inventoryHotbar.AddPiece(this);
            }
            else
            {
                inventoryHotbar.AddPiece(this);   
            }


            return;
        }
        else
        {
            Debug.Log("No podemos agarrar la pieza, ya está recogida");
        }
    }

    public void MostrarPieza()
    {
        this.estaOculta = false;
        this.gameObject.SetActive(true);
    }

    public Sprite GetIconUI()
    {
        return this.iconUI;
    }

    public int GetSlotIndex()
    {
        return this.slotIndex;
    }

    public void SetSlotIndex(int index)
    {
        this.slotIndex = index;
    }

    public PuzzleSlot GetPuzzleSlot()
    {
        return this.puzzleSlot;
    }

    public void SetPuzzleSlot(PuzzleSlot slot)
    {
        this.puzzleSlot = slot;
    }

    /*public override void OnHoverEnter()
    {
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
    }*/
}
