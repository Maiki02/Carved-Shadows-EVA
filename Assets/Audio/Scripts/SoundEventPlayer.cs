using UnityEngine;

public static class SoundEventPlayer
{
    /// <summary>
    /// Reproduce un SoundEvent creando un AudioSource temporal.
    /// </summary>
    public static AudioSource Play(SoundEvent soundEvent, Transform parent = null)
    {
        if (soundEvent == null || soundEvent.clip == null)
        {
            Debug.LogWarning("SoundEventPlayer: Intento de reproducir un SoundEvent nulo o sin clip.");
            return null;
        }

        // Crear objeto temporal para reproducir
        GameObject go = new GameObject("Audio_" + soundEvent.clip.name);
        if (parent != null)
            go.transform.SetParent(parent, false);

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = soundEvent.clip;
        source.volume = soundEvent.volume;
        source.spatialBlend = soundEvent.spatialBlend; // en tu caso 0 para DearVR
        source.loop = soundEvent.loop;
        source.outputAudioMixerGroup = soundEvent.mixerGroup;

        source.Play();

        // Si no es loop, destruir el objeto al terminar
        if (!soundEvent.loop)
        {
            Object.Destroy(go, soundEvent.clip.length);
        }

        return source;
    }
}
