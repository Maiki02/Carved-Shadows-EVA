using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AmbientSound
{
    public string name;
    public AudioSource source;
    public bool isEnabled;
    public float startDelay;
    public bool loop;
    public bool randomizeStart;

    public bool enableFadeOut = false;
    public float fadeOutStartTime = 0f;
    public float fadeOutDuration = 2f;


}

public class AmbientSoundController : MonoBehaviour
{
    public List<AmbientSound> ambientSounds;

    private void Start()
    {
        foreach (var sound in ambientSounds)
        {
            if (sound == null)
            {
                Debug.LogWarning("AmbientSoundController: Se encontró un AmbientSound nulo en la lista.");
                continue;
            }

            if (sound.source == null)
            {
                Debug.LogWarning($"AmbientSoundController: El AudioSource es nulo para el sonido '{sound.name}'. Saltando este sonido.");
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
        // Validación adicional por si el AudioSource se hace null durante la ejecución
        if (sound.source == null)
        {
            Debug.LogWarning($"AmbientSoundController: El AudioSource se volvió nulo para el sonido '{sound.name}' durante PlayWithDelay.");
            yield break;
        }

        float delay = sound.startDelay;
        if (sound.randomizeStart)
        {
            delay += Random.Range(0f, 5f); // podés ajustar el rango
        }

        yield return new WaitForSeconds(delay);

        // Validación antes de reproducir
        if (sound.source == null)
        {
            Debug.LogWarning($"AmbientSoundController: El AudioSource se volvió nulo para el sonido '{sound.name}' después del retraso.");
            yield break;
        }

        sound.source.loop = sound.loop;
        sound.source.Play();

        if (sound.enableFadeOut)
        {
            StartCoroutine(HandleFadeOut(sound));
        }
    }

    public void FadeOutAll(float fadeTime = 2f)
    {
        foreach (var sound in ambientSounds)
        {
            if (sound == null)
            {
                Debug.LogWarning("AmbientSoundController: Se encontró un AmbientSound nulo durante FadeOutAll.");
                continue;
            }

            if (sound.source == null)
            {
                Debug.LogWarning($"AmbientSoundController: El AudioSource es nulo para el sonido '{sound.name}' durante FadeOutAll.");
                continue;
            }

            if (sound.source.isPlaying)
            {
                StartCoroutine(FadeOut(sound.source, fadeTime));
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
        
        if (sound.source == null)
        {
            Debug.LogWarning($"AmbientSoundController: El AudioSource se volvió nulo para el sonido '{sound.name}' durante HandleFadeOut.");
            yield break;
        }

        yield return FadeOut(sound.source, sound.fadeOutDuration);
    }

}
