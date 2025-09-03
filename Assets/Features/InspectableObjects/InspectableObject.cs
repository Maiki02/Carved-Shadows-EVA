using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class InspectableObject : ObjectInteract
{

    [Header("Inspección")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float distanciaInspeccion = 0.75f;
    [SerializeField] private float escalaInspeccion = 1f; // Escala durante la inspección
    [SerializeField] private bool ajustarEscalaAutomaticamente = true; // Auto-ajustar según tamaño del objeto

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

            // Ya no necesitamos mantener la posición porque la cámara está congelada
            // MantenerPosicionFrenteCamara(); // Comentado porque la cámara ya no se mueve

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

        // 2) Activar el modo inspección (congela la cámara principal)
        this.ActivarInspeccion();

        // 3) Desanidar del parent
        transform.SetParent(null);

        // 4) Calcular escala apropiada para la inspección
        Vector3 nuevaEscala = originalScale;
        if (ajustarEscalaAutomaticamente)
        {
            // Obtener el tamaño del bounding box del objeto
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                
                // Escalar para que la dimensión más grande sea aproximadamente 0.5 unidades
                float targetSize = 0.5f;
                float scaleMultiplier = targetSize / maxDimension;
                nuevaEscala = originalScale * scaleMultiplier;
            }
        }
        else
        {
            nuevaEscala = originalScale * escalaInspeccion;
        }
        
        transform.localScale = nuevaEscala;

        // 5) Posicionar el objeto frente a la cámara principal (que ahora está congelada)
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null && jugador.TryGetComponent(out PlayerController pc))
        {
            Transform cameraTransform = pc.GetActiveCameraTransform();
            if (cameraTransform != null)
            {
                // Posicionar el objeto frente a la cámara principal congelada
                Vector3 posicionFrenteCamara = cameraTransform.position + cameraTransform.forward * distanciaInspeccion;
                transform.position = posicionFrenteCamara;
                // Alinear rotación inicial con la cámara
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

        // 6) Desactivar físicas y colisiones
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        if (col != null) col.enabled = false;

        Debug.Log("Iniciar inspección del objeto: " + this.name);
    }

    public void FinalizarInspeccion()
    {
        isInspecting = false;

        // Restaurar posición, rotación, parent y escala originales
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.SetParent(originalParent);
        transform.localScale = originalScale; // Restaurar la escala original

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
