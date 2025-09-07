using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InspectableObject : ObjectInteract
{
    [Header("Inspección")]
    [SerializeField] private float distanciaInspeccion = 0.75f;
    [SerializeField] private float escalaInspeccion = 1f;
    [SerializeField] private bool ajustarEscalaAutomaticamente = true;

    [Header("Rotación con Look")]
    // Si los dejas vacíos, se configuran solos desde PlayerInput.
    [SerializeField] private InputActionReference lookActionRef;
    [SerializeField] private InputActionReference interactActionRef;
    [SerializeField] private InputActionReference cancelActionRef;

    [Tooltip("Ruta de las acciones en el asset (auto). Formato: 'Mapa/Accion' o 'Accion'.")]
    [SerializeField] private string lookActionPath = "Gameplay/Look";
    [SerializeField] private string interactActionPath = "Gameplay/Interact";
    [SerializeField] private string cancelActionPath = "Gameplay/Cancel";

    [Header("Sensibilidad")]
    [SerializeField] private float mouseSensitivity = 4.0f;
    [SerializeField] private float stickDegreesPerSecond = 180f;
    [SerializeField] private float stickDeadzone = 0.15f;
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = true;

    protected bool isInspecting = false;
    protected float tiempoDesdeInicio = 0f;
    protected float tiempoDeCancelarInspeccion = 0f;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Rigidbody rb;
    private Collider col;

    [Header("Punto de inspección")]
    [SerializeField] protected Transform inspectionPoint;

    // Ejes de referencia congelados al iniciar inspección
    private Transform refCam;
    private Vector3 refRight, refUp;

    // ---- cache estático compartido por todos los inspectables ----
    private static bool sActionsResolved = false;
    private static InputAction sLook, sInteract, sCancel;
    private static bool sLoggedMissingOnce = false;

    // instancias locales (usan refs explícitas o el cache)
    private InputAction lookAction, interactAction, cancelAction;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (inspectionPoint == null)
        {
            var go = GameObject.FindGameObjectWithTag("InspectionPoint");
            if (go) inspectionPoint = go.transform;
        }
    }

    void OnEnable()
    {
        ResolveActionsIfNeeded();

        lookAction?.Enable();
        interactAction?.Enable();
        cancelAction?.Enable();
    }

    void OnDisable()
    {
        lookAction?.Disable();
        interactAction?.Disable();
        cancelAction?.Disable();
    }

    void Update()
    {
        // usamos unscaled para que funcione bien si hay UIs/pausas
        tiempoDeCancelarInspeccion += Time.unscaledDeltaTime;

        if (!isInspecting) return;

        tiempoDesdeInicio += Time.unscaledDeltaTime;

        RotateWithLook();

        bool closePressed =
            (interactAction != null && interactAction.WasPressedThisFrame()) ||
            (cancelAction != null && cancelAction.WasPressedThisFrame());

        if (closePressed && tiempoDesdeInicio > 0.2f)
        {
            FinalizarInspeccion();
            tiempoDeCancelarInspeccion = 0f;
        }
    }

    public override void OnInteract()
    {
        if (tiempoDeCancelarInspeccion < 0.5f) return;

        if (isInspecting)
        {
            FinalizarInspeccion();
            return;
        }

        IniciarInspeccion(inspectionPoint);
    }

    public void IniciarInspeccion(Transform inspeccionDestino)
    {
        if (isInspecting) return;
        isInspecting = true;
        tiempoDesdeInicio = 0f;

        // guardar estado original
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // congelar control del jugador / cámara
        ActivarInspeccion();

        // ejes de referencia (cámara del player en el momento de iniciar)
        refCam = ObtenerCamaraJugador();
        if (refCam != null)
        {
            refRight = refCam.right;
            refUp = refCam.up;
        }
        else
        {
            refRight = Vector3.right;
            refUp = Vector3.up;
        }

        // desanidar
        transform.SetParent(null);

        // escala para inspección
        Vector3 nuevaEscala = originalScale;
        if (ajustarEscalaAutomaticamente)
        {
            Renderer rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Bounds b = rend.bounds;
                float maxDim = Mathf.Max(b.size.x, b.size.y, b.size.z);
                float targetSize = 0.5f;
                float mul = (maxDim > 0.0001f) ? (targetSize / maxDim) : 1f;
                nuevaEscala = originalScale * mul;
            }
        }
        else
        {
            nuevaEscala = originalScale * escalaInspeccion;
        }
        transform.localScale = nuevaEscala;

        // posicionar frente a la cámara
        var camT = refCam != null ? refCam : transform;
        transform.position = camT.position + camT.forward * distanciaInspeccion;
        transform.rotation = Quaternion.Euler(0f, camT.eulerAngles.y, 0f);

        // físicas off
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        if (col != null) col.enabled = false;
    }

    public void FinalizarInspeccion()
    {
        isInspecting = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.SetParent(originalParent);
        transform.localScale = originalScale;

        if (rb != null) { rb.isKinematic = false; rb.detectCollisions = true; }
        if (col != null) col.enabled = true;

        DesactivarInspeccion();
    }

    public bool EstaSiendoInspeccionado() => isInspecting;

    public void ActivarInspeccion()
    {
        var pc = ObtenerPlayerController();
        pc?.ActivarCamaraInspeccion(transform);
    }

    public void DesactivarInspeccion()
    {
        var pc = ObtenerPlayerController();
        pc?.DesactivarCamaraInspeccion();
    }

    // ------------ helpers ------------

    private void ResolveActionsIfNeeded()
    {
        // 1) ¿tiene refs explícitas? úsrlas
        if (lookActionRef != null) lookAction = lookActionRef.action;
        if (interactActionRef != null) interactAction = interactActionRef.action;
        if (cancelActionRef != null) cancelAction = cancelActionRef.action;

        // 2) si falta alguna, resolvemos cache global desde PlayerInput
        if (lookAction != null && interactAction != null && cancelAction != null) return;

        if (!sActionsResolved)
        {
            var pi = FindPlayerInput();
            if (pi != null && pi.actions != null)
            {
                // Buscar por ruta "Mapa/Accion" y fallback por nombre simple
                sLook = pi.actions.FindAction(lookActionPath, false) ?? pi.actions.FindAction("Look", false);
                sInteract = pi.actions.FindAction(interactActionPath, false) ?? pi.actions.FindAction("Interact", false);
                sCancel = pi.actions.FindAction(cancelActionPath, false) ?? pi.actions.FindAction("Cancel", false);
                sActionsResolved = true;

                // Enable por si el mapa no las habilitó aún (no pasa nada si ya están enable)
                sLook?.Enable(); sInteract?.Enable(); sCancel?.Enable();
            }
            else if (!sLoggedMissingOnce)
            {
                Debug.LogWarning("[InspectableObject] No se encontró PlayerInput ni acciones. Asigná refs o revisa el Input.");
                sLoggedMissingOnce = true;
            }
        }

        // Completar las que falten con el cache
        lookAction ??= sLook;
        interactAction ??= sInteract;
        cancelAction ??= sCancel;
    }

    private static PlayerInput FindPlayerInput()
    {
        // Prioriza el Player
        var player = GameObject.FindWithTag("Player");
        if (player && player.TryGetComponent(out PlayerInput piFromPlayer))
            return piFromPlayer;

        // Fallback: buscar en escena
        PlayerInput[] all;
#if UNITY_2023_1_OR_NEWER
        all = Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
#else
    all = Object.FindObjectsOfType<PlayerInput>(true);
#endif
        return (all != null && all.Length > 0) ? all[0] : null;
    }

    private void RotateWithLook()
    {
        if (lookAction == null) return;

        Vector2 look = lookAction.ReadValue<Vector2>();

        bool usingMouse = Mouse.current != null && Mouse.current.delta.IsActuated();
        bool usingStick = !usingMouse && Gamepad.current != null && Gamepad.current.rightStick.IsActuated();

        if (usingStick && look.sqrMagnitude < stickDeadzone * stickDeadzone) return;

        if (invertX) look.x = -look.x;
        if (invertY) look.y = -look.y;

        float yawDelta, pitchDelta;
        if (usingMouse)
        {
            yawDelta = look.x * mouseSensitivity;
            pitchDelta = look.y * mouseSensitivity;
        }
        else
        {
            float dt = Time.unscaledDeltaTime;
            yawDelta = look.x * stickDegreesPerSecond * dt;
            pitchDelta = look.y * stickDegreesPerSecond * dt;
        }

        Vector3 upAxis = (refUp.sqrMagnitude > 0.001f) ? refUp : Vector3.up;
        Vector3 rightAxis = (refRight.sqrMagnitude > 0.001f) ? refRight : Vector3.right;

        transform.Rotate(upAxis, yawDelta, Space.World);
        transform.Rotate(rightAxis, -pitchDelta, Space.World);
    }

    private Transform ObtenerCamaraJugador()
    {
        var pc = ObtenerPlayerController();
        return pc != null ? pc.GetActiveCameraTransform() : Camera.main?.transform;
    }

    private PlayerController ObtenerPlayerController()
    {
        var player = GameObject.FindWithTag("Player");
        if (!player) return null;
        player.TryGetComponent(out PlayerController pc);
        return pc;
    }
}
