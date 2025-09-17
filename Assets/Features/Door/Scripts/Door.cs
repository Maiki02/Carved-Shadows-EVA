using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    [Header("Anti-spam puerta cerrada")]
    [SerializeField] private float lockedInteractionCooldown = 0.8f;
    [SerializeField] private bool blockHoverDuringLockedCooldown = true;

    private bool isLockedCooldown = false;
    private Coroutine lockedCooldownCoroutine;


    [Header("Events")]
    public UnityEvent OnClosedDoorReviewedUnity;                 // para enganchar desde Inspector
    public event Action ClosedDoorSequenceCompleted;             // para scripts (e.g., BasementArrivalTrigger)

    protected override void Awake()
    {
        base.Awake();
        initialRotation = transform.rotation;

        this.FindAudioSource();

        if (audioSource != null)
            originalMixerGroup = audioSource.outputAudioMixerGroup;
    }

    private void Start()
    {
        if (type == TypeDoorInteract.SlowClosing)
        {
            Quaternion openRotation = Quaternion.Euler(initialRotation.eulerAngles.x,
                                                     initialRotation.eulerAngles.y + initialOpenDegrees,
                                                     initialRotation.eulerAngles.z);
            transform.rotation = openRotation;
            Debug.Log($"[Door] Puerta SlowClosing iniciada abierta en {initialOpenDegrees} grados");
        }
    }

    private void FindAudioSource()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource == null)
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
        if (type == TypeDoorInteract.Close && isLockedCooldown) return;

        if (type != TypeDoorInteract.OpenAndClose && type != TypeDoorInteract.Close) return;
        if (isAnimating && blockInteractionWhileAnimating) return;
        if (type == TypeDoorInteract.None) return;

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

        // Secuencia de puerta cerrada (locked)
        if (type == TypeDoorInteract.Close)
        {
            HandleClosedDoorInteraction();
            return;
        }

        // Normal (abrir/cerrar)
        isDoorOpen = !isDoorOpen;
        ValidateDoorWithAnimation();
    }

    public void SetType(TypeDoorInteract newType)
    {
        type = newType;
    }

    public float SlowCloseDuration => slowCloseDuration;
    public float FastCloseDuration => fastCloseDuration;

    public void SetFastCloseClip(AudioClip clip)
    {
        fastCloseClip = clip;
    }

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

    public void OpenDoorByRotation()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(RotateDoorCoroutine(openDegreesY));
    }

    public void KnockDoor()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(KnockDoorCoroutine());
    }

    public void StartKnockingLoop()
    {
        if (type != TypeDoorInteract.Knocking) return;
        if (knockingLoopCoroutine != null) StopCoroutine(knockingLoopCoroutine);
        knockingLoopCoroutine = StartCoroutine(KnockingLoopCoroutine());
        Debug.Log("[Door] Iniciando bucle de Knocking");
    }

    public void StopKnockingLoop()
    {
        if (knockingLoopCoroutine != null)
        {
            StopCoroutine(knockingLoopCoroutine);
            knockingLoopCoroutine = null;
            Debug.Log("[Door] Deteniendo bucle de Knocking");
        }
    }

    public void StartSlowClosing()
    {
        if (type != TypeDoorInteract.SlowClosing) return;
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(SlowCloseCoroutine());
        Debug.Log("[Door] Iniciando cierre lento");
    }

    public void StartFastClosing()
    {
        if (doorCoroutine != null) StopCoroutine(doorCoroutine);
        doorCoroutine = StartCoroutine(FastCloseCoroutine());
        Debug.Log("[Door] Iniciando cierre rápido");
    }

    private void PlayDoorAudio(AudioClip clip)
    {
        PlayDoorAudio(clip, null);
    }

    private void PlayDoorAudio(AudioClip clip, UnityEngine.Audio.AudioMixerGroup mixerGroup)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup != null ? mixerGroup : originalMixerGroup;
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

        PlayDoorAudio(knockClip);

        while (t < 1f)
        {
            t += Time.deltaTime * knockSpeed;
            transform.rotation = Quaternion.Slerp(startRot, knockRot, t);
            yield return null;
        }
        transform.rotation = knockRot;

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
        Vector3 targetEuler = startRot.eulerAngles;
        targetEuler.y = 0f;
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        float elapsed = 0f;

        while (elapsed < slowCloseDuration)
        {
            float t = Mathf.Clamp01(elapsed / slowCloseDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            elapsed += Time.deltaTime;
            yield return null;
        }

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

    private void ValidateDoorWithAnimation()
    {
        if (type != TypeDoorInteract.OpenAndClose) return;
        this.OpenOrCloseDoor(isDoorOpen);
    }

    private void HandleClosedDoorInteraction()
    {
        if (isLockedCooldown)
        {
            return;
        }

        if (hasCompletedClosedDoorSequence && !closedDoorDialogData.ShouldAllowReinteraction())
        {
            this.SetType(TypeDoorInteract.None);
            return;
        }

        if (isPlayingClosedDoorSequence)
        {
            BeginLockedCooldown();
            return;
        }

        if (lockedDoorClip != null)
            PlayDoorAudio(lockedDoorClip);

        BeginLockedCooldown();

        if (closedDoorSequenceCoroutine != null)
            StopCoroutine(closedDoorSequenceCoroutine);

        closedDoorSequenceCoroutine = StartCoroutine(PlayClosedDoorSequence());
    }


    private IEnumerator PlayClosedDoorSequence()
    {
        isPlayingClosedDoorSequence = true;

        if (lockedDoorClip != null)
            yield return new WaitForSeconds(lockedDoorClip.length);

        AudioClip dialogAudio = closedDoorDialogData.GetDialogAudioClip();
        if (dialogAudio != null)
        {
            PlayDoorAudio(dialogAudio);
            yield return new WaitForSeconds(0.2f);
        }

        DialogData[] messages = closedDoorDialogData.GetAllDialogMessages();
        for (int i = 0; i < messages.Length; i++)
        {
            DialogData message = messages[i];
            DialogController.Instance.ShowDialog(message.dialogText, message.duration);
            Debug.Log($"[Door] Mostrando diálogo {i + 1}/{messages.Length}: '{message.dialogText}'");
            yield return new WaitForSeconds(message.duration);
            if (i < messages.Length - 1)
                yield return new WaitForSeconds(0.2f);
        }

        hasCompletedClosedDoorSequence = true;
        isPlayingClosedDoorSequence = false;
        closedDoorSequenceCoroutine = null;

        if (!closedDoorDialogData.ShouldAllowReinteraction())
        {
            type = TypeDoorInteract.None;
            Debug.Log("[Door] Puerta cambiada a tipo 'None' después de completar secuencia con allowReinteraction desactivado");
        }

        Debug.Log("[Door] Secuencia de puerta cerrada completada");

        // === DISPARAR EVENTO: el jugador “revisó” la puerta cerrada ===
        OnClosedDoorReviewedUnity?.Invoke();
        ClosedDoorSequenceCompleted?.Invoke();
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

    private void BeginLockedCooldown(float? duration = null)
    {
        float d = duration ?? lockedInteractionCooldown;

        if (lockedCooldownCoroutine != null)
            StopCoroutine(lockedCooldownCoroutine);

        lockedCooldownCoroutine = StartCoroutine(LockedCooldownCoroutine(d));
    }

    private IEnumerator LockedCooldownCoroutine(float duration)
    {
        isLockedCooldown = true;

        if (blockHoverDuringLockedCooldown)
            ForceUnhover();

        yield return new WaitForSeconds(duration);

        isLockedCooldown = false;
        lockedCooldownCoroutine = null;
    }

}