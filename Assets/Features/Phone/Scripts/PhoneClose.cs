using System.Collections;
using UnityEngine;

/// <summary>
/// Maneja la interacción con el teléfono en estado cerrado.
/// 
/// Funcionalidad:
/// - SIEMPRE permite interacción independientemente del estado
/// - Si está sonando (llamada entrante): reproduce la llamada del Call_Loop_01
/// - Si NO está sonando: reproduce tono de "sin contestar" (noAnswerToneClip)
/// 
/// Audio Clips:
/// - ringClip: Sonido del teléfono sonando (llamadas entrantes)
/// - pickupClip: Sonido de levantar el auricular 
/// - noAnswerToneClip: Tono que se escucha cuando no hay llamada entrante
/// 
/// Controladores:
/// - callController (Call_Loop_01): Maneja llamadas específicas del primer loop
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PhoneClose : ObjectInteract
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip ringClip; // Clip del teléfono sonando
    [SerializeField] private AudioClip pickupClip; // Clip de atender la llamada
    [SerializeField] private AudioClip noAnswerToneClip; // Clip de tono de llamada sin contestar
    
    [Header("Phone Objects Reference")]
    [SerializeField] private PhoneOpen phoneOpenScript; // Referencia al script del teléfono abierto
    [SerializeField] private GameObject phoneOpenGameObject; // GameObject del teléfono abierto
    
    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController; // Referencia opcional al PlayerController

    [Header("Call Controller")]
    [SerializeField] private Call_Loop_01 callController; // Referencia al controlador específico de llamadas

    private AudioSource audioSource;
    private bool isRinging = false;
    private bool canInteract = true; // Siempre se puede interactuar

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        
        // Buscar PlayerController si no está asignado
        if (playerController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }
    }

    private void Start()
    {
        ValidateReferences();
    }

    public override void OnHoverEnter()
    {
        // Siempre mostrar outline cuando se puede interactuar
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
    }

    public override void OnInteract()
    {        
        if (!canInteract) return;

        if (isRinging)
        {
            // Si está sonando, atender la llamada entrante
            AnswerCall();
        }
        else
        {
            // Si no está sonando, hacer una llamada saliente (sin contestar)
            MakeOutgoingCall();
        }
    }

    /// <summary>
    /// Hace que el teléfono empiece a sonar
    /// </summary>
    public void StartRinging()
    {
        if (isRinging) return;
        
        isRinging = true;
        // canInteract sigue siendo true siempre
        
        if (audioSource != null && ringClip != null)
        {
            audioSource.clip = ringClip;
            audioSource.loop = true;
            audioSource.Play();
        }
        
    }

    /// <summary>
    /// Detiene el sonido del teléfono
    /// </summary>
    public void StopRinging()
    {
        if (!isRinging) return;
        
        isRinging = false;
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
    }

    /// <summary>
    /// Atiende la llamada e inicia la transición
    /// </summary>
    private void AnswerCall()
    {
        
        StopRinging();
        // canInteract sigue siendo true para permitir futuras interacciones
        
        // Desactivar controles del player al atender
        if (playerController != null)
        {
            playerController.SetControlesActivos(false);
        }
        
        // Reproducir sonido de atender
        if (audioSource != null && pickupClip != null)
        {
            audioSource.clip = pickupClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        StartCoroutine(TransitionToPhoneOpen(true)); // true = llamada entrante
    }

    /// <summary>
    /// Realiza una llamada saliente (sin contestar)
    /// </summary>
    private void MakeOutgoingCall()
    {
        // Desactivar controles del player
        if (playerController != null)
        {
            playerController.SetControlesActivos(false);
        }
        
        // Reproducir sonido de atender (pickup)
        if (audioSource != null && pickupClip != null)
        {
            audioSource.clip = pickupClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        StartCoroutine(TransitionToPhoneOpen(false)); // false = llamada saliente
    }

    /// <summary>
    /// Corrutina que maneja la transición al teléfono abierto
    /// </summary>
    /// <param name="isIncomingCall">True si es una llamada entrante, False si es una llamada saliente</param>
    private IEnumerator TransitionToPhoneOpen(bool isIncomingCall = true)
    {
        
        // 1. Fade out
        yield return StartCoroutine(FadeManager.Instance.FadeOutCoroutine(0.3f));
        
        // 2. Activar el teléfono abierto ANTES de desactivar este
        if (phoneOpenGameObject != null)
            phoneOpenGameObject.SetActive(true);
        
        // 3. Llamar a la función del teléfono abierto para continuar
        if (phoneOpenScript != null)
        {
            if (isIncomingCall)
            {
                // Llamada entrante - usar parámetros del Call_Loop_01 si está disponible
                if (callController != null)
                {
                    AudioClip phoneCallClip = callController.GetPhoneCallClip();
                    DialogData[] phoneDialogs = callController.GetCallDialogs();
                    phoneOpenScript.StartCallWithParameters(phoneCallClip, phoneDialogs, true);
                }
                else
                {
                    // Fallback al método anterior si no hay callController
                    phoneOpenScript.StartCallWithFadeIn();
                }
            }
            else
            {
                // Llamada saliente - usar el clip de "sin contestar"
                phoneOpenScript.StartCallWithParameters(noAnswerToneClip, null, false);
            }
        }
        
        // 4. Desactivar este GameObject AL FINAL
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Función llamada cuando se cuelga desde PhoneOpen
    /// </summary>
    public void OnHangUp(bool wasWithCall)
    {
        StartCoroutine(HangUpRoutine(wasWithCall));
    }

    /// <summary>
    /// Corrutina que maneja el colgado de la llamada
    /// </summary>
    private IEnumerator HangUpRoutine(bool wasWithCall)
    {
        // Cambiar prioridad de cámara de vuelta al player
        // (Esto se maneja desde PhoneOpen antes de llamar OnHangUp)
        
        // Fade in para mostrar el teléfono cerrado de nuevo
        yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(0.3f));

        base.OnHoverExit();

        // Reactivar controles del player después del Fade In
        if (playerController != null)
        {
            playerController.SetControlesActivos(true);
        }

        if (callController != null)
        {
            callController.FinishCall(wasWithCall);
        }

    }

    /// <summary>
    /// Valida las referencias necesarias
    /// </summary>
    private void ValidateReferences()
    {
        if (ringClip == null)
            Debug.LogWarning("[PhoneClose] Ring clip no asignado");
        
        if (pickupClip == null)
            Debug.LogWarning("[PhoneClose] Pickup clip no asignado");
        
        if (noAnswerToneClip == null)
            Debug.LogWarning("[PhoneClose] No answer tone clip no asignado");
        
        if (phoneOpenScript == null)
            Debug.LogWarning("[PhoneClose] PhoneOpen script no asignado");
        
        if (phoneOpenGameObject == null)
            Debug.LogWarning("[PhoneClose] PhoneOpen GameObject no asignado");
        
        if (playerController == null)
            Debug.LogWarning("[PhoneClose] PlayerController no encontrado - controles no se desactivarán");
    }
}
