using System.Collections;
using UnityEngine;
using DearVR;

/// <summary>
/// Script de Radio que reproduce audio espacial y diálogos usando DearVR.
/// Funciona como ejecutor, recibiendo parámetros desde RadioController.
/// Utiliza DearVRSource para audio espacializado de alta calidad.
/// </summary>
[RequireComponent(typeof(DearVRSource))]
public class Radio : MonoBehaviour
{
    [Header("Referencias DearVR")]
    [SerializeField] private DearVRSource dearVRSource; // Componente DearVR para audio espacial
    [SerializeField] private AudioSource audioSource; // AudioSource (requerido por DearVR)
    
    [Header("Configuración Radio")]
    [SerializeField] private bool useDearVRPlayback = true; // Usar DearVR para reproducción o AudioSource estándar
    
    private bool radioReproducida = false; // Bandera para evitar múltiples reproducciones

    private void Awake()
    {
        // Obtener referencias automáticamente si no están asignadas
        if (dearVRSource == null)
            dearVRSource = GetComponent<DearVRSource>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ValidateReferences();
    }

    /// <summary>
    /// Reproduce la radio con los parámetros proporcionados por el controller
    /// </summary>
    /// <param name="audioClip">Clip de audio a reproducir</param>
    /// <param name="audioDuration">Duración del audio</param>
    /// <param name="dialogSequence">Secuencia de diálogos</param>
    /// <param name="playerController">Referencia al PlayerController</param>
    public void PlayRadioWithParameters(AudioClip audioClip, float audioDuration, DialogMessage[] dialogSequence)
    {
        if (radioReproducida)
        {
            Debug.Log("[Radio] La radio ya fue reproducida anteriormente.");
            return;
        }

        Debug.Log("[Radio] Iniciando reproducción de radio con DearVR...");
        StartCoroutine(PlayRadioCoroutine(audioClip, audioDuration, dialogSequence));
    }

    /// <summary>
    /// Método simplificado para reproducir la radio (mantiene compatibilidad)
    /// </summary>
    public void PlayRadio()
    {
        Debug.Log("[Radio] PlayRadio() llamado sin parámetros - usar PlayRadio con parámetros desde RadioController");
    }

    private IEnumerator PlayRadioCoroutine(AudioClip audioClip, float audioDuration, DialogMessage[] dialogSequence)
    {
        // Configurar el clip de audio
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            Debug.Log($"[Radio] Configurando clip: {audioClip.name}");
        }
        else
        {
            Debug.LogWarning("[Radio] No se proporcionó AudioClip, usando clip actual del AudioSource");
        }

        radioReproducida = true;

        // Reproducir usando DearVR o AudioSource estándar
        if (useDearVRPlayback && dearVRSource != null)
        {
            Debug.Log("[Radio] Reproduciendo con DearVR espacializado");
            dearVRSource.DearVRPlay();
        }
        else
        {
            Debug.Log("[Radio] Reproduciendo con AudioSource estándar");
            audioSource.Play();
        }

        // Inicia la secuencia de diálogos en paralelo
        Coroutine dialogCoroutine = null;
        if (dialogSequence != null && dialogSequence.Length > 0)
        {
            dialogCoroutine = StartCoroutine(ShowDialogSequenceParallel(dialogSequence));
        }

        // Esperar la duración del audio
        yield return new WaitForSeconds(audioDuration);

        // Si la secuencia de diálogos sigue, la detenemos (opcional)
        if (dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
        }

        // Detener reproducción
        if (useDearVRPlayback && dearVRSource != null)
        {
            dearVRSource.DearVRStop();
        }
        else
        {
            audioSource.Stop();
        }
        
        Debug.Log("[Radio] Reproducción de radio completada.");

        // TODO: Agregar aquí lógica adicional post-reproducción si es necesaria
        // StartCoroutine(FadeManager.Instance.Oscurecer());
        // SceneController.Instance.LoadGameOverScene();
    }

    // Corrutina para mostrar la secuencia de diálogos en paralelo
    private IEnumerator ShowDialogSequenceParallel(DialogMessage[] dialogSequence)
    {
        Debug.Log("[Radio] Iniciando secuencia de diálogos paralela. " + dialogSequence.Length);
        DialogController.Instance.ShowDialogSequence(dialogSequence);
        // Calcula la duración total de los diálogos
        float totalDuration = 0f;
        foreach (var msg in dialogSequence)
            totalDuration += msg.duration;
        yield return new WaitForSeconds(totalDuration);
    }

    /// <summary>
    /// Resetea el estado de la radio (para testing)
    /// </summary>
    public void ResetRadio()
    {
        radioReproducida = false;
        
        // Detener reproducción en ambos sistemas
        if (useDearVRPlayback && dearVRSource != null)
        {
            dearVRSource.DearVRStop();
        }
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log("[Radio] Radio reseteada.");
    }

    /// <summary>
    /// Valida que las referencias necesarias estén asignadas
    /// </summary>
    private void ValidateReferences()
    {
        if (audioSource == null)
        {
            Debug.LogError($"[Radio] {name}: Falta asignar AudioSource");
        }
        
        if (dearVRSource == null)
        {
            Debug.LogError($"[Radio] {name}: Falta asignar DearVRSource. Agregue el componente DearVRSource al GameObject.");
        }
        else
        {
            Debug.Log($"[Radio] {name}: DearVRSource configurado correctamente");
        }
        
        // Verificar configuración de DearVR
        if (useDearVRPlayback && dearVRSource != null)
        {
            // Configurar DearVR para performance mode si es necesario
            if (dearVRSource.PerformanceMode)
            {
                Debug.Log($"[Radio] {name}: DearVR en Performance Mode - usar DearVRPlay()");
            }
        }
    }

    /// <summary>
    /// Configuraciones adicionales para optimizar DearVR para radio
    /// </summary>
    public void ConfigureForRadio()
    {
        if (dearVRSource != null)
        {
            // Configuraciones recomendadas para una radio
            dearVRSource.PerformanceMode = true; // Para mejor rendimiento
            dearVRSource.InternalReverb = true;  // Usar reverb interno
            dearVRSource.RoomPreset = DearVRSource.RoomList.Room_Medium; // Ambiente de habitación
            dearVRSource.DirectLevel = 0.0f;     // Nivel directo
            dearVRSource.ReflectionLevel = -10.0f; // Reflexiones moderadas
            dearVRSource.ReverbLevel = -15.0f;   // Reverb sutil
            
            Debug.Log($"[Radio] {name}: DearVR configurado para radio");
        }
    }

    /// <summary>
    /// Getter para verificar si la radio ya fue reproducida
    /// </summary>
    public bool IsRadioPlayed => radioReproducida;
}
