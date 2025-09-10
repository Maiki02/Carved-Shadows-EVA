
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

    [Header("Audio Mixer Groups")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup fastCloseMixer;

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
        if (type != TypeDoorInteract.OpenAndClose) return;
        if (isAnimating && blockInteractionWhileAnimating) return; // NO outline si anima
        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        if (type != TypeDoorInteract.OpenAndClose) return;
        base.OnHoverExit();
    }

    public override void OnInteract()
    {
        if (blockInteractionWhileAnimating && isAnimating) return;

        ForceUnhover();
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
        Quaternion targetRot = initialRotation;
        float elapsed = 0f;

        while (elapsed < slowCloseDuration)
        {
            float t = Mathf.Clamp01(elapsed / slowCloseDuration);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
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
    // Elimino llave de cierre extra para que las funciones siguientes estén dentro de la clase

    private void ValidateDoorWithAnimation()
    {
        if (type != TypeDoorInteract.OpenAndClose) return; // Si no es del tipo OpenAndClose, no hacemos nada

        this.OpenOrCloseDoor(isDoorOpen);


    }

    /*private void ValidateDoorWithNextLevel()
    {
        if (type != TypeDoorInteract.NextLevel) return; // Si no es del tipo NextLevel, no hacemos nada
        isDoorOpen = true; // Forzamos la apertura de la puerta para el siguiente nivel

        if (isDoorOpen)
        {
            GameFlowManager.Instance.GoToNextLevel(); // Subimos el nivel, (para activar otra room)
        }
        else
        {
            DialogController.Instance.ShowDialog("La puerta está cerrada. Necesitas abrirla para continuar.", 2f);
        }
    }*/

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

    /*private void ValidateDoorWithTeleport()
    {
        if (type != TypeDoorInteract.Key) { return; } //Si no es del tipo Key, no hacemos nada

        //Debug.Log("Tiene key: " + TieneObjetoEnInventario());

        if (inventarioHotbar != null && inventarioHotbar.TieneObjetoSeleccionado(objetoRequerido))
        {

            isDoorOpen = true;
            inventarioHotbar.RemoveSelectedPiece(); // Quitamos la pieza del inventario
            GameFlowManager.Instance.GoToNextLevel(); // Subimos el nivel, (para activar otra room)
        }
        else
        {
            DialogController.Instance.ShowDialog("La puerta está cerrada. Necesitas una llave.", 2f);
        }
    }*/

    /*private bool TieneObjetoEnInventario()
    {
        if (inventarioHotbar == null || objetoRequerido == null)
            return false;

        PuzzlePiece[] piezas = inventarioHotbar.ObtenerPiezas();
        foreach (PuzzlePiece pieza in piezas)
        {
            if (pieza != null && pieza == objetoRequerido)
            {
                return true;
            }
        }
        return false;
    }*/

}
