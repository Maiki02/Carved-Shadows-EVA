using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootstepAudioManager : MonoBehaviour
{
    [Header("Sound Events de pasos")]
    public List<SoundEvent> pasos;
    public List<SoundEvent> beats;

    [Header("Sound Events de pasos para alfombra")]
    public List<SoundEvent> alfombraPasos;
    public List<SoundEvent> alfombraBeats;

    [Header("Audio Sources para control de loop/pitch (opcional)")]
    public AudioSource pasosSource;
    public AudioSource beatsSource;

    [Header("Configuración de pasos")]
    public float stepInterval = 0.5f;
    public float minMovementTime = 0.15f;

    private float stepTimer;
    private float movementTime;
    private int lastIndex = -1;
    private bool sobreAlfombra = false;

    private CharacterController characterController;

    [Header("Escaleras")]
    public float stairStepInterval = 0.2f;
    public float stairPitchBoost = 1.05f;

    private float defaultStepInterval;
    private bool enEscaleras = false;

    void Start()
    {
        characterController = GetComponentInParent<CharacterController>();
        if (characterController == null)
            Debug.LogError("No se encontró CharacterController en el objeto padre.");

        defaultStepInterval = stepInterval;
    }

    void Update()
    {
        if (characterController == null) return;

        // 🔹 Nuevo: si el player está interactuando, no reproducir pasos
        if (characterController.GetComponent<PlayerController>().IsInteracting)
        {
            pasosSource.Stop();
            beatsSource.Stop();
            return;
        }

        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        bool isWalking = horizontalVelocity.magnitude > 0.1f && characterController.isGrounded;

        if (isWalking)
        {
            movementTime += Time.deltaTime;
            stepTimer -= Time.deltaTime;

            if (movementTime >= minMovementTime && stepTimer <= 0f)
            {
                PlayRandomStepPair();
                stepTimer = stepInterval;
            }
        }
        else
        {
            movementTime = 0f;
            stepTimer = 0f;
        }
    }

    void PlayRandomStepPair()
    {
        List<SoundEvent> currentPasos = sobreAlfombra ? alfombraPasos : pasos;
        List<SoundEvent> currentBeats = sobreAlfombra ? alfombraBeats : beats;

        if (currentPasos.Count == 0 || currentBeats.Count == 0) return;

        int index;
        do
        {
            index = Random.Range(0, currentPasos.Count);
        } while (index == lastIndex && currentPasos.Count > 1);
        lastIndex = index;

        float basePitch = Random.Range(0.95f, 1.05f);
        if (enEscaleras) basePitch *= stairPitchBoost;

        // Reproducir con helper
        var pasoSrc = SoundEventPlayer.Play(currentPasos[index]);
        var beatSrc = SoundEventPlayer.Play(currentBeats[index]);
        Debug.Log("Reproduciendo paso: " + currentPasos[index].name + " / beat: " + currentBeats[index].name);


        // Ajustar pitch y loop si se desea (opcional)
        if (pasoSrc != null) pasoSrc.pitch = basePitch;
        if (beatSrc != null) beatSrc.pitch = basePitch;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Alfombra"))
        {
            sobreAlfombra = true;
        }
        else if (other.CompareTag("Escaleras"))
        {
            enEscaleras = true;
            stepInterval = stairStepInterval;
        }
    }

    private void OnTriggerExit(Collider other)
    { 
        if (other.CompareTag("Alfombra"))
        {
            sobreAlfombra = false;
        }
        else if (other.CompareTag("Escaleras"))
        {
            enEscaleras = false;
            stepInterval = defaultStepInterval;
        }
    }
}
