using System.Collections;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float GetFallDuration() => fallTime;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 3.5f;
    private Vector3 velocity;
    private Vector3 currentMove = Vector3.zero;
    [SerializeField] private float accelerationSpeed = 5f;

    [Header("Cámaras")]
    [SerializeField] private CinemachineVirtualCamera mainCam;
    [SerializeField] private CinemachineVirtualCamera inspectionCam;
    [SerializeField] private CinemachineVirtualCamera cinematicCam;
    [SerializeField] private CinemachineBrain brain;

    [Header("Shake al caminar")]
    [SerializeField] private float idleAmplitude = 0.05f;
    [SerializeField] private float maxShake = 2.5f;
    [SerializeField] private float maxSpeed = 3.5f;
    private CinemachinePOV mainPOV;
    private CinemachineBasicMultiChannelPerlin noise;
    [SerializeField] private NoiseSettings walkNoiseProfile;

    [Header("Caída")]
    [SerializeField] private float fallTime = 1f;
    [SerializeField] private float fallAngle = 90f;
    private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Mareos")]
    [SerializeField] private float dizzyMoveMultiplier = 0.75f;    // menos agresivo
    [SerializeField] private float dizzyLookMultiplier = 0.85f;    // menos agresivo

    // Roll (Dutch) realista: deriva + micro oscilación
    [SerializeField] private float dizzyMaxRoll = 4f;              // grados tope (bajo)
    [SerializeField] private float rollDriftFreq = 0.5f;          // Hz lento (deriva)
    [SerializeField] private float rollMicroFreq = 0.8f;           // Hz micro oscilación
    [SerializeField] private float rollSmooth = 0.25f;             // suavizado (s)

    // Sway de la cabeza (posición local de la cámara)
    [SerializeField] private float swayMagnitude = 0.015f;         // metros
    [SerializeField] private float swayFreq = 0.8f;                // Hz principal
    [SerializeField] private float swayMicroMagnitude = 0.006f;    // metros micro
    [SerializeField] private float swayMicroFreq = 5.5f;           // Hz micro

    // Ruido de Cinemachine (ligero)
    [SerializeField] private float dizzyNoiseBoost = 0.35f;        // mucho más bajo

    // FOV: casi nada o apagado
    [SerializeField] private bool useFOVPulse = false;
    [SerializeField] private float dizzyFOVPulse = 0.5f;           // +/- muy leve
    [SerializeField] private float fovPulseFreq = 0.25f;           // Hz muy lento

    // Curva temporal del efecto
    [SerializeField]
    private AnimationCurve dizzyCurve =
        AnimationCurve.EaseInOut(0, 1, 1, 0);

    // Estado
    private bool isDizzy = false;
    private float dizzyDuration = 0f;
    private float dizzyTimer = 0f;
    private float dizzyIntensity = 1f; // 0..1

    // Semillas Perlin para desincronizar canales
    private float seedRoll, seedSwayX, seedSwayY, seedFov;

    // Cámaras / mezcla
    private float baseFOV = 60f;
    private float baseDutch = 0f;
    private float extraNoiseMul = 1f;
    private float targetExtraFreq = 1f;

    // Acumuladores y suavizados
    private float currentRoll = 0f;
    private float rollVel = 0f;
    private Vector3 baseCamLocalPos;
    private Vector3 swayVel = Vector3.zero;


    private CharacterController controller;
    private Transform camTransform;
    private bool controlesActivos = true;
    private bool isFalling = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camTransform = GetComponentInChildren<Camera>().transform;

        if (mainCam)
        {
            mainPOV = mainCam.GetCinemachineComponent<CinemachinePOV>();
            noise = mainCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null && walkNoiseProfile != null)
                noise.m_NoiseProfile = walkNoiseProfile;

            baseFOV = mainCam.m_Lens.FieldOfView;
            baseDutch = mainCam.m_Lens.Dutch;
        }
        if (mainCam != null)
        {
            baseFOV = mainCam.m_Lens.FieldOfView;
            baseDutch = mainCam.m_Lens.Dutch;
        }

        if (camTransform != null)
        {
            baseCamLocalPos = camTransform.localPosition;
        }
    }

    void Update()
    {
        ActualizarMareo();

        AplicarShakeCamara();

        if (!controlesActivos || isFalling || GameFlowManager.Instance.IsInTransition) return;

        UpdateSensibilidad();
        MoverJugador();
    }

    private void UpdateSensibilidad()
    {
        float sens = GameController.Instance.MouseSensitivity;

        // Reducir sensibilidad si hay mareo activo
        if (isDizzy)
        {
            float weight = GetDizzyWeight(); // 0..1
            sens *= Mathf.Lerp(1f, dizzyLookMultiplier, weight);
        }

        if (mainPOV != null)
        {
            mainPOV.m_HorizontalAxis.m_MaxSpeed = sens;
            mainPOV.m_VerticalAxis.m_MaxSpeed = sens;
        }
    }

    private void MoverJugador()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = right * inputX + forward * inputZ;

        // Multiplicador por mareo
        float moveMul = 1f;
        if (isDizzy)
        {
            float weight = GetDizzyWeight();
            moveMul = Mathf.Lerp(1f, dizzyMoveMultiplier, weight);
        }

        Vector3 targetMove = direction * moveSpeed * moveMul;
        currentMove = Vector3.Lerp(currentMove, targetMove, Time.deltaTime * accelerationSpeed);

        if (controller.isGrounded)
            velocity.y = -2f;
        else
            velocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 finalMove = currentMove;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);

        // Nota: si antes usabas doble Move por alguna razón específica, podés restaurarlo aquí.
        // controller.Move(finalMove * Time.deltaTime);
    }

    private void AplicarShakeCamara()
    {
        if (noise == null || controller == null) return;

        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        bool isMoving = speed > 0.1f && controller.isGrounded;

        float targetAmp = isMoving
            ? Mathf.Clamp01(speed / maxSpeed) * maxShake
            : idleAmplitude;

        // Mezcla sutil del mareo
        targetAmp *= Mathf.Lerp(1f, extraNoiseMul, 0.5f);

        noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, targetAmp, Time.deltaTime * 5f);
        noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, targetExtraFreq, Time.deltaTime * 3f);
    }


    public void TriggerDizziness(float duration, float intensity = 1f)
    {
        if (duration <= 0f) return;

        isDizzy = true;
        dizzyDuration = duration;
        dizzyTimer = 0f;
        dizzyIntensity = Mathf.Clamp01(intensity);

        // Semillas aleatorias para Perlin
        seedRoll = Random.value * 1000f;
        seedSwayX = Random.value * 1000f;
        seedSwayY = Random.value * 1000f;
        seedFov = Random.value * 1000f;

        // Reset acumuladores
        currentRoll = 0f;
        rollVel = 0f;
        swayVel = Vector3.zero;

        // Ruido Cinemachine arranca suave
        extraNoiseMul = 1f;
        targetExtraFreq = (noise != null) ? Mathf.Max(1f, noise.m_FrequencyGain) : 1f;
    }


    private void ActualizarMareo()
    {
        // Cuando no hay mareo: volver suave a la normalidad
        if (!isDizzy || mainCam == null || camTransform == null)
        {
            if (mainCam != null)
            {
                mainCam.m_Lens.FieldOfView = Mathf.Lerp(mainCam.m_Lens.FieldOfView, baseFOV, Time.deltaTime * 2f);
                mainCam.m_Lens.Dutch = Mathf.Lerp(mainCam.m_Lens.Dutch, baseDutch, Time.deltaTime * 3f);
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

        // === ROLL (Dutch) con deriva + micro oscilación (Perlin) ===
        // Perlin devuelve [0..1] -> remapeamos a [-1..1]
        float rollDrift = (Mathf.PerlinNoise(seedRoll, Time.time * rollDriftFreq) * 2f - 1f);
        float rollMicro = (Mathf.PerlinNoise(seedRoll + 33.7f, Time.time * rollMicroFreq) * 2f - 1f) * 0.35f;

        float targetRoll = (rollDrift + rollMicro) * dizzyMaxRoll * weight;
        currentRoll = Mathf.SmoothDamp(currentRoll, targetRoll, ref rollVel, rollSmooth);
        mainCam.m_Lens.Dutch = Mathf.Lerp(mainCam.m_Lens.Dutch, baseDutch + currentRoll, Time.deltaTime * 8f);

        // === SWAY de cabeza (posición local) ===
        float sx = (Mathf.PerlinNoise(seedSwayX, Time.time * swayFreq) * 2f - 1f) * swayMagnitude;
        float sy = (Mathf.PerlinNoise(seedSwayY, Time.time * swayFreq) * 2f - 1f) * swayMagnitude * 0.6f;
        // micro vibración
        sx += (Mathf.PerlinNoise(seedSwayX + 71.2f, Time.time * swayMicroFreq) * 2f - 1f) * swayMicroMagnitude;
        sy += (Mathf.PerlinNoise(seedSwayY + 19.5f, Time.time * swayMicroFreq) * 2f - 1f) * swayMicroMagnitude * 0.7f;

        Vector3 swayTarget = baseCamLocalPos + new Vector3(sx, sy, 0f) * weight;
        camTransform.localPosition = Vector3.SmoothDamp(camTransform.localPosition, swayTarget, ref swayVel, 0.12f);

        // === FOV (opcional, muy leve) ===
        if (useFOVPulse)
        {
            float fovNoise = (Mathf.PerlinNoise(seedFov, Time.time * fovPulseFreq) * 2f - 1f);
            float fovTarget = baseFOV + fovNoise * dizzyFOVPulse * weight;
            mainCam.m_Lens.FieldOfView = Mathf.Lerp(mainCam.m_Lens.FieldOfView, fovTarget, Time.deltaTime * 3f);
        }
        else
        {
            mainCam.m_Lens.FieldOfView = Mathf.Lerp(mainCam.m_Lens.FieldOfView, baseFOV, Time.deltaTime * 2f);
        }

        // === Ruido de Cinemachine: sutil ===
        extraNoiseMul = Mathf.Lerp(extraNoiseMul, 1f + dizzyNoiseBoost * weight, Time.deltaTime * 3f);
        targetExtraFreq = Mathf.Lerp(targetExtraFreq, 1f + 0.35f * weight, Time.deltaTime * 3f);

        // Fin del efecto
        if (dizzyTimer >= dizzyDuration)
            isDizzy = false;
    }


    private float GetDizzyWeight()
    {
        if (!isDizzy || dizzyDuration <= 0f) return 0f;
        float t = Mathf.Clamp01(dizzyTimer / dizzyDuration);
        return dizzyCurve.Evaluate(t) * dizzyIntensity;
    }
    // ======= FIN mareos =======

    public void SetControlesActivos(bool activos)
    {
        controlesActivos = activos;

        Cursor.lockState = activos ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !activos;
    }

    public void SetCamaraActiva(bool activa)
    {
        if (mainPOV == null) return;

        if (activa)
        {
            float sens = GameController.Instance.MouseSensitivity;
            mainPOV.m_HorizontalAxis.m_MaxSpeed = sens;
            mainPOV.m_VerticalAxis.m_MaxSpeed = sens;
        }
        else
        {
            mainPOV.m_HorizontalAxis.m_MaxSpeed = 0f;
            mainPOV.m_VerticalAxis.m_MaxSpeed = 0f;
        }
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

        // Preparar targets
        if (mainCam != null) cinematicCam.Follow = mainCam.Follow;
        cinematicCam.LookAt = lookAt;

        // Activar la vcam
        if (!cinematicCam.gameObject.activeSelf)
            cinematicCam.gameObject.SetActive(true);

        // Subir prioridad
        int basePrio = (mainCam != null) ? mainCam.Priority : 10;
        cinematicCam.Priority = basePrio + 100;

        // Congelar POV de la cámara principal para que no “pelee”
        if (mainPOV != null)
        {
            mainPOV.m_HorizontalAxis.m_MaxSpeed = 0f;
            mainPOV.m_VerticalAxis.m_MaxSpeed = 0f;
        }

        StartCoroutine(LogActiveVcamNextFrame());
    }

    private IEnumerator LogActiveVcamNextFrame()
    {
        yield return null; // esperar un frame al Brain
        if (brain != null && brain.ActiveVirtualCamera != null)
            Debug.Log("[Cam] Activa: " + brain.ActiveVirtualCamera.VirtualCameraGameObject.name);
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

        if (mainPOV != null)
        {
            float sens = GameController.Instance.MouseSensitivity;
            mainPOV.m_HorizontalAxis.m_MaxSpeed = sens;
            mainPOV.m_VerticalAxis.m_MaxSpeed = sens;
        }
    }

    public void ActivarCamaraInspeccion(Transform inspectionPoint)
    {
        this.SetControlesActivos(false);
        GameController.Instance.IsInspecting = true;
        if (brain) brain.enabled = false;
        if (inspectionCam) inspectionCam.gameObject.SetActive(true);
    }

    public void DesactivarCamaraInspeccion()
    {
        GameController.Instance.IsInspecting = false;
        SetControlesActivos(true);
        if (brain) brain.enabled = true;
        if (inspectionCam) inspectionCam.gameObject.SetActive(false);
    }

    public void SetStatusCharacterController(bool status)
    {
        if (controller) controller.enabled = status;
    }

    public Transform GetCameraTransform()
    {
        return camTransform;
    }
}
