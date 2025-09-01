using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PhoneClose : ObjectInteract
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioClip ringClip; // Clip del teléfono sonando
    [SerializeField] private AudioClip pickupClip; // Clip de atender la llamada
    
    [Header("Phone Objects Reference")]
    [SerializeField] private PhoneOpen phoneOpenScript; // Referencia al script del teléfono abierto
    [SerializeField] private GameObject phoneOpenGameObject; // GameObject del teléfono abierto
    
    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController; // Referencia opcional al PlayerController

    [Header("Call Controller")]
    [SerializeField] private PhoneController phoneController; // Referencia al controlador del teléfono

    private AudioSource audioSource;
    private bool isRinging = false;
    private bool canInteract = false;

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
        
        // Solo mostrar outline si está sonando y se puede interactuar
        if (!isRinging || !canInteract) 
        {
            return;
        }
        
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        if (!canInteract) return;

        base.OnHoverExit();
    }

    public override void OnInteract()
    {        
        if (!isRinging || !canInteract) 
        {
            return;
        }

        AnswerCall();
    }

    /// <summary>
    /// Hace que el teléfono empiece a sonar
    /// </summary>
    public void StartRinging()
    {
        if (isRinging) return;
        
        isRinging = true;
        canInteract = true;
        
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
        canInteract = false;
        
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
        
        StartCoroutine(TransitionToPhoneOpen());
    }

    /// <summary>
    /// Corrutina que maneja la transición al teléfono abierto
    /// </summary>
    private IEnumerator TransitionToPhoneOpen()
    {
        
        // 1. Fade out
        yield return StartCoroutine(FadeManager.Instance.FadeOutCoroutine(0.3f));
        
        // 2. Activar el teléfono abierto ANTES de desactivar este
        if (phoneOpenGameObject != null)
            phoneOpenGameObject.SetActive(true);
        
        // 3. Llamar a la función del teléfono abierto para continuar
        if (phoneOpenScript != null)
        {
            // Obtener parámetros del PhoneController si está disponible
            if (phoneController != null)
            {
                AudioClip phoneCallClip = phoneController.GetPhoneCallClip();
                DialogMessage[] phoneDialogs = phoneController.GetDialogsToUse();
                phoneOpenScript.StartCallWithParameters(phoneCallClip, phoneDialogs);
            }
            else
            {
                // Fallback al método anterior si no hay PhoneController
                phoneOpenScript.StartCallWithFadeIn();
            }
        }
        
        // 4. Desactivar este GameObject AL FINAL
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Función llamada cuando se cuelga desde PhoneOpen
    /// </summary>
    public void OnHangUp()
    {
        StartCoroutine(HangUpRoutine());
    }

    /// <summary>
    /// Corrutina que maneja el colgado de la llamada
    /// </summary>
    private IEnumerator HangUpRoutine()
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

        if (phoneController != null)
        {
            phoneController.FinishCall();
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
        
        if (phoneOpenScript == null)
            Debug.LogWarning("[PhoneClose] PhoneOpen script no asignado");
        
        if (phoneOpenGameObject == null)
            Debug.LogWarning("[PhoneClose] PhoneOpen GameObject no asignado");
        
        if (playerController == null)
            Debug.LogWarning("[PhoneClose] PlayerController no encontrado - controles no se desactivarán");
    }
}
