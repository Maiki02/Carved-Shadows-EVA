using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using BlendStyle = Unity.Cinemachine.CinemachineBlendDefinition.Styles;

[RequireComponent(typeof(AudioSource))]
public class PhoneOpen : MonoBehaviour
{
    private PlayerController playerController;

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip callClip;
    [SerializeField] private AudioClip hangupClip;

    [Header("Phone Call Settings")]
    [Tooltip("Secuencia de diálogos de la llamada telefónica")]
    [SerializeField] private DialogData[] callDialogSequence;

    [Header("Camera Settings")]
    [Tooltip("Priority to set for the phone camera during the call.")]
    public int phoneCameraPriority = 20;
    [Tooltip("Priority of the player camera (baseline). NO la bajo, la dejo fija.")]
    public int playerCameraPriority = 10;

    [Header("Controller Reference")]
    [SerializeField] private Call_Loop_01 callController;
    [SerializeField] private GameObject phoneCloseGameObject;

    // CM3
    private CinemachineCamera phoneCamera;
    private CinemachineCamera playerCamera;
    private CinemachineBrain cinemachineBrain;
    private AudioSource audioSource;
    private bool isCalling = false;
    private bool isWithCall = false;

    // Guardado de blends para "modo CUT"
    private CinemachineBlendDefinition savedDefaultBlend;
    private CinemachineBlenderSettings savedCustomBlends;
    private bool cutModeActive = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Brain en la Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            if (cinemachineBrain == null)
                Debug.LogError("CinemachineBrain not found on Main Camera.");
        }

        // Buscar la vcam del teléfono por tag en hijos (CM3)
        foreach (var cam in GetComponentsInChildren<CinemachineCamera>(true))
        {
            if (cam.CompareTag("CameraPhone"))
            {
                phoneCamera = cam;
                break;
            }
        }
        if (phoneCamera == null)
            Debug.LogError($"No CinemachineCamera with tag 'CameraPhone' found in {gameObject.name}.");

        // Buscar la vcam del jugador por tag (CM3)
        GameObject playerCameraObj = GameObject.FindGameObjectWithTag("PlayerVirtualCamera");
        if (playerCameraObj != null)
        {
            playerCamera = playerCameraObj.GetComponent<CinemachineCamera>();
            if (playerCamera == null)
                Debug.LogError("CinemachineCamera component not found on object with tag 'PlayerVirtualCamera'.");
        }
        else
        {
            Debug.LogError("Player camera object with tag 'PlayerVirtualCamera' not found.");
        }

        // PlayerController por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            if (playerController == null)
                Debug.LogError("PlayerController component not found on Player object.");
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

    // ================== UTILIDADES: MODO CUT SIN BLENDS ==================
    private void BeginHardCut()
    {
        if (cinemachineBrain == null || cutModeActive) return;
        savedDefaultBlend = cinemachineBrain.DefaultBlend;
        savedCustomBlends = cinemachineBrain.CustomBlends;

        // Anular CUALQUIER blend (incluyendo custom pairs)
        cinemachineBrain.CustomBlends = null;
        cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(BlendStyle.Cut, 0f);

        cutModeActive = true;
    }

    private IEnumerator EndHardCutNextFrame()
    {
        // Restauramos al frame siguiente para evitar que el Brain “enganche” un blend tardío
        yield return null;
        if (cinemachineBrain != null && cutModeActive)
        {
            cinemachineBrain.DefaultBlend = savedDefaultBlend;
            cinemachineBrain.CustomBlends = savedCustomBlends;
        }
        cutModeActive = false;
    }

    // ---------------------------------------------------------------------

    // ---------- Inicio llamada ----------
    public void StartCall()
    {
        if (isCalling) return;
        StartCoroutine(StartCallWithFadeInRoutine());
    }

    public void StartCallWithFadeIn()
    {
        if (isCalling) return;
        StartCoroutine(StartCallWithFadeInRoutine());
    }

    public void StartCallWithParameters(AudioClip phoneCallClip, DialogData[] phoneDialogs, bool isWithCall)
    {
        this.isWithCall = isWithCall;
        if (isCalling) return;

        if (phoneCallClip) callClip = phoneCallClip;
        if (phoneDialogs != null && phoneDialogs.Length > 0) callDialogSequence = phoneDialogs;

        StartCoroutine(StartCallWithFadeInRoutine());
    }

    private IEnumerator StartCallWithFadeInRoutine()
    {
        // Entramos en modo CUT para que NO haya blend al cambiar a la cam del teléfono
        BeginHardCut();

        // Subir/habilitar la cámara del teléfono. ¡NO toco la prioridad del player!
        EnableCameraCompletely(phoneCamera, Mathf.Max(phoneCameraPriority, playerCameraPriority + 1));

        // Dejo un frame para que el Brain haga el switch como CUT
        yield return null;

        // Restaurar blends a su estado normal
        yield return StartCoroutine(EndHardCutNextFrame());

        // (Opcional) si querés, podés apagar el “look” del player ya mismo
        // pero como ya estás en PhoneOpen, el PlayerClose gestiona los controles.

        // Fade in y arrancar la rutina de llamada
        yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(0.3f));
        StartCoroutine(PhoneCallRoutine());
    }

    // ---------- Fin llamada ----------
    private IEnumerator EndCallRoutine()
    {
        if (hangupClip != null)
            yield return new WaitForSeconds(hangupClip.length);

        // Fade out antes del cambio de cámara
        yield return StartCoroutine(FadeManager.Instance.FadeOutCoroutine(0.3f));

        // Entrar en modo CUT para el handoff de regreso al player
        BeginHardCut();

        // Asegurar que la del player esté activa y con prioridad base (no la bajo en ningún momento)
        if (playerCamera)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
            playerCamera.Priority = playerCameraPriority;
        }

        // Apagar la del teléfono y bajarla
        DisableCameraCompletely(phoneCamera);

        // Un frame para consolidar el cut
        yield return null;

        // Alinear yaw del player a lo que se ve (por si tu rig lo necesita)
        if (playerController != null)
            playerController.SnapToCurrentCamera();

        // Restaurar blends normales a partir del próximo frame
        yield return StartCoroutine(EndHardCutNextFrame());

        // Reactivar el teléfono cerrado y notificar
        if (phoneCloseGameObject)
        {
            phoneCloseGameObject.SetActive(true);
            var phoneCloseScript = phoneCloseGameObject.GetComponent<PhoneClose>();
            if (phoneCloseScript) phoneCloseScript.OnHangUp(isWithCall);
        }
        if (callController) callController.OnCallCompleted(isWithCall);

        isCalling = false;
        isWithCall = false;

        // Habilitar controles del player con un pelín de delay para no morder ejes
        if (playerController != null)
            StartCoroutine(EnableLookAfterDelay(0.05f));

        // Este GO deja de estar activo
        gameObject.SetActive(false);
    }

    private IEnumerator EnableLookAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerController != null)
            playerController.SetControlesActivos(true);
    }

    // ---------- Secuencia principal de llamada ----------
    private IEnumerator PhoneCallRoutine()
    {
        if (isCalling) yield break;
        isCalling = true;

        // Audio de llamada
        if (audioSource != null && callClip != null)
        {
            audioSource.clip = callClip;
            audioSource.loop = false;
            audioSource.Play();
        }

        yield return new WaitForSeconds(0.5f);

        // Diálogos
        if (callDialogSequence != null && callDialogSequence.Length > 0)
            yield return StartCoroutine(ShowDialogSequenceAndWait());

        EndCall();
    }

    private void EndCall()
    {
        if (audioSource != null && hangupClip != null)
        {
            audioSource.clip = hangupClip;
            audioSource.loop = false;
            audioSource.Play();
        }

        StartCoroutine(EndCallRoutine());
    }

    private IEnumerator ShowDialogSequenceAndWait()
    {
        bool finished = false;
        yield return StartCoroutine(DialogSequenceCoroutine(() => finished = true));
        while (!finished) yield return null;
    }

    private IEnumerator DialogSequenceCoroutine(System.Action onComplete)
    {
        DialogController.Instance.ShowDialogSequence(callDialogSequence);
        float totalDuration = 0f;
        foreach (var msg in callDialogSequence)
            totalDuration += msg.duration;
        yield return new WaitForSeconds(totalDuration);
        onComplete?.Invoke();
    }

    private void ValidateReferences()
    {
        if (callClip == null) Debug.LogWarning("[PhoneOpen] Call clip no asignado");
        if (hangupClip == null) Debug.LogWarning("[PhoneOpen] Hangup clip no asignado");
        if (callDialogSequence == null || callDialogSequence.Length == 0)
            Debug.LogWarning("[PhoneOpen] Secuencia de diálogos no asignada");
        if (callController == null) Debug.LogWarning("[PhoneOpen] Call Controller no asignado");
        if (phoneCamera == null) Debug.LogWarning("[PhoneOpen] Phone camera no encontrada");
        if (playerCamera == null) Debug.LogWarning("[PhoneOpen] Player camera no encontrada");
        if (phoneCloseGameObject == null) Debug.LogWarning("[PhoneOpen] PhoneClose GameObject no asignado");
        if (playerController == null) Debug.LogWarning("[PhoneOpen] PlayerController no asignado");
    }

    // ===== Helpers para (des)activar cámaras =====
    private void DisableCameraCompletely(CinemachineCamera camera)
    {
        if (!camera) return;
        camera.Priority = 0;
        camera.enabled = false;
        camera.gameObject.SetActive(false);
    }
    private void EnableCameraCompletely(CinemachineCamera camera, int priority)
    {
        if (!camera) return;
        camera.gameObject.SetActive(true);
        camera.enabled = true;
        camera.Priority = priority;
    }
}
