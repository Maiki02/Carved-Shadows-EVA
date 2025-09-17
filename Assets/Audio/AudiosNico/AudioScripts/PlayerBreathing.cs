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
        if (!breathingSources[randomIndex].isPlaying) // para que no se corte el audio
            breathingSources[randomIndex].Play();
    }

    void ResetBreathTimer()
    {
        breathTimer = 0f;
        nextBreathTime = Random.Range(minTimeBetweenBreaths, maxTimeBetweenBreaths);
    }
}

