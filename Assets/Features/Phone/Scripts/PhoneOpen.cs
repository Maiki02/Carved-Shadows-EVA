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
    [Tooltip("Priority of the player camera (to restore after call).")]
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

    /// Fuerza un corte instantáneo en Cinemachine (CM3)
    private void ForceInstantCameraCut()
    {
        if (cinemachineBrain == null) return;

        var originalBlend = cinemachineBrain.DefaultBlend;
        var cutBlend = new CinemachineBlendDefinition(BlendStyle.Cut, 0f);
        cinemachineBrain.DefaultBlend = cutBlend;

        StartCoroutine(RestoreBlendAfterFrame(originalBlend));
    }

    private IEnumerator RestoreBlendAfterFrame(CinemachineBlendDefinition originalBlend)
    {
        yield return null; // 1 frame
        if (cinemachineBrain != null)
            cinemachineBrain.DefaultBlend = originalBlend;
    }

    public void StartCall()
    {
        if (isCalling) return;

        if (phoneCamera != null) phoneCamera.Priority = phoneCameraPriority;
        if (playerCamera != null) playerCamera.Priority = 0;

        StartCoroutine(PhoneCallRoutine());
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

        if (phoneCallClip != null) callClip = phoneCallClip;
        if (phoneDialogs != null && phoneDialogs.Length > 0) callDialogSequence = phoneDialogs;

        StartCoroutine(StartCallWithFadeInRoutine());
    }

    private IEnumerator StartCallWithFadeInRoutine()
    {
        // 1) Corte instantáneo
        ForceInstantCameraCut();

        // 2) Subir prioridad de la cámara del teléfono
        if (phoneCamera != null) phoneCamera.Priority = phoneCameraPriority;
        if (playerCamera != null) playerCamera.Priority = 0;

        // 3) Esperar un frame para que el Brain cambie
        yield return null;

        // 4) Fade in
        yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(0.3f));

        // 5) Iniciar llamada
        StartCoroutine(PhoneCallRoutine());
    }

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

    private IEnumerator EndCallRoutine()
    {
        // Esperar a que termine el sonido de colgar
        if (hangupClip != null)
            yield return new WaitForSeconds(hangupClip.length);

        // 1) Fade out
        yield return StartCoroutine(FadeManager.Instance.FadeOutCoroutine(0.3f));

        // 2) Corte instantáneo para volver
        ForceInstantCameraCut();

        // 3) Volver a la cámara del jugador
        if (playerCamera != null) playerCamera.Priority = playerCameraPriority;
        if (phoneCamera != null) phoneCamera.Priority = 0;

        // 4) Un frame para que el Brain procese
        yield return null;

        // 5) Reactivar el teléfono cerrado (esto habilita controles desde PhoneClose.OnHangUp)
        if (phoneCloseGameObject != null)
        {
            phoneCloseGameObject.SetActive(true);

            var phoneCloseScript = phoneCloseGameObject.GetComponent<PhoneClose>();
            if (phoneCloseScript != null)
                phoneCloseScript.OnHangUp(isWithCall);
        }

        // 6) Notificar al controlador principal
        if (callController != null)
            callController.OnCallCompleted(isWithCall);

        isCalling = false;
        isWithCall = false;

        // 7) Desactivar este GO al final
        gameObject.SetActive(false);
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
    }
}
