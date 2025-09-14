using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador específico para el primer loop de llamada en el pasillo 2.
/// Al llegar al pasillo 2 suena el teléfono, la puerta se cierra y si el jugador 
/// ignora la llamada no se abrirá.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Call_Loop_01 : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Door doorToClose; // Puerta que se cerrará y luego se abrirá
    [SerializeField] private PhoneClose phoneClose; // Script del teléfono cerrado
    
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip phoneCallClip; // Clip de audio de la llamada telefónica
    
    [Header("Dialog Configuration")]
    [Tooltip("ScriptableObject con mensajes predefinidos")]
    [SerializeField] private PhoneMessages phoneMessages;
    
    [Header("Configuración")]
    [SerializeField] private float ringDuration = 10f; // Duración que suena el teléfono
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

        if(doorToClose != null)
        {
            // Asegurar que la puerta empieza en modo cerrado
            doorToClose.SetType(TypeDoorInteract.Close);
        }
    }

    private void Start()
    {
        ValidateReferences();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ya se completó la llamada, no hacer nada
        if (callCompleted)
        {
            Debug.Log("[Call_Loop_01] La llamada ya fue completada. El jugador puede pasar.");
            return;
        }

        // Verificar si es el player (asumiendo que tiene tag "Player")
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return; // Si solo se activa una vez y ya se activó
            
            hasTriggered = true;
            StartCallSequence();
        }
    }

    /// <summary>
    /// Inicia la secuencia completa: cerrar puerta -> sonar teléfono
    /// </summary>
    public void StartCallSequence()
    {
        if (phoneSequenceCoroutine != null)
        {
            StopCoroutine(phoneSequenceCoroutine);
        }
        
        phoneSequenceCoroutine = StartCoroutine(CallSequenceCoroutine());
    }

    /// <summary>
    /// Detiene la secuencia del teléfono
    /// </summary>
    public void StopCallSequence()
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
    /// Llamado por PhoneOpen cuando la llamada se completa exitosamente
    /// </summary>
    public void OnCallCompleted()
    {
        callCompleted = true;
        Debug.Log("[Call_Loop_01] Llamada completada. Ejecutando secuencia post-llamada...");
        
        StartCoroutine(PostCallSequence());
    }

    /// <summary>
    /// Obtiene los diálogos de la llamada específicos para este loop
    /// </summary>
    public DialogData[] GetCallDialogs()
    {
        // Si hay PhoneMessages asignado, usar esos diálogos
        if (phoneMessages != null)
        {
            return phoneMessages.GetPrimerLoopMessages();
        }
        
        // Si no hay PhoneMessages, usar los diálogos hardcodeados del guión
        return GetHardcodedCallDialogs();
    }

    /// <summary>
    /// Obtiene el clip de audio de la llamada
    /// </summary>
    public AudioClip GetPhoneCallClip()
    {
        return phoneCallClip;
    }

    /// <summary>
    /// Diálogos hardcodeados basados en el guión proporcionado
    /// </summary>
    private DialogData[] GetHardcodedCallDialogs()
    {
        return new DialogData[]
        {
            // Eurídice
            new DialogData("Perdón por llamar a estas horas (dice suspirando)", 4f),
            
            // Protagonista (intenta hablar)
            new DialogData("Euri, yo...", 2f),
            
            // Eurídice continúa e interrumpe
            new DialogData("También perdón por no avisar que nos fuimos.", 4f),
            
            // Protagonista intenta interrumpir
            new DialogData("Euri, hola ¿Me escuchas?", 3f),
            
            // Eurídice continúa
            new DialogData("Es que te vimos durmiendo y no quisimos despertarte.", 5f),
            new DialogData("Hoy tampoco vamos a pasar la noche en casa,", 4f),
            new DialogData("creo que todavía necesitamos… Todavía necesito algo más de tiempo.", 6f),
            
            // Alice interrumpe
            new DialogData("¿Estás hablando con papi?", 3f),
            
            // Eurídice a Alice y luego al protagonista
            new DialogData("Si hija, estoy hablando con papi.", 3f),
            new DialogData("Anda a acostarte que ya es tarde corazón.", 4f),
            new DialogData("Perdón Jorge, se por lo que estás pasando y todo lo que has hecho pero yo…", 6f),
            
            // La llamada se corta
            new DialogData("", 2f), // Silencio abrupto
            
            // Protagonista después de que se corte
            new DialogData("Euri? Alice? Hola?", 3f)
        };
    }

    /// <summary>
    /// Diálogo final del protagonista después de que se corten las luces
    /// </summary>
    private DialogData[] GetFinalDialogs()
    {
        return new DialogData[]
        {
            new DialogData("Mierda, voy a tener que revisar la caja eléctrica…", 4f),
            new DialogData("aunque no quiera volver a bajar allí, detesto ese lugar.", 5f)
        };
    }

    /// <summary>
    /// Corrutina principal que maneja toda la secuencia inicial
    /// </summary>
    private IEnumerator CallSequenceCoroutine()
    {
        Debug.Log("[Call_Loop_01] Iniciando secuencia de llamada del pasillo 2");
        
        // 1. Activar el cierre de la puerta
        /*if (door != null)
        {
            //Debug.Log("[Call_Loop_01] Cerrando puerta del hall de entrada...");
            //door.StartSlowClosing(); // Usar el método de cierre lento
            
            // Esperar a que termine el cierre de la puerta
            //yield return new WaitForSeconds(door.SlowCloseDuration);
            Debug.Log("[Call_Loop_01] Puerta cerrada completamente");
        }*/
        
        // 2. Empezar a sonar el teléfono
        if (phoneClose != null)
        {
            phoneClose.StartRinging();
            Debug.Log("[Call_Loop_01] Teléfono empezó a sonar");
        }
        
        // 3. Esperar la duración configurada del ring (o hasta que se conteste)
        yield return new WaitForSeconds(ringDuration);
        
        // 4. Si todavía está sonando después del tiempo, detener automáticamente
        if (phoneClose != null)
        {
            phoneClose.StopRinging();
            Debug.Log("[Call_Loop_01] Tiempo de ring agotado - teléfono dejó de sonar");
        }
    }

    /// <summary>
    /// Secuencia que se ejecuta después de completar la llamada
    /// </summary>
    private IEnumerator PostCallSequence()
    {
        Debug.Log("[Call_Loop_01] Iniciando secuencia post-llamada");
        
        // 1. TODO: Apagar las luces
        Debug.Log("[Call_Loop_01] TODO: Apagar las luces");
        // ApplyLightEffects();
        
        // 2. Mostrar diálogo final del protagonista
        DialogData[] finalDialogs = GetFinalDialogs();
        if (DialogController.Instance != null)
        {
            DialogController.Instance.ShowDialogSequence(finalDialogs);
            
            // Esperar a que terminen los diálogos
            float totalDialogTime = 0f;
            foreach (var dialog in finalDialogs)
            {
                totalDialogTime += dialog.duration;
            }
            yield return new WaitForSeconds(totalDialogTime);
        }
        
        // 3. Abrir la puerta para permitir el paso
        if (doorToClose != null)
        {
            doorToClose.SetType(TypeDoorInteract.OpenAndClose);
            Debug.Log("[Call_Loop_01] Puerta configurada para permitir interacción");
        }
        
        Debug.Log("[Call_Loop_01] Secuencia post-llamada completada");
    }

    /// <summary>
    /// TODO: Implementar efectos de luces apagándose
    /// </summary>
    private void ApplyLightEffects()
    {
        // TODO: Implementar lógica para apagar las luces
        // Esto podría incluir:
        // - Buscar todas las luces en la escena
        // - Animación de parpadeo
        // - Apagado gradual o súbito
        // - Efectos de sonido de electricidad
        Debug.Log("[Call_Loop_01] TODO: Implementar efectos de apagado de luces");
    }

    /// <summary>
    /// Método público para ser llamado desde PhoneOpen cuando termine la llamada
    /// </summary>
    public void FinishCall()
    {
        // Solo ejecutar la secuencia post-llamada si había una llamada activa
        // (si el trigger se había activado y había una llamada en progreso)
        if (hasTriggered)
        {
            OnCallCompleted();
        }
        else
        {
            // Si no había llamada previa (interacción directa sin llamada entrante),
            // no hacer nada especial - el teléfono simplemente se cuelga
            Debug.Log("[Call_Loop_01] Llamada colgada sin contexto de llamada entrante");
        }
    }

    /// <summary>
    /// Valida que todas las referencias estén asignadas
    /// </summary>
    private void ValidateReferences()
    {
        if (doorToClose == null)
            Debug.LogWarning("[Call_Loop_01] Puerta no asignada");

        if (phoneClose == null)
            Debug.LogWarning("[Call_Loop_01] PhoneClose script no asignado");

        if (phoneCallClip == null)
            Debug.LogWarning($"[Call_Loop_01] {name}: No se asignó AudioClip para la llamada telefónica");
        
        // Validar que hay diálogos disponibles
        bool hasDialogs = phoneMessages != null;
        
        if (hasDialogs)
        {
            Debug.Log($"[Call_Loop_01] {name}: Usando PhoneMessages predefinidos");
        }
        else
        {
            Debug.Log($"[Call_Loop_01] {name}: Usando diálogos hardcodeados del guión");
        }
    }

    /// <summary>
    /// Resetea el trigger para que pueda activarse de nuevo (para testing)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        callCompleted = false;
        Debug.Log("[Call_Loop_01] Trigger reseteado");
    }

    /// <summary>
    /// Propiedad para verificar si la llamada fue completada
    /// </summary>
    public bool IsCallCompleted => callCompleted;

    /// <summary>
    /// Propiedad para obtener la duración del ring configurada
    /// </summary>
    public float RingDuration => ringDuration;

    // Para debugging en el editor
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = callCompleted ? Color.green : (hasTriggered ? Color.red : Color.yellow);
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}
