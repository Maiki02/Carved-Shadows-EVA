using System.Collections;
using UnityEngine;

/// <summary>
/// Trigger que activa una secuencia de niña, estatua y piano.
/// Al entrar el jugador, activa los gameObjects especificados por un tiempo determinado
/// y reproduce un sonido de piano.
/// </summary>
public class GirlStatuePianoTrigger : MonoBehaviour
{
    [Header("Referencias de GameObjects")]
    [SerializeField] private GameObject girlObject; // Objeto de la niña
    [SerializeField] private GameObject statueObject; // Objeto de la estatua

    [Header("Configuración de activación")]
    [SerializeField] private bool activateGirl = true; // Si activar la niña
    [SerializeField] private bool activateStatue = true; // Si activar la estatua
    [SerializeField] private float activationDuration = 4f; // Tiempo que permanecen activos

    [Header("Configuración de audio")]
    [SerializeField] private AudioClip pianoSound; // Sonido del piano a reproducir
    [SerializeField] private AudioSource audioSource; // AudioSource para reproducir el sonido
    [SerializeField] private float pianoVolume = 1.25f; // Volumen del piano

    [Header("Configuración del trigger")]
    [SerializeField] private bool triggerOnce = true; // Si se activa solo una vez

    // Control interno
    private bool hasTriggered = false; // Control para evitar múltiples activaciones
    private Coroutine activationCoroutine; // Referencia a la corrutina actual

    private void Start()
    {
        // Validaciones iniciales
        ValidateReferences();

        // Asegurar que los objetos estén inactivos al inicio
        if (girlObject != null && activateGirl)
        {
            girlObject.SetActive(false);
        }

        if (statueObject != null && activateStatue)
        {
            statueObject.SetActive(false);
        }

        // Configurar AudioSource si no está asignado
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        if (!other.CompareTag("Player")) return;

        // Verificar si ya se activó (si triggerOnce está activo)
        if (triggerOnce && hasTriggered) return;

        Debug.Log("Trigger activado - Iniciando secuencia de niña, estatua y piano");

        // Marcar como activado si es de una sola vez
        if (triggerOnce)
        {
            hasTriggered = true;
        }

        // Ejecutar la secuencia
        ExecuteSequence();
    }

    /// <summary>
    /// Ejecuta la secuencia completa: activa objetos y reproduce sonido
    /// </summary>
    private void ExecuteSequence()
    {
        // Detener corrutina anterior si existe
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }

        // Iniciar nueva secuencia
        activationCoroutine = StartCoroutine(ActivationSequence());

        // Reproducir sonido del piano
        PlayPianoSound();
    }

    /// <summary>
    /// Corrutina que maneja la activación temporal de los objetos
    /// </summary>
    private IEnumerator ActivationSequence()
    {
        // Activar la niña si está configurado
        if (girlObject != null && activateGirl)
        {
            girlObject.SetActive(true);
        }

        // Activar la estatua si está configurado
        if (statueObject != null && activateStatue)
        {
            statueObject.SetActive(true);
        }
        Debug.Log("Duración de activación: " + activationDuration + " segundos");
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(activationDuration);

        // Desactivar la niña si estaba activada
        if (girlObject != null && activateGirl)
        {
            girlObject.SetActive(false);
        }

        // Desactivar la estatua si estaba activada
        if (statueObject != null && activateStatue)
        {
            statueObject.SetActive(false);
        }

    }

    /// <summary>
    /// Reproduce el sonido del piano
    /// </summary>
    private void PlayPianoSound()
    {
        if (pianoSound != null && audioSource != null)
        {
            audioSource.clip = pianoSound;
            audioSource.volume = pianoVolume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No se puede reproducir el sonido del piano - Clip o AudioSource faltante");
        }
    }

    /// <summary>
    /// Valida que las referencias necesarias estén asignadas
    /// </summary>
    private void ValidateReferences()
    {
        if (girlObject == null && activateGirl)
        {
            Debug.LogWarning("Objeto de la niña no asignado pero está marcado para activarse");
        }

        if (statueObject == null && activateStatue)
        {
            Debug.LogWarning("Objeto de la estatua no asignado pero está marcado para activarse");
        }

        if (pianoSound == null)
        {
            Debug.LogWarning("Sonido del piano no asignado");
        }

        if (activationDuration <= 0)
        {
            Debug.LogWarning("Duración de activación debe ser mayor a 0");
            activationDuration = 1f;
        }
    }

    /// <summary>
    /// Método público para resetear el trigger (útil para testing)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("Trigger reseteado - puede activarse nuevamente");
    }

    /// <summary>
    /// Método público para activar manualmente la secuencia
    /// </summary>
    public void ManuallyTriggerSequence()
    {
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("Trigger ya se activó anteriormente y está configurado para una sola activación");
            return;
        }

        Debug.Log("Activación manual de la secuencia");
        ExecuteSequence();

        if (triggerOnce)
        {
            hasTriggered = true;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Dibuja el área del trigger en la vista de escena
    /// </summary>
    private void OnDrawGizmos()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = hasTriggered ? Color.gray : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (triggerCollider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }
        }
    }
#endif
}
