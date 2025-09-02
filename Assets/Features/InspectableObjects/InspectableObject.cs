using Cinemachine;
using UnityEngine;

public class InspectableObject : ObjectInteract
{

    [Header("Inspección")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float distanciaInspeccion = 0.75f;

    protected bool isInspecting = false;
    protected float tiempoDesdeInicio = 0f;
    protected float tiempoDeCancelarInspeccion = 0f; // Tiempo desde que se canceló la inspeccións

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Rigidbody rb;
    private Collider col;

    [Header("Punto de inspección")]
    [SerializeField] protected Transform inspectionPoint;

    protected override void Awake()
    {
        base.Awake();


        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        this.inspectionPoint = GameObject.FindGameObjectWithTag("InspectionPoint").GetComponent<Transform>();
    }


    void Start()
    {

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        this.tiempoDeCancelarInspeccion += Time.deltaTime;
        if (isInspecting)
        {

            tiempoDesdeInicio += Time.deltaTime;

            // Rotar con el mouse mientras se mantiene presionado el botón izquierdo
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                transform.Rotate(Vector3.up, -mouseX * rotationSpeed, Space.World);
                transform.Rotate(Vector3.right, mouseY * rotationSpeed, Space.World);
            }

            if (Input.GetKeyDown(KeyCode.E) && tiempoDesdeInicio > 0.2f)
            {
                FinalizarInspeccion();
                this.tiempoDeCancelarInspeccion = 0f;
                Debug.Log("Inspección cancelada con Escape.");
            }

        }
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        Debug.Log("Hover enter en objeto inspeccionable: " + this.name);
    }

    public override void OnInteract()
    {
        if (this.tiempoDeCancelarInspeccion < 0.5f) return;

        if (isInspecting)
        {
            FinalizarInspeccion();
            Debug.Log("Inspección finalizada.");
            return;
        }
        else
        {

            IniciarInspeccion(inspectionPoint);
            if (EstaSiendoInspeccionado())
            {
                Debug.Log("Inspección iniciada correctamente.");
                //objetoActual = inspect;
            }
        }
    }

    public void IniciarInspeccion(Transform inspeccionDestino)
    {
        if (isInspecting) return;
        isInspecting = true;
        tiempoDesdeInicio = 0f;

        // 1) Guardar estado original
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // 2) Activar el sistema de inspección de Cinemachine PRIMERO
        this.ActivarInspeccion();

        // 3) Desanidar y resetear escala
        transform.SetParent(null);
        transform.localScale = Vector3.one;

        // 4) Obtener la posición de la cámara activa desde el PlayerController
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null && jugador.TryGetComponent(out PlayerController pc))
        {
            Transform cameraTransform = pc.GetActiveCameraTransform();
            if (cameraTransform != null)
            {
                // Posicionar el objeto frente a la cámara actual
                Vector3 posicionFrenteCamara = cameraTransform.position + cameraTransform.forward * distanciaInspeccion;
                transform.position = posicionFrenteCamara;
                // Alinear rotación con la cámara, pero sin girar en X/Z para que quede "recto"
                transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            }
            else
            {
                Debug.LogWarning("No se pudo obtener la cámara del PlayerController.");
                // Fallback: usar InspectionPoint si está disponible
                if (inspeccionDestino != null)
                {
                    transform.position = inspeccionDestino.position;
                    transform.rotation = Quaternion.identity;
                }
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el PlayerController. Se usará el punto de inspección por defecto.");
            if (inspeccionDestino != null)
            {
                transform.position = inspeccionDestino.position;
                transform.rotation = Quaternion.identity;
            }
        }

        // 5) Desactivar físicas y colisiones
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        if (col != null) col.enabled = false;

        Debug.Log("Iniciar inspección del objeto: " + this.name);
    }

    public void FinalizarInspeccion()
    {
        isInspecting = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.SetParent(originalParent);

        // Restaurar físicas y colisiones
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        if (col != null)
        {
            col.enabled = true;
        }

        // Restaurar controles del jugador
        this.DesactivarInspeccion();
    }

    public bool EstaSiendoInspeccionado() => isInspecting;

    public void ActivarInspeccion()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null && jugador.TryGetComponent(out PlayerController pc))
            pc.ActivarCamaraInspeccion(this.transform);
    }

    public void DesactivarInspeccion()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null && jugador.TryGetComponent(out PlayerController pc))
            pc.DesactivarCamaraInspeccion();
    }
}
