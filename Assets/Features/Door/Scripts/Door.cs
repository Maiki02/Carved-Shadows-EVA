
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Door : ObjectInteract
{
    [Header("Knocking Loop")]
    [SerializeField] private float knockingInterval = 3f;
    private Coroutine knockingLoopCoroutine;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;
    [SerializeField] private AudioClip knockClip;
    [SerializeField] private AudioClip slowCloseClip;
    [SerializeField] private AudioClip fastCloseClip;
    [SerializeField] private AudioClip lockedDoorClip;

    [Header("Audio Mixer Groups")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup fastCloseMixer;

    [Header("Closed Door Configuration")]
    [SerializeField] private ClosedDoorDialogData closedDoorDialogData;

    private AudioSource audioSource;
    private UnityEngine.Audio.AudioMixerGroup originalMixerGroup;

    [Header("Tipo de acción sobre la puerta")]
    [SerializeField] private TypeDoorInteract type = TypeDoorInteract.None;

    private bool isDoorOpen = false;
    [Header("Configuración de apertura por rotación")]
    [SerializeField] private float openDegreesY = 110f;
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float knockAmount = 4f;
    [SerializeField] private float knockSpeed = 5f;

    [Header("Configuración de cierre lento")]
    [SerializeField] private float slowCloseDuration = 3f;
    [SerializeField] private float initialOpenDegrees = 110f;

    [Header("Configuración de cierre rápido")]
    [SerializeField] private float fastCloseDuration = 0.5f;

    [Header("Protecciones de interacción")]
    [SerializeField] private bool blockInteractionWhileAnimating = true;
    [SerializeField] private bool blockRaycastsWhileAnimating = true;

    private bool isAnimating = false;
    private int originalLayer;

    private Quaternion initialRotation;
    private Coroutine doorCoroutine;
    
    // Variables para puertas cerradas
    private bool isPlayingClosedDoorSequence = false;
    private bool hasCompletedClosedDoorSequence = false;
    private Coroutine closedDoorSequenceCoroutine;

    protected override void Awake()
    {
        base.Awake(); // Llamamos al Awake de la clase base para inicializar el objeto interactivo
        initialRotation = transform.rotation;

        this.FindAudioSource();

        // Guardar el mixer group original
        if (audioSource != null)
        {
            originalMixerGroup = audioSource.outputAudioMixerGroup;
        }
    }

    private void Start()
    {
        // Si es de tipo SlowClosing, empezar con la puerta abierta
        if (type == TypeDoorInteract.SlowClosing)
        {
            // Rotar la puerta a la posición inicial abierta
            Quaternion openRotation = Quaternion.Euler(initialRotation.eulerAngles.x,
                                                     initialRotation.eulerAngles.y + initialOpenDegrees,
                                                     initialRotation.eulerAngles.z);
            transform.rotation = openRotation;
            Debug.Log($"[Door] Puerta SlowClosing iniciada abierta en {initialOpenDegrees} grados");
        }

        //this.StartKnockingLoop();
        //this.StartSlowClosing();
    }

    /// <summary>
    /// Busca un AudioSource en los GameObjects hijos
    /// </summary>
    private void FindAudioSource()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource != null)
        {
            return;
        }

        // Si no se encuentra ningún AudioSource, mostrar advertencia
        Debug.LogWarning($"[Door] No se encontró ningún AudioSource en {gameObject.name} en sus hijos. Los sonidos no funcionarán.");
    }

    private void SetAnimating(bool value)
    {
        isAnimating = value;

        if (value)
        {
            ForceUnhover();
            if (blockRaycastsWhileAnimating)
            {
                originalLayer = gameObject.layer;
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
        else
        {
            if (blockRaycastsWhileAnimating)
                gameObject.layer = originalLayer;
        }
    }


    public override void OnHoverEnter()
    {
        if (type != TypeDoorInteract.OpenAndClose && type != TypeDoorInteract.Close) return;
        if (isAnimating && blockInteractionWhileAnimating) return; // NO outline si anima
        if(type== TypeDoorInteract.None) return;
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        if (type != TypeDoorInteract.OpenAndClose && type != TypeDoorInteract.Close) return;
        base.OnHoverExit();
    }

    public override void OnInteract()
    {
        if (blockInteractionWhileAnimating && isAnimating) return;
        
        // Verificar si estamos reproduciendo una secuencia de puerta cerrada
        if (isPlayingClosedDoorSequence && closedDoorDialogData != null && closedDoorDialogData.ShouldBlockInteractionDuringPlayback())
        {
            Debug.Log("[Door] Interacción bloqueada durante la reproducción de la secuencia de puerta cerrada");
            return;
        }

        ForceUnhover();
        
        // Manejar puerta cerrada
        if (type == TypeDoorInteract.Close)
        {
            HandleClosedDoorInteraction();
            return;
        }
        
        // Comportamiento normal para puertas que se pueden abrir/cerrar
        isDoorOpen = !isDoorOpen;
        ValidateDoorWithAnimation();
    }


    public void SetType(TypeDoorInteract newType)
    {
        type = newType;
    }

    /// <summary>
    /// Propiedad pública para acceder a la duración del cierre lento
    /// </summary>
    public float SlowCloseDuration => slowCloseDuration;

    /// <summary>
    /// Propiedad pública para acceder a la duración del cierre rápido
    /// </summary>
    public float FastCloseDuration => fastCloseDuration;

    /// <summary>
    /// Permite asignar el clip de cierre rápido desde código
    /// </summary>
    public void SetFastCloseClip(AudioClip clip)
    {
        fastCloseClip = clip;
    }

    /// <summary>
    /// Reinicia el estado de la puerta cerrada para permitir nueva interacción
    /// </summary>
    public void ResetClosedDoorState()
    {
        hasCompletedClosedDoorSequence = false;
        if (closedDoorSequenceCoroutine != null)
        {
            StopCoroutine(closedDoorSequenceCoroutine);
            closedDoorSequenceCoroutine = null;
        }
        isPlayingClosedDoorSequence = false;
        Debug.Log("[Door] Estado de puerta cerrada reiniciado");
    }

    /// Abre la puerta rotando en Y según el atributo openDegreesY
    public void OpenDoorByRotation()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(RotateDoorCoroutine(openDegreesY));
    }

    /// Simula que la puerta "late" (golpe sutil y regresa a rotación original)
    public void KnockDoor()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(KnockDoorCoroutine());
    }

    // Inicia el bucle de golpeo (Knocking) si el tipo es Knocking
    public void StartKnockingLoop()
    {
        if (type != TypeDoorInteract.Knocking) return;
        if (knockingLoopCoroutine != null) StopCoroutine(knockingLoopCoroutine);
        knockingLoopCoroutine = StartCoroutine(KnockingLoopCoroutine());
        Debug.Log("[Door] Iniciando bucle de Knocking");
    }

    /// <summary>
    /// Detiene el bucle de golpeo (Knocking)
    /// </summary>
    public void StopKnockingLoop()
    {
        if (knockingLoopCoroutine != null)
        {
            StopCoroutine(knockingLoopCoroutine);
            knockingLoopCoroutine = null;
            Debug.Log("[Door] Deteniendo bucle de Knocking");
        }
    }

    // Inicia el cierre lento si el tipo es SlowClosing
    public void StartSlowClosing()
    {
        if (type != TypeDoorInteract.SlowClosing) return;
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(SlowCloseCoroutine());
        Debug.Log("[Door] Iniciando cierre lento");
    }

    // Inicia el cierre rápido (para cuando se activa externamente)
    public void StartFastClosing()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(FastCloseCoroutine());
        Debug.Log("[Door] Iniciando cierre rápido");
    }

    /// <summary>
    /// Reproduce un sonido de audio específico
    /// </summary>
    private void PlayDoorAudio(AudioClip clip)
    {
        PlayDoorAudio(clip, null);
    }

    /// <summary>
    /// Reproduce un sonido de audio específico con un mixer group opcional
    /// </summary>
    private void PlayDoorAudio(AudioClip clip, UnityEngine.Audio.AudioMixerGroup mixerGroup)
    {
        if (audioSource != null && clip != null)
        {
            // Cambiar temporalmente el mixer group si se especifica uno
            if (mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }
            else
            {
                audioSource.outputAudioMixerGroup = originalMixerGroup;
            }

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private IEnumerator KnockingLoopCoroutine()
    {
        while (true)
        {
            KnockDoor();
            yield return new WaitForSeconds(knockingInterval);
        }
    }

    private IEnumerator RotateDoorCoroutine(float targetDegrees)
    {
        SetAnimating(true);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(
            startRot.eulerAngles.x,
            startRot.eulerAngles.y + targetDegrees,
            startRot.eulerAngles.z);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            float t = Mathf.Clamp01(elapsed / openDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;

        SetAnimating(false);
    }

    private IEnumerator KnockDoorCoroutine()
    {
        Quaternion startRot = initialRotation;
        Quaternion knockRot = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y + knockAmount, startRot.eulerAngles.z);
        float t = 0f;

        // Reproducir sonido de golpe
        PlayDoorAudio(knockClip);

        // Golpe hacia knockAmount
        while (t < 1f)
        {
            t += Time.deltaTime * knockSpeed;
            transform.rotation = Quaternion.Slerp(startRot, knockRot, t);
            yield return null;
        }
        transform.rotation = knockRot;
        // Regresa a rotación original
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * knockSpeed;
            transform.rotation = Quaternion.Slerp(knockRot, startRot, t);
            yield return null;
        }
        transform.rotation = startRot;
    }

    private IEnumerator SlowCloseCoroutine()
    {
        SetAnimating(true);
        PlayDoorAudio(slowCloseClip);

        Quaternion startRot = transform.rotation;
        
        // Crear la rotación objetivo (Y = 0, manteniendo X y Z originales)
        Vector3 targetEuler = startRot.eulerAngles;
        targetEuler.y = 0f;
        Quaternion targetRot = Quaternion.Euler(targetEuler);
            
        float elapsed = 0f;

        while (elapsed < slowCloseDuration)
        {
            float t = Mathf.Clamp01(elapsed / slowCloseDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Usar Slerp para interpolación suave de quaternions (toma el camino más corto)
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Asegurar que termine exactamente en la rotación objetivo
        transform.rotation = targetRot;

        SetAnimating(false);
    }

    private IEnumerator FastCloseCoroutine()
    {
        SetAnimating(true);
        PlayDoorAudio(fastCloseClip, fastCloseMixer);

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = initialRotation;
        float elapsed = 0f;

        while (elapsed < fastCloseDuration)
        {
            float t = Mathf.Clamp01(elapsed / fastCloseDuration);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;

        SetAnimating(false);
    }
    // Elimino llave de cierre extra para que las funciones siguientes estén dentro de la clase

    private void ValidateDoorWithAnimation()
    {
        if (type != TypeDoorInteract.OpenAndClose) return; // Si no es del tipo OpenAndClose, no hacemos nada

        this.OpenOrCloseDoor(isDoorOpen);


    }

    /// <summary>
    /// Maneja la interacción con una puerta cerrada
    /// </summary>
    private void HandleClosedDoorInteraction()
    {
        // SIEMPRE reproducir el sonido de puerta cerrada
        if (lockedDoorClip != null)
        {
            PlayDoorAudio(lockedDoorClip);
        }

        // Si ya está reproduciendo, no hacer nada
        if (isPlayingClosedDoorSequence)
        {
            Debug.Log("[Door] Secuencia ya en progreso, ignorando interacción");
            return;
        }

        // Si ya se completó y no se permite reinteracción, no hacer nada
        if (hasCompletedClosedDoorSequence && !closedDoorDialogData.ShouldAllowReinteraction())
        {
            Debug.Log("[Door] Secuencia ya completada y no se permite reinteracción");
            this.SetType(TypeDoorInteract.None);
            return;
        }

        // Detener secuencia anterior si existe
        if (closedDoorSequenceCoroutine != null)
        {
            StopCoroutine(closedDoorSequenceCoroutine);
        }

        // Iniciar nueva secuencia
        closedDoorSequenceCoroutine = StartCoroutine(PlayClosedDoorSequence());
    }

    /// <summary>
    /// Reproduce la secuencia completa de puerta cerrada: sonido + diálogos
    /// </summary>
    private IEnumerator PlayClosedDoorSequence()
    {
        isPlayingClosedDoorSequence = true;
        
        // Pequeña pausa para que no se solape con el lockedDoorClip que ya se reprodujo
        if (lockedDoorClip != null)
        {
            yield return new WaitForSeconds(lockedDoorClip.length);
        }
        
        // 1. Reproducir audio del diálogo (UN SOLO clip para toda la secuencia)
        AudioClip dialogAudio = closedDoorDialogData.GetDialogAudioClip();
        if (dialogAudio != null)
        {
            PlayDoorAudio(dialogAudio);
            // Pequeña pausa para que no se solape
            yield return new WaitForSeconds(0.2f);
        }
        
        // 2. Reproducir secuencia de diálogos
        DialogData[] messages = closedDoorDialogData.GetAllDialogMessages();
        
        for (int i = 0; i < messages.Length; i++)
        {
            DialogData message = messages[i];
            
            // Mostrar el diálogo
            DialogController.Instance.ShowDialog(message.dialogText, message.duration);
            Debug.Log($"[Door] Mostrando diálogo {i + 1}/{messages.Length}: '{message.dialogText}'");
            
            // Esperar la duración del diálogo
            yield return new WaitForSeconds(message.duration);
            
            // Pequeña pausa entre diálogos si hay más de uno
            if (i < messages.Length - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        // Marcar como completada
        hasCompletedClosedDoorSequence = true;
        isPlayingClosedDoorSequence = false;
        closedDoorSequenceCoroutine = null;
        
        // Si NO tiene allowReinteraction activado, cambiar el tipo a None
        if (!closedDoorDialogData.ShouldAllowReinteraction())
        {
            type = TypeDoorInteract.None;
            Debug.Log("[Door] Puerta cambiada a tipo 'None' después de completar secuencia con allowReinteraction desactivado");
        }
        
        Debug.Log("[Door] Secuencia de puerta cerrada completada");
    }

    public void OpenOrCloseDoor(bool open)
    {
        if (blockInteractionWhileAnimating && isAnimating) return;

        if (doorCoroutine != null) StopCoroutine(doorCoroutine);

        if (open)
        {
            Debug.Log($"[Door] Abriendo puerta: rotando a {openDegreesY} grados Y");
            doorCoroutine = StartCoroutine(RotateDoorCoroutine(openDegreesY));
            PlayDoorAudio(openDoorClip);
        }
        else
        {
            Debug.Log($"[Door] Cerrando puerta: rotando a {-openDegreesY} grados Y");
            doorCoroutine = StartCoroutine(RotateDoorCoroutine(-openDegreesY));
            PlayDoorAudio(closeDoorClip);
        }
    }
}
