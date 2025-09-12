using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class AmbientSoundTrigger : MonoBehaviour
{
    [Tooltip("Si se asigna, este SoundEvent se reproducirá al activarse.")]
    public SoundEvent soundEvent;

    [Tooltip("Tag del jugador que activa el trigger.")]
    public string playerTag = "Player";

    private AudioSource runtimeSource;
    private bool hasPlayed = false;

    private void Awake()
    {
        // Asegurar configuración del Rigidbody y Collider
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (soundEvent == null)
        {
            Debug.LogWarning($"{name}: No se asignó ningún SoundEvent en el Inspector.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;
        if (!other.CompareTag(playerTag)) return;
        if (soundEvent == null) return;

        // Reproducir el SoundEvent usando el helper
        runtimeSource = SoundEventPlayer.Play(soundEvent, transform);

        hasPlayed = true;
    }
}
