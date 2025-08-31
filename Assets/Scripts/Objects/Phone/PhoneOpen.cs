using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(AudioSource))]
public class PhoneOpen : MonoBehaviour
{
    private PlayerController playerController;
    
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip callClip; // Clip de la llamada/conversación
    [SerializeField] private AudioClip hangupClip; // Clip de colgar la llamada
    
    [Header("Phone Call Settings")]
    [Tooltip("Secuencia de diálogos de la llamada telefónica")]
    [SerializeField] private DialogMessage[] callDialogSequence;
    
    [Header("Camera Settings")]
    [Tooltip("Priority to set for the phone camera during the call.")]
    public int phoneCameraPriority = 20;
    [Tooltip("Priority of the player camera (to restore after call).")]
    public int playerCameraPriority = 10;
    
    [Header("Controller Reference")]
    [SerializeField] private PhoneController phoneController; // Referencia al controlador principal
    [SerializeField] private GameObject phoneCloseGameObject; // GameObject del teléfono cerrado para reactivar
    
    private CinemachineVirtualCamera phoneCamera;
    private CinemachineVirtualCamera playerCamera;
    private CinemachineBrain cinemachineBrain;
    private AudioSource audioSource;
    private bool isCalling = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Buscar CinemachineBrain en la Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.LogError("CinemachineBrain not found on Main Camera.");
            }
        }
        
        // Busca la cámara virtual con el tag "CameraPhone" en los hijos
        foreach (var cam in GetComponentsInChildren<CinemachineVirtualCamera>(true))
        {
            if (cam.CompareTag("CameraPhone"))
            {
                phoneCamera = cam;
                break;
            }
        }
        if (phoneCamera == null)
        {
            Debug.LogError($"No CinemachineVirtualCamera with tag 'CameraPhone' found in {gameObject.name}.");
        }

        // Busca la cámara del player por tag "PlayerVirtualCamera"
        GameObject playerCameraObj = GameObject.FindGameObjectWithTag("PlayerVirtualCamera");
        if (playerCameraObj != null)
        {
            playerCamera = playerCameraObj.GetComponent<CinemachineVirtualCamera>();
            if (playerCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera component not found on CameraPlayer object.");
            }
        }
        else
        {
            Debug.LogError("Player camera object with tag 'CameraPlayer' not found.");
        }

        // Busca el PlayerController por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController component not found on Player object.");
            }
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found.");
        }
    }

    private void Start()
    {
        ValidateReferences();
        
        // Este GameObject debería estar desactivado al inicio
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fuerza un corte instantáneo en Cinemachine
    /// </summary>
    private void ForceInstantCameraCut()
    {
        if (cinemachineBrain != null)
        {
            // Temporalmente cambiar el blend a Cut, luego restaurar
            var originalBlend = cinemachineBrain.m_DefaultBlend;
            var cutBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
            cinemachineBrain.m_DefaultBlend = cutBlend;
            
            // Restaurar el blend original después de un frame
            StartCoroutine(RestoreBlendAfterFrame(originalBlend));
        }
    }

    /// <summary>
    /// Restaura el blend original después de un frame
    /// </summary>
    private IEnumerator RestoreBlendAfterFrame(CinemachineBlendDefinition originalBlend)
    {
        yield return null; // Esperar un frame
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend = originalBlend;
        }
    }
    public void StartCall()
    {
        if (isCalling) return;
        
        
        // Cambio instantáneo de cámara
        if (phoneCamera != null)
        {
            phoneCamera.Priority = phoneCameraPriority;
        }
        
        if (playerCamera != null)
        {
            playerCamera.Priority = 0; // Desactivar cámara del player
        }
        
        StartCoroutine(PhoneCallRoutine());
    }

    /// <summary>
    /// Inicia la llamada con Fade In (llamado desde PhoneClose)
    /// </summary>
    public void StartCallWithFadeIn()
    {
        if (isCalling) return;
        
        StartCoroutine(StartCallWithFadeInRoutine());
    }

    /// <summary>
    /// Configura e inicia la llamada telefónica con parámetros específicos
    /// </summary>
    public void StartCallWithParameters(AudioClip phoneCallClip, DialogMessage[] phoneDialogs)
    {
        if (isCalling) return;
        
        // Configurar los parámetros de la llamada
        if (phoneCallClip != null)
        {
            callClip = phoneCallClip;
        }
        
        if (phoneDialogs != null && phoneDialogs.Length > 0)
        {
            callDialogSequence = phoneDialogs;
        }
        
        StartCoroutine(StartCallWithFadeInRoutine());
    }

    /// <summary>
    /// Corrutina que maneja el inicio de llamada con Fade In
    /// </summary>
    private IEnumerator StartCallWithFadeInRoutine()
    {
        // 1. Forzar corte instantáneo de cámara
        ForceInstantCameraCut();
        
        // 2. Cambio instantáneo de cámara
        if (phoneCamera != null)
        {
            phoneCamera.Priority = phoneCameraPriority;
        }
        
        if (playerCamera != null)
        {
            playerCamera.Priority = 0; // Desactivar cámara del player
        }
        
        // 3. Esperar un frame para asegurar el cambio de cámara
        yield return null;
        
        // 4. Fade in
        yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(0.3f));
        
        // 5. Iniciar la llamada
        StartCoroutine(PhoneCallRoutine());
    }

    /// <summary>
    /// Corrutina principal que maneja la llamada
    /// </summary>
    private IEnumerator PhoneCallRoutine()
    {
        if (isCalling) yield break;

        isCalling = true;
        
        // Ya no desactivamos controles aquí - se hace en PhoneClose.AnswerCall()
        
        // Reproducir sonido de la llamada si existe
        if (audioSource != null && callClip != null)
        {
            audioSource.clip = callClip;
            audioSource.loop = false;
            audioSource.Play();
        }

        // Esperar un poco antes de empezar los diálogos
        yield return new WaitForSeconds(0.5f);

        // Muestra la secuencia de diálogos y espera a que termine
        if (callDialogSequence != null && callDialogSequence.Length > 0)
        {
            yield return StartCoroutine(ShowDialogSequenceAndWait());
        }

        // Termina la llamada
        EndCall();
    }

    /// <summary>
    /// Termina la llamada y restaura el estado normal
    /// </summary>
    private void EndCall()
    {
        
        // Reproducir sonido de colgar
        if (audioSource != null && hangupClip != null)
        {
            audioSource.clip = hangupClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        StartCoroutine(EndCallRoutine());
    }

    /// <summary>
    /// Corrutina que maneja el final de la llamada
    /// </summary>
    private IEnumerator EndCallRoutine()
    {
        // Esperar a que termine el sonido de colgar
        if (hangupClip != null)
        {
            yield return new WaitForSeconds(hangupClip.length);
        }
        
        
        // 1. Fade out
        yield return StartCoroutine(FadeManager.Instance.FadeOutCoroutine(0.3f));
        
        // 2. Forzar corte instantáneo para el regreso
        ForceInstantCameraCut();
        
        // 3. Cambio instantáneo de cámara de vuelta al player
        if (playerCamera != null)
        {
            playerCamera.Priority = playerCameraPriority; // Restaurar cámara del player
        }
        
        if (phoneCamera != null)
        {
            phoneCamera.Priority = 0; // Desactivar cámara del teléfono
        }
        
        // 4. Esperar un frame para asegurar el cambio de cámara
        yield return null;
        
        // 3. Los controles se reactivarán en PhoneClose.OnHangUp() después del Fade In
        
        // 4. Activar teléfono cerrado ANTES de desactivar este
        if (phoneCloseGameObject != null)
        {
            phoneCloseGameObject.SetActive(true);
            
            // Llamar a la función de colgado en el teléfono cerrado
            PhoneClose phoneCloseScript = phoneCloseGameObject.GetComponent<PhoneClose>();
            if (phoneCloseScript != null)
            {
                phoneCloseScript.OnHangUp();
            }
        }
        
        // 5. Notificar al controller que la llamada terminó
        if (phoneController != null)
        {
            phoneController.OnCallCompleted();
        }
        
        isCalling = false;
        
        // 6. Desactivar este GameObject AL FINAL
        gameObject.SetActive(false);
        
    }

    /// <summary>
    /// Corrutina auxiliar para esperar a que termine la secuencia de diálogos
    /// </summary>
    private IEnumerator ShowDialogSequenceAndWait()
    {
        bool finished = false;
        yield return StartCoroutine(DialogSequenceCoroutine(() => finished = true));
        while (!finished) yield return null;
    }

    /// <summary>
    /// Corrutina que ejecuta la secuencia y llama al callback al terminar
    /// </summary>
    private IEnumerator DialogSequenceCoroutine(System.Action onComplete)
    {
        DialogController.Instance.ShowDialogSequence(callDialogSequence);
        float totalDuration = 0f;
        foreach (var msg in callDialogSequence)
            totalDuration += msg.duration;
        yield return new WaitForSeconds(totalDuration);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Valida las referencias necesarias
    /// </summary>
    private void ValidateReferences()
    {
        if (callClip == null)
            Debug.LogWarning("[PhoneOpen] Call clip no asignado");
        
        if (hangupClip == null)
            Debug.LogWarning("[PhoneOpen] Hangup clip no asignado");
        
        if (callDialogSequence == null || callDialogSequence.Length == 0)
            Debug.LogWarning("[PhoneOpen] Secuencia de diálogos no asignada");
        
        if (phoneController == null)
            Debug.LogWarning("[PhoneOpen] PhoneController no asignado");
        
        if (phoneCamera == null)
            Debug.LogWarning("[PhoneOpen] Phone camera no encontrada");
        
        if (playerCamera == null)
            Debug.LogWarning("[PhoneOpen] Player camera no encontrada");
        
        if (phoneCloseGameObject == null)
            Debug.LogWarning("[PhoneOpen] PhoneClose GameObject no asignado");
    }
}