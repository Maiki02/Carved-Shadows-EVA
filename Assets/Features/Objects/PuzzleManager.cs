using System.Collections;
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public PuzzleSlot[] slots;
    //public TextMeshProUGUI resultadoTexto;
    //public float tiempoMensaje = 3f;
    [SerializeField] private PuzzleType puzzleType;

    [SerializeField] private PuzzlePiece objetoRecompensa;

    [SerializeField] private LeatherTrunk maletin;

    [SerializeField] private Radio radio;

    [SerializeField] private Door doorToOpen;

    private bool puzzleResuelto = false;
    public static PuzzleManager Instancia { get; private set; }

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        /*if (resultadoTexto != null)
        {
            resultadoTexto.gameObject.SetActive(false);
        }

        if (objetoRecompensa != null)
        {
            objetoRecompensa.SetActive(false); // Asegura que empiece oculto
        }*/
    }

    public void VerificarYValidarPuzzle()
    {
        Debug.Log("Verificando puzzle... " + puzzleType);
        if (puzzleResuelto) return;

        int colocadas = 0;
        int correctas = 0;

        foreach (var slot in slots)
        {
            if (slot.piezaColocada != null)
            {
                colocadas++;
                if (slot.EsCorrecta())
                    correctas++;
            }
        }

        // Solo validar si todas las piezas están colocadas
        if (colocadas == slots.Length)
        {
            //resultadoTexto.gameObject.SetActive(true);

            if (correctas == slots.Length)
            {
                //resultadoTexto.text = "¡Puzzle resuelto!";
                puzzleResuelto = true;
                Debug.Log("Puzzle resuelto");
                if (puzzleType == PuzzleType.Museum && maletin != null)
                {
                    //Significa que estamos en el puzzle 1
                    objetoRecompensa.MostrarPieza(); // Mostramos la pieza de recompensa
                    maletin.Open(); // Abrimos el maletín al resolver el puzzle
                }
                else if (puzzleType == PuzzleType.Cuadros && objetoRecompensa != null)
                {

                    InventoryHotbar inv = FindObjectOfType<InventoryHotbar>();
                    if (inv != null)
                    {
                        inv.AddPiece(objetoRecompensa);
                    }
                    //doorToOpen.SetType(TypeDoorInteract.NextLevel); // Cambiamos el tipo de puerta a NextLevel
                }
                else if (puzzleType == PuzzleType.Radio && radio != null)
                {
                    Debug.Log("PUZZLE RADIO RESUELTO");
                    radio.PlayRadio();


                /*if (inventarioHotbar != null && inventarioHotbar.TieneObjetoSeleccionado(objetoRequerido))
                    {
                        inventarioHotbar.RemoveSelectedPiece(); // Quitamos la pieza del inventario
                        PlayRadio();
                    }
                    else
                    {
                    }*/
                }

            }
            else
            {
                //Signiffica que estamos en el puzzle 2
                if (puzzleType == PuzzleType.Cuadros)
                {
                    DialogController.Instance.ShowDialog($"La historia no es como crees.", 4f);
                }
            }

            StopAllCoroutines();
            //StartCoroutine(HideMessageAfterSeconds());
        }
    }

    public bool PuzzleFueResuelto()
    {
        return puzzleResuelto;
    }

    /*private IEnumerator HideMessageAfterSeconds()
    {
        yield return new WaitForSeconds(tiempoMensaje);
        resultadoTexto.gameObject.SetActive(false);
    }*/
}
