using System.Collections;
using UnityEngine;

/// <summary>
/// Controller de la radio que maneja triggers para activar la reproducción automática.
/// Funciona como un trigger que detecta cuando el jugador entra en un collider y
/// reproduce la radio con parámetros configurables.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RadioController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Radio radio; // Referencia a la radio que se activará

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip radioClip; // Clip de audio que se reproducirá
    [SerializeField] private float audioDuration = 121f; // Duración del audio de la radio

    [Header("Dialog Configuration")]
    [Tooltip("ScriptableObject con mensajes predefinidos (opcional)")]
    [SerializeField] private RadioMessages radioMessages;

    [Tooltip("Tipo de loop de diálogos a usar")]
    [SerializeField] private RadioLoopType loopType = RadioLoopType.PrimerLoop;

    [Space]
    [Tooltip("Secuencia de diálogos personalizada (solo si radioMessages está vacío)")]
    [SerializeField] private DialogData[] radioDialogSequence;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true; // Solo se activa una vez

    private bool hasTriggered = false;
    private bool radioPlayed = false; // Bandera para evitar múltiples reproducciones

    private void Awake()
    {
        // Asegurar que el collider es trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        ValidateReferences();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya se reprodujo la radio, no hacer nada
        if (radioPlayed)
        {
            Debug.Log("[RadioController] La radio ya fue reproducida anteriormente.");
            return;
        }

        // Verificar si es el player (asumiendo que tiene tag "Player")
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return; // Si solo se activa una vez y ya se activó

            hasTriggered = true;
            StartRadioSequence();
        }
    }

    /// <summary>
    /// Inicia la secuencia de reproducción de la radio
    /// </summary>
    public void StartRadioSequence()
    {
        if (radio != null)
        {
            Debug.Log("[RadioController] Iniciando secuencia de radio...");

            // Obtener los diálogos a usar
            DialogData[] dialogsToUse = GetDialogsToUse();

            radio.PlayRadioWithParameters(radioClip, audioDuration, dialogsToUse);

            Debug.Log("[RadioController] " + loopType);
            Debug.Log("[RadioController] Secuencia de radio iniciada. " + dialogsToUse.Length + " diálogos.");
            radioPlayed = true;
        }
        else
        {
            Debug.LogError("[RadioController] No hay referencia a la radio asignada.");
        }
    }

    /// <summary>
    /// Obtiene los diálogos a usar según la configuración
    /// </summary>
    private DialogData[] GetDialogsToUse()
    {
        // Si hay RadioMessages asignado, usar los predefinidos
        if (radioMessages != null)
        {
            switch (loopType)
            {
                case RadioLoopType.PrimerLoop:
                    return radioMessages.GetPrimerLoopMessages();
                case RadioLoopType.SegundoLoop:
                    return radioMessages.GetSegundoLoopMessages();
                default:
                    return radioMessages.GetPrimerLoopMessages();
            }
        }
        
        // Si no hay RadioMessages, usar la secuencia personalizada
        return radioDialogSequence;
    }
    
    /// <summary>
    /// Resetea el trigger para que pueda activarse de nuevo (para testing)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        radioPlayed = false;
        Debug.Log("[RadioController] Trigger reseteado.");
    }
    
    /// <summary>
    /// Valida que todas las referencias estén asignadas
    /// </summary>
    private void ValidateReferences()
    {
        if (radio == null)
        {
            Debug.LogError($"[RadioController] {name}: Falta asignar la referencia a Radio");
        }
        
        if (radioClip == null)
        {
            Debug.LogWarning($"[RadioController] {name}: No se asignó AudioClip para la radio");
        }
        
        // Validar que hay diálogos disponibles
        bool hasDialogs = false;
        if (radioMessages != null)
        {
            hasDialogs = true;
            Debug.Log($"[RadioController] {name}: Usando RadioMessages predefinidos - {loopType}");
        }
        else if (radioDialogSequence != null && radioDialogSequence.Length > 0)
        {
            hasDialogs = true;
            Debug.Log($"[RadioController] {name}: Usando diálogos personalizados ({radioDialogSequence.Length} mensajes)");
        }
        
        if (!hasDialogs)
        {
            Debug.LogWarning($"[RadioController] {name}: No se asignaron diálogos (ni RadioMessages ni radioDialogSequence)");
        }
    }
}