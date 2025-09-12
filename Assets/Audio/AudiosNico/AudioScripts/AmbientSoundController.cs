using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AmbientSound
{
    public string name;
    public SoundEvent soundEvent;        // Antes era AudioSource
    public bool isEnabled;
    public float startDelay;
    public bool loop;
    public bool randomizeStart;

    public bool enableFadeOut = false;
    public float fadeOutStartTime = 0f;
    public float fadeOutDuration = 2f;

    [HideInInspector]
    public AudioSource runtimeSource;     // El AudioSource generado en runtime
}

public class AmbientSoundController : MonoBehaviour
{
    public List<AmbientSound> ambientSounds;

    private void Start()
    {
        foreach (var sound in ambientSounds)
        {
            if (sound == null || sound.soundEvent == null)
            {
                Debug.LogWarning("AmbientSoundController: AmbientSound o SoundEvent nulo en la lista.");
                continue;
            }

            if (sound.isEnabled)
            {
                StartCoroutine(PlayWithDelay(sound));
            }
        }
    }

    private IEnumerator PlayWithDelay(AmbientSound sound)
    {
        float delay = sound.startDelay;
        if (sound.randomizeStart)
        {
            delay += Random.Range(0f, 5f);
        }

        yield return new WaitForSeconds(delay);

        // Reproducir con el helper
        sound.runtimeSource = SoundEventPlayer.Play(sound.soundEvent, transform);
        if (sound.runtimeSource == null)
        {
            Debug.LogWarning($"AmbientSoundController: No se pudo reproducir SoundEvent '{sound.name}'.");
            yield break;
        }

        // Ajustar loop según configuración del AmbientSound (sobrescribe lo que venga del SO)
        sound.runtimeSource.loop = sound.loop;

        if (sound.enableFadeOut)
        {
            StartCoroutine(HandleFadeOut(sound));
        }
    }

    public void FadeOutAll(float fadeTime = 2f)
    {
        foreach (var sound in ambientSounds)
        {
            if (sound == null || sound.runtimeSource == null) continue;
            if (sound.runtimeSource.isPlaying)
            {
                StartCoroutine(FadeOut(sound.runtimeSource, fadeTime));
            }
        }
    }

    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    private IEnumerator HandleFadeOut(AmbientSound sound)
    {
        yield return new WaitForSeconds(sound.fadeOutStartTime);

        if (sound.runtimeSource == null) yield break;

        yield return FadeOut(sound.runtimeSource, sound.fadeOutDuration);
    }
}
