using System.Collections;
using UnityEngine;

/// <summary>
/// Controla la secuencia de despertar del protagonista al inicio del juego.
/// Incluye: despertar con mareo/movimiento de cámara, cierre lento de puerta, y diálogos iniciales.
/// </summary>
public class PlayerWakeUpSequence : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Door doorToClose; // La puerta que se cierra lentamente
    [SerializeField] private GameObject playerGameObject; // Para obtener PlayerController si no está asignado
    [SerializeField] private PlayerWakeUpEffects wakeUpEffects; // Efectos adicionales opcionales
    
    [Header("Configuración del Despertar")]
    [SerializeField] private float wakeUpDizzyDuration = 3f;
    [SerializeField] private float wakeUpDizzyIntensity = 0.6f;
    [SerializeField] private float delayBeforeDialog = 1f; // Pausa antes de los diálogos
    
    [Header("Configuración de la Puerta")]
    [SerializeField] private float doorCloseDelay = 0.5f; // Tiempo antes de cerrar la puerta
    
    [Header("Audio del Despertar")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioClip breathingClip; // Sonido de jadeo/respiración agitada
    [SerializeField] private AudioClip wakeUpGaspClip; // Sonido de despertar exaltado
    
    [Header("Diálogos Iniciales")]
    [SerializeField] private DialogData[] initialDialogs = new DialogData[]
    {
        new DialogData("Euri??... Hija??... Hay alguien en casa??", 4f),
        new DialogData("", 1f), // Pausa
        new DialogData("Juraría que alguien cerró la puerta...", 3f)
    };
    
    private bool hasTriggered = false;
    
    private void Awake()
    {
        // Buscar referencias automáticamente si no están asignadas
        if (playerController == null)
        {
            if (playerGameObject != null)
                playerController = playerGameObject.GetComponent<PlayerController>();
            else
                playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (playerAudioSource == null)
        {
            playerAudioSource = GetComponent<AudioSource>();
            if (playerAudioSource == null && playerController != null)
                playerAudioSource = playerController.GetComponent<AudioSource>();
        }
        
        if (wakeUpEffects == null)
        {
            wakeUpEffects = GetComponent<PlayerWakeUpEffects>();
            if (wakeUpEffects == null && playerController != null)
                wakeUpEffects = playerController.GetComponent<PlayerWakeUpEffects>();
        }
    }
    
    /// <summary>
    /// Inicia la secuencia completa de despertar
    /// </summary>
    public void StartWakeUpSequence()
    {
        if (hasTriggered)
        {
            Debug.LogWarning("[PlayerWakeUpSequence] La secuencia ya ha sido ejecutada");
            return;
        }
        
        hasTriggered = true;
        StartCoroutine(WakeUpSequenceCoroutine());
    }
    
    /// <summary>
    /// Corrutina principal que coordina toda la secuencia de despertar
    /// </summary>
    private IEnumerator WakeUpSequenceCoroutine()
    {
        Debug.Log("[PlayerWakeUpSequence] Iniciando secuencia de despertar");
        
        // 1. DESPERTAR DEL PROTAGONISTA
        yield return StartCoroutine(TriggerPlayerWakeUp());
        
        // 2. CERRAR PUERTA LENTAMENTE (en paralelo)
        if (doorToClose != null)
        {
            StartCoroutine(CloseDoorSlowly());
        }
        
        // 3. PEQUEÑA PAUSA ANTES DE DIÁLOGOS
        yield return new WaitForSeconds(delayBeforeDialog);
        
        // 4. DISPARAR DIÁLOGOS
        yield return StartCoroutine(TriggerInitialDialogs());
        
        Debug.Log("[PlayerWakeUpSequence] Secuencia de despertar completada");
    }
    
    /// <summary>
    /// Simula el despertar del protagonista con mareo y sonidos
    /// </summary>
    private IEnumerator TriggerPlayerWakeUp()
    {
        Debug.Log("[PlayerWakeUpSequence] Protagonista despertando...");
        
        // Sonido de despertar exaltado (jadeo inicial)
        if (playerAudioSource != null && wakeUpGaspClip != null)
        {
            playerAudioSource.PlayOneShot(wakeUpGaspClip);
        }
        
        // Pequeña pausa para que se escuche el sonido inicial
        yield return new WaitForSeconds(0.3f);
        
        // Activar mareo para simular desorientación al despertar
        if (playerController != null)
        {
            playerController.TriggerDizziness(wakeUpDizzyDuration, wakeUpDizzyIntensity);
            Debug.Log($"[PlayerWakeUpSequence] Mareo activado por {wakeUpDizzyDuration}s con intensidad {wakeUpDizzyIntensity}");
        }
        
        // Activar efectos adicionales de despertar si están disponibles
        if (wakeUpEffects != null)
        {
            wakeUpEffects.StartWakeUpEffects();
            Debug.Log("[PlayerWakeUpSequence] Efectos adicionales de despertar activados");
        }
        
        // Respiración agitada durante el mareo
        if (playerAudioSource != null && breathingClip != null)
        {
            playerAudioSource.PlayOneShot(breathingClip);
        }
        
        // Esperar un poco para que el mareo tenga efecto antes de continuar
        yield return new WaitForSeconds(1.5f);
    }
    
    /// <summary>
    /// Cierra la puerta lentamente usando el sistema de puertas existente
    /// </summary>
    private IEnumerator CloseDoorSlowly()
    {
        // Pequeña pausa antes de cerrar la puerta
        yield return new WaitForSeconds(doorCloseDelay);
        
        if (doorToClose != null)
        {
            Debug.Log("[PlayerWakeUpSequence] Cerrando puerta lentamente...");
            
            // Cambiar tipo a SlowClosing y ejecutar cierre lento
            doorToClose.SetType(TypeDoorInteract.SlowClosing);
            doorToClose.StartSlowClosing();
            
            Debug.Log($"[PlayerWakeUpSequence] Puerta iniciando cierre lento (duración: {doorToClose.SlowCloseDuration}s)");
        }
        else
        {
            Debug.LogWarning("[PlayerWakeUpSequence] No hay puerta asignada para cerrar");
        }
    }
    
    /// <summary>
    /// Dispara la secuencia de diálogos iniciales
    /// </summary>
    private IEnumerator TriggerInitialDialogs()
    {
        if (DialogController.Instance == null)
        {
            Debug.LogError("[PlayerWakeUpSequence] DialogController no encontrado");
            yield break;
        }
        
        Debug.Log("[PlayerWakeUpSequence] Iniciando diálogos iniciales...");
        
        // Usar el sistema de diálogos existente
        DialogController.Instance.ShowDialogSequence(initialDialogs);
        
        // Calcular duración total para esperar
        float totalDuration = 0f;
        foreach (var dialog in initialDialogs)
        {
            totalDuration += dialog.duration;
        }
        
        yield return new WaitForSeconds(totalDuration);
        
        Debug.Log("[PlayerWakeUpSequence] Diálogos iniciales completados");
    }
    
    /// <summary>
    /// Método público para configurar diálogos desde el inspector o código
    /// </summary>
    public void SetInitialDialogs(DialogData[] dialogs)
    {
        initialDialogs = dialogs;
    }
    
    /// <summary>
    /// Permite resetear la secuencia para testing
    /// </summary>
    [ContextMenu("Reset Wake Up Sequence")]
    public void ResetSequence()
    {
        hasTriggered = false;
        Debug.Log("[PlayerWakeUpSequence] Secuencia reseteada para testing");
    }
    
    /// <summary>
    /// Configurar la puerta a cerrar
    /// </summary>
    public void SetDoorToClose(Door door)
    {
        doorToClose = door;
    }
    
    private void OnValidate()
    {
        // Validaciones en el editor
        if (wakeUpDizzyIntensity < 0f) wakeUpDizzyIntensity = 0f;
        if (wakeUpDizzyIntensity > 1f) wakeUpDizzyIntensity = 1f;
        if (wakeUpDizzyDuration < 0f) wakeUpDizzyDuration = 0f;
    }
}
