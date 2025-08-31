using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class AmbientSoundTrigger : MonoBehaviour
{
    [Tooltip("Si se asigna, este AudioSource se usará en lugar del que está en los hijos.")]
    public AudioSource customAudioSource;

    [Tooltip("Tag del jugador que activa el trigger.")]
    public string playerTag = "Player";

    private AudioSource audioSource;
    private bool hasPlayed = false;

    private void Awake()
    {
        // Usar AudioSource asignado o buscar uno en los hijos
        audioSource = customAudioSource != null ? customAudioSource : GetComponentInChildren<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning($"{name}: No se encontró un AudioSource ni se asignó uno en el Inspector.");
        }

        // Asegurar configuración del Rigidbody y Collider
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;
        if (!other.CompareTag(playerTag)) return;
        if (audioSource == null) return;

        audioSource.Play();
        hasPlayed = true;
    }
}
