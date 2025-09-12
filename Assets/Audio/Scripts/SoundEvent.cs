using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSoundEvent", menuName = "Audio/SoundEvent")]
public class SoundEvent : ScriptableObject
{
    public AudioClip clip;                  // Clip de audio asociado
    public float volume = 1f;               // Volumen por defecto
    public float spatialBlend = 1f;         // 0 = 2D, 1 = 3D
    public AudioMixerGroup mixerGroup;      // Mixer al que pertenece
    public bool loop = false;               // Loop o no
}
