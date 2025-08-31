using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FakeDoor : MonoBehaviour
{
    [Header("Apertura")]
    [SerializeField] private float openDegreesY = 110f;
    [SerializeField] private float openDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;

    private AudioSource audioSource;
    private Quaternion initialRotation;
    private Coroutine openRoutine;

    private void Awake()
    {
        initialRotation = transform.rotation;
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Llamar a este método para abrir la puerta.
    /// </summary>
    public void PlayDoorOpen()
    {
        Debug.Log("[FakeDoor] Reproduciendo animación de apertura...");
        if (openRoutine != null) StopCoroutine(openRoutine);
        openRoutine = StartCoroutine(OpenDoorCoroutine());
    }

    private IEnumerator OpenDoorCoroutine()
    {
        Debug.Log("[FakeDoor] Comenzando rotación de puerta...");
        // Reproducir sonido de apertura
        if (openSound != null && audioSource != null)
        {
            audioSource.clip = openSound;
            audioSource.Play();
        }

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y + openDegreesY, startRot.eulerAngles.z);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            float t = Mathf.Clamp01(elapsed / openDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;

        yield return new WaitForSeconds(1f);
        transform.root.gameObject.SetActive(false);
    }
}
