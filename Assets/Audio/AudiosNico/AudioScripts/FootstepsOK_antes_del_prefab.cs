using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// public class FootstepAudioManager : MonoBehaviour
// {
//     [Header("Clips de pasos")]
//     public List<AudioClip> pasos;
//     public List<AudioClip> beats;

//     [Header("Audio Sources")]
//     public AudioSource pasosSource;
//     public AudioSource beatsSource;

//     [Header("Audio Mixer")]
//     public AudioMixerGroup pasosMixer;
//     public AudioMixerGroup beatsMixer;

//     [Header("Configuración de pasos")]
//     public float stepInterval = 0.5f;
//     public float minMovementTime = 0.15f; // tiempo mínimo que se debe estar moviendo para que se dispare un paso

//     private float stepTimer;
//     private float movementTime;
//     private int lastIndex = -1;

//     private CharacterController characterController;

    

//     void Start()
//     {
//         characterController = GetComponent<CharacterController>();
//         pasosSource.outputAudioMixerGroup = pasosMixer;
//         beatsSource.outputAudioMixerGroup = beatsMixer;
//     }

//     void Update()
//     {
//         Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
//         bool isWalking = horizontalVelocity.magnitude > 0.1f && characterController.isGrounded;

//         if (isWalking)
//         {
//             movementTime += Time.deltaTime;
//             stepTimer -= Time.deltaTime;

//             if (movementTime >= minMovementTime && stepTimer <= 0f)
//             {
//                 PlayRandomStepPair();
//                 stepTimer = stepInterval;
//             }
//         }
//         else
//         {
//             movementTime = 0f;
//             stepTimer = 0f;
//         }
//     }

//     void PlayRandomStepPair()
//     {
//         if (pasos.Count == 0 || beats.Count == 0) return;

//         int index;
//         do
//         {
//             index = Random.Range(0, pasos.Count);
//         } while (index == lastIndex && pasos.Count > 1);
//         lastIndex = index;

//         // Variación de pitch para ambos
//         float randomPitch = Random.Range(0.95f, 1.05f);
//         pasosSource.pitch = randomPitch;
//         beatsSource.pitch = randomPitch;

//         pasosSource.clip = pasos[index];
//         beatsSource.clip = beats[index];

//         pasosSource.Play();
//         beatsSource.Play();
//     }
// }