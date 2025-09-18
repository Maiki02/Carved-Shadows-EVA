using UnityEngine;

public class PlayerBreathing : MonoBehaviour
{
    [Header("Respiration Settings")]
    public GameObject breathingSourcesParent; // objeto que tiene los 5 AudioSources
    private AudioSource[] breathingSources;   // array interno para manejar los sources

    public float minTimeBetweenBreaths = 5f;
    public float maxTimeBetweenBreaths = 10f;

    [Header("Movement Detection")]
    public CharacterController controller; // O el sistema que uses para mover
    public float movementThreshold = 0.1f; // Sensibilidad al movimiento

    private float breathTimer;
    private float nextBreathTime;
    private Vector3 lastPosition;

    void Start()
    {
        if (breathingSourcesParent != null)
            breathingSources = breathingSourcesParent.GetComponents<AudioSource>();

        if (breathingSources == null || breathingSources.Length == 0)
            Debug.LogWarning("No hay AudioSources asignados para la respiración.");

        lastPosition = transform.position;
        ResetBreathTimer();
    }

    void Update()
    {
        if (controller == null) return;

        // 🔹 Nuevo: chequeo de interacción
        PlayerController playerCtrl = controller.GetComponent<PlayerController>();
        if (playerCtrl != null && playerCtrl.IsInteracting)
        {
            // Detener todos los AudioSources de respiración mientras interactúa
            if (breathingSources != null)
            {
                foreach (AudioSource source in breathingSources)
                {
                    if (source.isPlaying) source.Stop();
                }
            }
            breathTimer = 0f; // reiniciar contador mientras está quieto
            return;
        }

        // Verificamos si el jugador está caminando
        bool isMoving = controller.velocity.magnitude > movementThreshold;

        if (isMoving)
        {
            breathTimer += Time.deltaTime;

            if (breathTimer >= nextBreathTime)
            {
                PlayRandomBreath();
                ResetBreathTimer();
            }
        }
        else
        {
            // Si se detiene, reiniciamos el contador
            breathTimer = 0f;
        }
    }

    void PlayRandomBreath()
    {
        if (breathingSources == null || breathingSources.Length == 0) return;

        int randomIndex = Random.Range(0, breathingSources.Length);

        // reproducir solo si no está sonando ese source
        if (!breathingSources[randomIndex].isPlaying)
            breathingSources[randomIndex].Play();
    }

    void ResetBreathTimer()
    {
        breathTimer = 0f;
        nextBreathTime = Random.Range(minTimeBetweenBreaths, maxTimeBetweenBreaths);
    }
}
