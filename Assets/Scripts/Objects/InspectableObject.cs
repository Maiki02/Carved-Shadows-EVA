using Cinemachine;
using UnityEngine;

public class InspectableObject : ObjectInteract
{

    public float rotationSpeed = 5f;
    private float distanciaInspeccion = 0.75f;

    protected bool isInspecting = false;
    protected float tiempoDesdeInicio = 0f;
    protected float tiempoDeCancelarInspeccion = 0f; // Tiempo desde que se canceló la inspeccións

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Rigidbody rb;
    private Collider col;

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
        //}
        // Candado
        /* else if (hit.collider.GetComponentInParent<PadlockInspectable>() is PadlockInspectable padlock)
         {
             Debug.Log("Es un candado inspeccionable: " + padlock.name);
             padlock.EntrarInspeccion();
             if (padlock.EstaSiendoInspeccionado())
             {
                 Debug.Log("Modo inspección del candado iniciado.");
                 candadoActual = padlock;
             }
             else
             {
                 Debug.LogWarning("¡No se inició la inspección del candado!");
             }
         }*/
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

        // 2) Nivelar rotación de la Main Camera
        this.NivelarCamara();

        // 3) Desanidar y resetear escala
        transform.SetParent(null);
        transform.localScale = Vector3.one;

        // 4) Mover frente a la cámara principal del jugador
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 posicionFrenteCamara = cam.transform.position + cam.transform.forward * distanciaInspeccion;
            transform.position = posicionFrenteCamara;
            // Opcional: alinear rotación con la cámara, pero sin girar en X/Z para que quede "recto"
            transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        }
        else
        {
            Debug.LogWarning("No se encontró la cámara principal para inspección. Se usará el punto de inspección por defecto.");
            if (inspeccionDestino != null)
            {
                transform.position = inspeccionDestino.position;
                transform.rotation = Quaternion.identity;
            }
        }

        // 5) Desactivar físicas y colisiones
        if (rb != null) { rb.isKinematic = true; rb.detectCollisions = false; }
        if (col != null) col.enabled = false;

        // 6) Subir prioridad de la cámara de inspección
        /*if (inspectionCamera != null)
            inspectionCamera.Priority = 20;*/
        Debug.Log("Iniciar inspección del objeto: " + this.name);
        // 7) Bloquear controles del jugador
        this.ActivarInspeccion();
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

    public void NivelarCamara()
    {
        if (Camera.main != null)
            Camera.main.transform.localRotation = Quaternion.identity;
    }

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
