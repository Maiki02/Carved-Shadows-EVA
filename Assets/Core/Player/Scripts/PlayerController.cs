using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float GetFallDuration() => fallTime;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3.5f;
    private Vector3 velocity;
    private Vector3 currentMove = Vector3.zero;
    [SerializeField] private float accelerationSpeed = 5f;

    [Header("Cámaras (CM3)")]
    [SerializeField] private CinemachineCamera mainCam;
    [SerializeField] private CinemachineCamera cinematicCam;
    [SerializeField] private CinemachineBrain brain;

    [Header("Depth of Field - Inspección")]
    [SerializeField] private bool usarDepthOfField = true;
    [SerializeField] private float depthOfFieldIntensidad = 5f;
    [SerializeField] private float transicionDepthOfField = 1f;
    [SerializeField] private Volume postProcessVolume;
    private float depthOfFieldOriginal = 0f;

    [Header("Shake al caminar (Noise)")]
    [SerializeField] private float idleAmplitude = 0.05f;
    [SerializeField] private float maxShake = 2.5f;
    [SerializeField] private float maxSpeed = 3.5f;
    [SerializeField] private NoiseSettings walkNoiseProfile;

    // CM3 components
    private CinemachineBasicMultiChannelPerlin noise;
    private CinemachineInputAxisController axisCtrl; // habilitar/deshabilitar look

    [Header("Caída")]
    [SerializeField] private float fallTime = 1f;
    [SerializeField] private float fallAngle = 90f;
    private readonly AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Mareos")]
    [SerializeField] private float dizzyMoveMultiplier = 0.75f;
    [SerializeField] private float dizzyLookMultiplier = 0.85f;

    // Roll (Dutch) realista
    [SerializeField] private float dizzyMaxRoll = 4f;
    [SerializeField] private float rollDriftFreq = 0.5f;
    [SerializeField] private float rollMicroFreq = 0.8f;
    [SerializeField] private float rollSmooth = 0.25f;

    // Sway de cabeza
    [SerializeField] private float swayMagnitude = 0.015f;
    [SerializeField] private float swayFreq = 0.8f;
    [SerializeField] private float swayMicroMagnitude = 0.006f;
    [SerializeField] private float swayMicroFreq = 5.5f;

    // Ruido Cinemachine (extra en mareo)
    [SerializeField] private float dizzyNoiseBoost = 0.35f;

    // FOV (opcional)
    [SerializeField] private bool useFOVPulse = false;
    [SerializeField] private float dizzyFOVPulse = 0.5f;
    [SerializeField] private float fovPulseFreq = 0.25f;

    [SerializeField] private AnimationCurve dizzyCurve = null;

    // Estado mareo
    private bool isDizzy = false;
    private float dizzyDuration = 0f;
    private float dizzyTimer = 0f;
    private float dizzyIntensity = 1f; // 0..1

    // Semillas Perlin
    private float seedRoll, seedSwayX, seedSwayY, seedFov;

    // Cámaras / mezcla
    private float baseFOV = 60f;
    private float baseDutch = 0f;
    private float extraNoiseMul = 1f;
    private float targetExtraFreq = 1f;

    // Acumuladores
    private float currentRoll = 0f;
    private float rollVel = 0f;
    private Vector3 baseCamLocalPos;
    private Vector3 swayVel = Vector3.zero;

    private CharacterController controller;
    private Transform camTransform;
    private bool controlesActivos = true;
    private bool isFalling = false;

    [Header("Input System")]
    [SerializeField] private InputActionReference moveAction;   // Gameplay/Move (Vector2)
    [SerializeField] private InputActionReference lookAction;   // Gameplay/Look (Vector2) - lo usa CM3
    [SerializeField] private InputActionReference pauseAction;  // Gameplay/Pause (Button)

    // ---- NUEVO: soporte “hold to clear” ----
    private Coroutine dizzyHoldCR;
    private bool dizzyHoldActive;
    private const float DIZZY_VERY_LONG = 9999f;

    void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        pauseAction?.action.Enable();
    }

    void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        pauseAction?.action.Disable();
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camTransform = GetComponentInChildren<Camera>()?.transform;

        if (dizzyCurve == null)
            dizzyCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        if (mainCam)
        {
            noise = mainCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            axisCtrl = mainCam.GetComponent<CinemachineInputAxisController>();

            if (noise != null && walkNoiseProfile != null)
                noise.NoiseProfile = walkNoiseProfile;

            baseFOV = mainCam.Lens.FieldOfView;
            baseDutch = mainCam.Lens.Dutch;
        }

        if (camTransform != null)
            baseCamLocalPos = camTransform.localPosition;
    }

    void Update()
    {
        ActualizarMareo();
        AplicarShakeCamara();

        // Toggle de pausa (Esc / Start)
        if (pauseAction != null && pauseAction.action.WasPressedThisFrame())
        {
            PauseController.Instance?.TogglePauseMenu();
        }

        // Sincroniza si la cámara puede rotar (look) según estado actual
        SyncCameraInputEnabled();

        // Si el juego está pausado, no procesamos input de juego
        if (GameController.Instance != null && GameController.Instance.IsPaused())
            return;

        if (!controlesActivos || isFalling || GameFlowManager.Instance.IsInTransition) return;

        UpdateSensibilidad();
        MoverJugador();
    }

    // Habilita/inhabilita el look de CM3 en función de pausa, caída, transiciones o controles
    private void SyncCameraInputEnabled()
    {
        bool blocked =
            !controlesActivos ||
            isFalling ||
            (GameController.Instance != null && GameController.Instance.IsPaused()) ||
            GameFlowManager.Instance.IsInTransition;

        if (axisCtrl != null) axisCtrl.enabled = !blocked;
    }

    // En CM3 la sensibilidad se configura en el CinemachineInputAxisController.
    private void UpdateSensibilidad() { }

    private void MoverJugador()
    {
        // Leer Vector2 desde el Input System (WASD / Left Stick)
        Vector2 move = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        // Deadzone suave para stick
        const float dead = 0.15f;
        if (move.sqrMagnitude < dead * dead) move = Vector2.zero;

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 direction = right * move.x + forward * move.y;

        float moveMul = 1f;
        if (isDizzy)
        {
            float weight = GetDizzyWeight();
            moveMul = Mathf.Lerp(1f, dizzyMoveMultiplier, weight);
        }

        Vector3 targetMove = direction * moveSpeed * moveMul;
        currentMove = Vector3.Lerp(currentMove, targetMove, Time.deltaTime * accelerationSpeed);

        if (controller.isGrounded) velocity.y = -2f;
        else velocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 finalMove = currentMove; finalMove.y = velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void AplicarShakeCamara()
    {
        if (noise == null || controller == null) return;

        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        bool isMoving = speed > 0.1f && controller.isGrounded;

        float targetAmp = isMoving ? Mathf.Clamp01(speed / maxSpeed) * maxShake : idleAmplitude;

        // Mezcla sutil del mareo
        targetAmp *= Mathf.Lerp(1f, extraNoiseMul, 0.5f);

        noise.AmplitudeGain = Mathf.Lerp(noise.AmplitudeGain, targetAmp, Time.deltaTime * 5f);
        noise.FrequencyGain = Mathf.Lerp(noise.FrequencyGain, targetExtraFreq, Time.deltaTime * 3f);
    }

    public void TriggerDizziness(float duration, float intensity = 1f)
    {
        if (duration <= 0f) return;

        isDizzy = true;
        dizzyDuration = duration;
        dizzyTimer = 0f;
        dizzyIntensity = Mathf.Clamp01(intensity);

        seedRoll = Random.value * 1000f;
        seedSwayX = Random.value * 1000f;
        seedSwayY = Random.value * 1000f;
        seedFov = Random.value * 1000f;

        currentRoll = 0f;
        rollVel = 0f;
        swayVel = Vector3.zero;

        extraNoiseMul = 1f;
        targetExtraFreq = (noise != null) ? Mathf.Max(1f, noise.FrequencyGain) : 1f;
    }

    private void ActualizarMareo()
    {
        if (!isDizzy || mainCam == null || camTransform == null)
        {
            if (mainCam != null)
            {
                mainCam.Lens.FieldOfView = Mathf.Lerp(mainCam.Lens.FieldOfView, baseFOV, Time.deltaTime * 2f);
                mainCam.Lens.Dutch = Mathf.Lerp(mainCam.Lens.Dutch, baseDutch, Time.deltaTime * 3f);
            }

            if (camTransform != null)
            {
                camTransform.localPosition = Vector3.SmoothDamp(
                    camTransform.localPosition,
                    baseCamLocalPos,
                    ref swayVel,
                    0.15f
                );
            }

            extraNoiseMul = Mathf.Lerp(extraNoiseMul, 1f, Time.deltaTime * 2.5f);
            targetExtraFreq = Mathf.Lerp(targetExtraFreq, 1f, Time.deltaTime * 2.5f);
            return;
        }

        // Progreso y peso
        dizzyTimer += Time.deltaTime;
        float t = Mathf.Clamp01(dizzyTimer / Mathf.Max(0.0001f, dizzyDuration));
        float weight = dizzyCurve.Evaluate(t) * dizzyIntensity;

        // ROLL (Dutch) con deriva + micro
        float rollDrift = (Mathf.PerlinNoise(seedRoll, Time.time * rollDriftFreq) * 2f - 1f);
        float rollMicro = (Mathf.PerlinNoise(seedRoll + 33.7f, Time.time * rollMicroFreq) * 2f - 1f) * 0.35f;
        float targetRoll = (rollDrift + rollMicro) * dizzyMaxRoll * weight;

        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVel, rollSmooth);
        mainCam.Lens.Dutch = Mathf.Lerp(mainCam.Lens.Dutch, baseDutch + currentRoll, Time.deltaTime * 8f);

        // SWAY de cabeza
        float sx = (Mathf.PerlinNoise(seedSwayX, Time.time * swayFreq) * 2f - 1f) * swayMagnitude;
        float sy = (Mathf.PerlinNoise(seedSwayY, Time.time * swayFreq) * 2f - 1f) * swayMagnitude * 0.6f;
        sx += (Mathf.PerlinNoise(seedSwayX + 71.2f, Time.time * swayMicroFreq) * 2f - 1f) * swayMicroMagnitude;
        sy += (Mathf.PerlinNoise(seedSwayY + 19.5f, Time.time * swayMicroFreq) * 2f - 1f) * swayMicroMagnitude * 0.7f;

        Vector3 swayTarget = baseCamLocalPos + new Vector3(sx, sy, 0f) * weight;
        camTransform.localPosition = Vector3.SmoothDamp(camTransform.localPosition, swayTarget, ref swayVel, 0.12f);

        // FOV leve (opcional)
        if (useFOVPulse)
        {
            float fovNoise = (Mathf.PerlinNoise(seedFov, Time.time * fovPulseFreq) * 2f - 1f);
            float fovTarget = baseFOV + fovNoise * dizzyFOVPulse * weight;
            mainCam.Lens.FieldOfView = Mathf.Lerp(mainCam.Lens.FieldOfView, fovTarget, Time.deltaTime * 3f);
        }
        else
        {
            mainCam.Lens.FieldOfView = Mathf.Lerp(mainCam.Lens.FieldOfView, baseFOV, Time.deltaTime * 2f);
        }

        // Ruido de Cinemachine: sutil
        extraNoiseMul = Mathf.Lerp(extraNoiseMul, 1f + dizzyNoiseBoost * weight, Time.deltaTime * 3f);
        targetExtraFreq = Mathf.Lerp(targetExtraFreq, 1f + 0.35f * weight, Time.deltaTime * 3f);

        if (dizzyTimer >= dizzyDuration)
            isDizzy = false;
    }

    private float GetDizzyWeight()
    {
        if (!isDizzy || dizzyDuration <= 0f) return 0f;
        float t = Mathf.Clamp01(dizzyTimer / dizzyDuration);
        return dizzyCurve.Evaluate(t) * dizzyIntensity;
    }

    public void SetControlesActivos(bool activos)
    {
        controlesActivos = activos;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = !activos;

        // Habilitar/inhabilitar rotación de cámara
        if (axisCtrl != null)
        {
            if (!activos)
            {
                axisCtrl.enabled = false;
            }
            else
            {
                // habilitar el look un frame después para evitar saltos
                StartCoroutine(EnableLookNextFrame());
            }
        }

        // También bloqueamos el Move si hace falta
        if (activos) moveAction?.action.Enable(); else moveAction?.action.Disable();
    }

    private IEnumerator EnableLookNextFrame()
    {
        yield return null;
        axisCtrl.enabled = true;
    }

    public void SnapToCurrentCamera()
    {
        Transform liveCam = GetActiveCameraTransform();
        if (liveCam == null) return;

        // Alinear YAW del jugador al forward de la cámara (plano XZ)
        Vector3 flatFwd = liveCam.forward; flatFwd.y = 0f;
        if (flatFwd.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(flatFwd.normalized, Vector3.up);
    }

    public void SetCamaraActiva(bool activa)
    {
        if (axisCtrl != null) axisCtrl.enabled = activa;
    }

    public void FallToTheGround()
    {
        if (!isFalling)
            StartCoroutine(FallCoroutine());
    }

    private IEnumerator FallCoroutine()
    {
        isFalling = true;
        SetControlesActivos(false);
        controller.enabled = false;

        Vector3 pivot = transform.position + Vector3.down * ((controller.height * 0.5f) - controller.radius);
        float elapsed = 0f, prevCurve = 0f;

        while (elapsed < fallTime)
        {
            float t = Mathf.Clamp01(elapsed / fallTime);
            float curve = fallCurve.Evaluate(t);
            float delta = (curve - prevCurve) * fallAngle;

            transform.RotateAround(pivot, transform.forward, delta);
            prevCurve = curve;

            elapsed += Time.deltaTime;
            yield return null;
        }

        float final = fallAngle - (prevCurve * fallAngle);
        if (!Mathf.Approximately(final, 0f))
            transform.RotateAround(pivot, transform.forward, final);

        controller.enabled = true;
        SetControlesActivos(true);
        isFalling = false;
    }

    public void StartCutsceneLook(Transform lookAt)
    {
        if (brain) brain.enabled = true;
        if (cinematicCam == null)
        {
            Debug.LogWarning("[Cam] cinematicCam no asignada");
            return;
        }

        if (mainCam != null) cinematicCam.Follow = mainCam.Follow;
        cinematicCam.LookAt = lookAt;

        if (!cinematicCam.gameObject.activeSelf)
            cinematicCam.gameObject.SetActive(true);

        int basePrio = (mainCam != null) ? mainCam.Priority : 10;
        cinematicCam.Priority = basePrio + 100;

        if (axisCtrl != null) axisCtrl.enabled = false; // no pelear con la cinemática

        StartCoroutine(LogActiveVcamNextFrame());
    }

    private IEnumerator LogActiveVcamNextFrame()
    {
        yield return null;
        if (brain != null && brain.ActiveVirtualCamera != null)
            Debug.Log("[Cam] Activa: " + brain.ActiveVirtualCamera.Name);
        else
            Debug.LogWarning("[Cam] No hay vcam activa");
    }

    public void EndCutsceneLook()
    {
        if (cinematicCam != null)
        {
            cinematicCam.Priority = 0;
            cinematicCam.gameObject.SetActive(false);
        }

        if (axisCtrl != null) axisCtrl.enabled = true;
    }

    public void ActivarCamaraInspeccion(Transform inspectionPoint)
    {
        SetControlesActivos(false);
        GameController.Instance.IsInspecting = true;

        // En lugar de cambiar de cámara, solo congelamos el movimiento de la cámara principal
        if (axisCtrl != null) axisCtrl.enabled = false;

        // Activar Depth of Field para desenfocar el fondo
        if (usarDepthOfField)
            StartCoroutine(TransicionDepthOfField(true));

        Debug.Log("Inspección activada - Movimiento de cámara desactivado");
    }

    public void DesactivarCamaraInspeccion()
    {
        GameController.Instance.IsInspecting = false;
        SetControlesActivos(true);

        // Reactivar controles de rotación de la cámara principal
        if (axisCtrl != null) axisCtrl.enabled = true;

        // Desactivar Depth of Field
        if (usarDepthOfField)
            StartCoroutine(TransicionDepthOfField(false));

        Debug.Log("Inspección desactivada - Movimiento de cámara reactivado");
    }

    private IEnumerator TransicionDepthOfField(bool activar)
    {
        if (postProcessVolume == null || !postProcessVolume.profile.TryGet<DepthOfField>(out var depthOfField))
        {
            Debug.LogWarning("No se encontró Volume o Depth of Field en el profile");
            yield break;
        }

        float valorInicial = depthOfField.focusDistance.value;
        float valorFinal = activar ? depthOfFieldIntensidad : depthOfFieldOriginal;

        // Si es la primera vez, guardamos el valor original
        if (activar && depthOfFieldOriginal == 0f)
            depthOfFieldOriginal = valorInicial;

        float tiempoTranscurrido = 0f;

        // Activar el efecto
        depthOfField.active = true;

        while (tiempoTranscurrido < transicionDepthOfField)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / transicionDepthOfField;

            float valorActual = Mathf.Lerp(valorInicial, valorFinal, progreso);
            depthOfField.focusDistance.value = valorActual;

            yield return null;
        }

        depthOfField.focusDistance.value = valorFinal;

        // Si desactivamos y llegamos al valor original, desactivar el efecto
        if (!activar && Mathf.Approximately(valorFinal, depthOfFieldOriginal))
            depthOfField.active = false;
    }

    public void SetStatusCharacterController(bool status)
    {
        if (controller) controller.enabled = status;
    }

    public Transform GetCameraTransform() => camTransform;

    public Transform GetActiveCameraTransform()
    {
        // En CM3 no existe VirtualCameraGameObject; para FPS alcanza con la cámara real
        return Camera.main != null ? Camera.main.transform : camTransform;
    }
    
    /// <summary>
    /// Inicia un mareo “infinito” que solo se limpia manteniendo [ESPACIO] durante 'holdSeconds'.
    /// Muestra un diálogo persistente con el prompt (sin tocar tu DialogController).
    /// </summary>
    public void TriggerDizzinessHoldToClear(float intensity, float holdSeconds, string prompt = "Mantén apretado [ESPACIO] para calmarte")
    {
        // Si ya había uno en curso, lo reinicio
        if (dizzyHoldCR != null)
        {
            StopCoroutine(dizzyHoldCR);
            dizzyHoldCR = null;
        }
        dizzyHoldCR = StartCoroutine(DizzyHoldRoutine(Mathf.Clamp01(intensity), Mathf.Max(0.1f, holdSeconds), prompt));
    }

    /// Limpia inmediatamente el mareo y oculta el mensaje.
    public void ForceClearDizziness()
    {
        isDizzy = false;
        dizzyTimer = 0f;

        // Detengo rutina de hold si estaba activa
        if (dizzyHoldCR != null)
        {
            StopCoroutine(dizzyHoldCR);
            dizzyHoldCR = null;
            dizzyHoldActive = false;
        }

        // Restauros suaves por si acaso
        if (mainCam != null)
        {
            mainCam.Lens.FieldOfView = baseFOV;
            mainCam.Lens.Dutch = baseDutch;
        }

        // Ocultar mensaje si estaba en pantalla
        if (DialogController.Instance != null)
            DialogController.Instance.ShowDialog("", 0.01f);
    }

    private IEnumerator DizzyHoldRoutine(float intensity, float holdSeconds, string prompt)
    {
        dizzyHoldActive = true;

        // “Mareo infinito”: uso una duración muy larga y dejo que el hold lo cancele
        TriggerDizziness(DIZZY_VERY_LONG, intensity);

        float holdTimer = 0f;

        while (dizzyHoldActive)
        {
            bool paused = (GameController.Instance != null && GameController.Instance.IsPaused()) ||
                          GameFlowManager.Instance.IsInTransition;

            bool holding = !paused && Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            if (holding) holdTimer += Time.deltaTime;
            else holdTimer = 0f;

            float remaining = Mathf.Max(0f, holdSeconds - holdTimer);

            // Refresco “persistente” del mensaje
            if (DialogController.Instance != null)
            {
                string txt = remaining > 0f
                    ? $"{prompt}"
                    : "Listo";
                DialogController.Instance.ShowDialog(txt, 0.2f);
            }

            if (holdTimer >= holdSeconds)
                break;

            yield return null;
        }

        isDizzy = false;
        dizzyTimer = 0f;

        if (mainCam != null)
        {
            mainCam.Lens.FieldOfView = baseFOV;
            mainCam.Lens.Dutch = baseDutch;
        }

        if (DialogController.Instance != null)
            DialogController.Instance.ShowDialog("", 0.01f);

        dizzyHoldActive = false;
        dizzyHoldCR = null;
    }
}
