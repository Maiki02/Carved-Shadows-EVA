using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootstepAudioManager : MonoBehaviour
{
    [Header("Clips de pasos")]
    public List<AudioClip> pasos;
    public List<AudioClip> beats;

    [Header("Clips de pasos para alfombra")]
    public List<AudioClip> alfombraPasos;
    public List<AudioClip> alfombraBeats;

    [Header("Audio Sources")]
    public AudioSource pasosSource;
    public AudioSource beatsSource;

    [Header("Audio Mixer")]
    public AudioMixerGroup pasosMixer;
    public AudioMixerGroup beatsMixer;

    [Header("Configuración de pasos")]
    public float stepInterval = 0.5f;
    public float minMovementTime = 0.15f;

    private float stepTimer;
    private float movementTime;
    private int lastIndex = -1;
    private bool sobreAlfombra = false;

    private CharacterController characterController;

    [Header("Escaleras")]
    public float stairStepInterval = 0.2f; // Intervalo cuando baja escaleras
    public float stairPitchBoost = 1.05f;  // Opcional: le subimos un poquito el pitch

    private float defaultStepInterval;
    private bool enEscaleras = false;

    void Start()
    {
        characterController = GetComponentInParent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("No se encontró CharacterController en el objeto padre.");
        }

        if (pasosSource != null && pasosMixer != null)
            pasosSource.outputAudioMixerGroup = pasosMixer;

        if (beatsSource != null && beatsMixer != null)
            beatsSource.outputAudioMixerGroup = beatsMixer;

        defaultStepInterval = stepInterval;
    }

    void Update()
    {
        if (characterController == null) return;

        bool isWalking;

        if (enEscaleras)
        {
            // En escaleras usamos la velocidad total (incluye Y) para no cortar pasos al bajar
            isWalking = characterController.velocity.magnitude > 0.1f && characterController.isGrounded;
            stepInterval = stairStepInterval; // siempre ritmo rápido en escaleras
        }
        else
        {
            // En suelo plano/alfombra usamos solo velocidad horizontal
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
            isWalking = horizontalVelocity.magnitude > 0.1f && characterController.isGrounded;
            stepInterval = defaultStepInterval;
        }

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
        List<AudioClip> currentPasos = sobreAlfombra ? alfombraPasos : pasos;
        List<AudioClip> currentBeats = sobreAlfombra ? alfombraBeats : beats;

        if (currentPasos.Count == 0 || currentBeats.Count == 0) return;

        int index;
        do
        {
            index = Random.Range(0, currentPasos.Count);
        } while (index == lastIndex && currentPasos.Count > 1);
        lastIndex = index;

        float basePitch = Random.Range(0.95f, 1.05f);
        if (enEscaleras) basePitch *= stairPitchBoost; // Un poco más agudo en escaleras

        pasosSource.pitch = basePitch;
        beatsSource.pitch = basePitch;

        pasosSource.clip = currentPasos[index];
        beatsSource.clip = currentBeats[index];

        pasosSource.Play();
        beatsSource.Play();
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
