using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectSound : MonoBehaviour
{
    private AudioSource _audioSource;
    [Header("Configuración del sonido")]
    [Tooltip("Sonido en 3D")]
    [SerializeField] private float spatialBlend = 1f; // Sonido 3D
    [Tooltip("Si el sonido debe repetirse en bucle")]
    [SerializeField] private bool loop = true;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = spatialBlend; // Configura el sonido como 3D
        _audioSource.loop = loop; // Configura el sonido para que se repita en bucle
    }

    void Start()
    {
        _audioSource.Play();
        _audioSource.volume = AudioController.Instance.SfxVolume; //Se ajusta el volumen del sonido del reloj al volumen de SFX

    }


    public void StopSound() {
        _audioSource.Stop();
    } 
}
