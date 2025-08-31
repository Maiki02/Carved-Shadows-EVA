using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PhoneController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Door door; // Puerta que se cerrará
    [SerializeField] private PhoneClose phoneClose; // Script del teléfono cerrado
    
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip phoneCallClip; // Clip de audio de la llamada telefónica
    
    [Header("Dialog Configuration")]
    [Tooltip("ScriptableObject con mensajes predefinidos (opcional)")]
    [SerializeField] private PhoneMessages phoneMessages;
    
    [Tooltip("Tipo de loop de diálogos a usar")]
    [SerializeField] private PhoneLoopType loopType = PhoneLoopType.PrimerLoop;
    
    [Space]
    [Tooltip("Secuencia de diálogos personalizada (solo si phoneMessages está vacío)")]
    [SerializeField] private DialogMessage[] phoneDialogSequence;
    
    [Header("Configuración")]
    [SerializeField] private float ringDuration = 5f; // Duración que suena el teléfono
    [SerializeField] private bool triggerOnce = true; // Solo se activa una vez
    
    private bool hasTriggered = false;
    private bool callCompleted = false; // Bandera para saber si ya se completó la llamada
    private Coroutine phoneSequenceCoroutine;

    private void Awake()
    {
        // Asegurar que el trigger esté configurado correctamente
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        
        // Validar referencias
        ValidateReferences();
    }

    private void Start()
    {
        // Validar referencias
        ValidateReferences();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya se completó la llamada, no hacer nada
        if (callCompleted)
        {
            Debug.Log("[PhoneController] La llamada ya fue completada. No se puede pasar más.");
            return;
        }

        // Verificar si es el player (asumiendo que tiene tag "Player")
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return; // Si solo se activa una vez y ya se activó
            
            hasTriggered = true;
            StartPhoneSequence();
        }
    }

    /// <summary>
    /// Inicia la secuencia completa: cerrar puerta -> sonar teléfono
    /// </summary>
    public void StartPhoneSequence()
    {
        if (phoneSequenceCoroutine != null)
        {
            StopCoroutine(phoneSequenceCoroutine);
        }
        
        phoneSequenceCoroutine = StartCoroutine(PhoneSequenceCoroutine());
    }

    /// <summary>
    /// Detiene la secuencia del teléfono
    /// </summary>
    public void StopPhoneSequence()
    {
        if (phoneSequenceCoroutine != null)
        {
            StopCoroutine(phoneSequenceCoroutine);
            phoneSequenceCoroutine = null;
        }
        
        if (phoneClose != null)
        {
            phoneClose.StopRinging();
        }
    }

    /// <summary>
    /// Llamado por PhoneOpen cuando la llamada se completa
    /// </summary>
    public void OnCallCompleted()
    {
        callCompleted = true;
        Debug.Log("[PhoneController] Llamada completada. Trigger desactivado permanentemente.");
        
        // TODO: Interactuar con la puerta (cambiar tipo o abrirla)
        if (door != null)
        {
            // Ejemplo: cambiar el tipo de puerta para que se pueda abrir
            door.SetType(TypeDoorInteract.OpenAndClose);
            Debug.Log("[PhoneController] TODO: Configurar puerta para permitir interacción");
        }
        
        // TODO: Otras acciones post-llamada aquí
        Debug.Log("[PhoneController] TODO: Realizar acciones adicionales después de la llamada");
    }

    /// <summary>
    /// Obtiene los diálogos a usar según la configuración
    /// </summary>
    public DialogMessage[] GetDialogsToUse()
    {
        // Si hay PhoneMessages asignado, usar los predefinidos
        if (phoneMessages != null)
        {
            switch (loopType)
            {
                case PhoneLoopType.PrimerLoop:
                    return phoneMessages.GetPrimerLoopMessages();
                case PhoneLoopType.SegundoLoop:
                    return phoneMessages.GetSegundoLoopMessages();
                default:
                    return phoneMessages.GetPrimerLoopMessages();
            }
        }
        
        // Si no hay PhoneMessages, usar la secuencia personalizada
        return phoneDialogSequence;
    }

    /// <summary>
    /// Obtiene el clip de audio de la llamada
    /// </summary>
    public AudioClip GetPhoneCallClip()
    {
        return phoneCallClip;
    }

    /// <summary>
    /// Corrutina principal que maneja toda la secuencia
    /// </summary>
    private IEnumerator PhoneSequenceCoroutine()
    {
        Debug.Log("[PhoneController] Iniciando secuencia del teléfono");
        
        // 1. Activar el cierre de la puerta
        if (door != null)
        {
            Debug.Log("[PhoneController] Cerrando puerta...");
            door.StartSlowClosing(); // Usar el método de cierre lento
            
            // Esperar a que termine el cierre de la puerta
            yield return new WaitForSeconds(door.SlowCloseDuration);
            Debug.Log("[PhoneController] Puerta cerrada completamente");
        }
        
        // 2. Empezar a sonar el teléfono
        if (phoneClose != null)
        {
            phoneClose.StartRinging();
            Debug.Log("[PhoneController] Teléfono empezó a sonar");
        }
        
        // 3. Esperar la duración configurada del ring (o hasta que se conteste)
        yield return new WaitForSeconds(ringDuration);
        
        // 4. Si todavía está sonando después del tiempo, detener automáticamente
        if (phoneClose != null)
        {
            phoneClose.StopRinging();
            Debug.Log("[PhoneController] Tiempo de ring agotado - teléfono dejó de sonar");
        }
    }

    public void FinishCall()
    {
        this.door.SetType(TypeDoorInteract.OpenAndClose);
    }

    /// <summary>
    /// Valida que todas las referencias estén asignadas
    /// </summary>
    private void ValidateReferences()
    {
        if (door == null)
            Debug.LogWarning("[PhoneController] Puerta no asignada");

        if (phoneClose == null)
            Debug.LogWarning("[PhoneController] PhoneClose script no asignado");

        if (phoneCallClip == null)
            Debug.LogWarning($"[PhoneController] {name}: No se asignó AudioClip para la llamada telefónica");
        
        // Validar que hay diálogos disponibles
        bool hasDialogs = false;
        if (phoneMessages != null)
        {
            hasDialogs = true;
            Debug.Log($"[PhoneController] {name}: Usando PhoneMessages predefinidos - {loopType}");
        }
        else if (phoneDialogSequence != null && phoneDialogSequence.Length > 0)
        {
            hasDialogs = true;
            Debug.Log($"[PhoneController] {name}: Usando diálogos personalizados ({phoneDialogSequence.Length} mensajes)");
        }
        
        if (!hasDialogs)
        {
            Debug.LogWarning($"[PhoneController] {name}: No se asignaron diálogos (ni PhoneMessages ni phoneDialogSequence)");
        }
    }

    /// <summary>
    /// Resetea el trigger para que pueda activarse de nuevo (para testing)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        callCompleted = false;
        Debug.Log("[PhoneController] Trigger reseteado");
    }

    /// <summary>
    /// Propiedad para verificar si la llamada fue completada
    /// </summary>
    public bool IsCallCompleted => callCompleted;

    // Para debugging en el editor
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = callCompleted ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
