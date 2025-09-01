using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Group Names - IMPORTANT: Match exactly with your mixer groups")]
    [SerializeField] private string musicGroupName = "Music";
    [SerializeField] private string sfxGroupName = "SFX";
    [SerializeField] private string masterGroupName = "Master";

    [Header("Clips de Sonido (orden según SoundType)")]
    [SerializeField] private AudioClip[] soundClips;

    [Header("Volúmenes")]
    [Range(0f, 1f)]
    private float musicVolume = 1f;
    [Range(0f, 1f)]
    private float sfxVolume = 1f;
    [Range(0f, 1f)]
    private float masterVolume = 1f;

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            SetMixerVolume(musicGroupName, musicVolume);
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            SetMixerVolume(sfxGroupName, sfxVolume);
        }
    }

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            SetMixerVolume(masterGroupName, masterVolume);
        }
    }



    /// <summary>
    /// Convierte un valor de volumen de 0-1 a decibeles y lo aplica al mixer
    /// </summary>
    /// <param name="groupName">Nombre del grupo en el mixer</param>
    /// <param name="volume">Volumen entre 0 y 1</param>
    private void SetMixerVolume(string groupName, float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning($"[AudioController] AudioMixer no asignado. No se puede establecer volumen para {groupName}");
            return;
        }

        // Convertir 0-1 a decibeles (-80 a 0 dB)
        // Si el volumen es 0, usar -80dB (prácticamente silencio)
        // Si el volumen es 1, usar 0dB (volumen máximo)
        float dbValue = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;
        
        audioMixer.SetFloat(groupName, dbValue);
        
        Debug.Log($"[AudioController] Volumen {groupName} establecido a {volume:F2} ({dbValue:F2} dB)");
    }

    /// <summary>
    /// Obtiene el volumen actual de un grupo del mixer
    /// </summary>
    /// <param name="groupName">Nombre del grupo en el mixer</param>
    /// <returns>Volumen entre 0 y 1</returns>
    private float GetMixerVolume(string groupName)
    {
        if (audioMixer == null) return 1f;

        if (audioMixer.GetFloat(groupName, out float dbValue))
        {
            // Convertir decibeles a 0-1
            return dbValue <= -80f ? 0f : Mathf.Pow(10f, dbValue / 20f);
        }
        
        return 1f;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Inicializar volúmenes desde el mixer si está disponible
        if (audioMixer != null)
        {
            musicVolume = GetMixerVolume(musicGroupName);
            sfxVolume = GetMixerVolume(sfxGroupName);
            masterVolume = GetMixerVolume(masterGroupName);
            
            Debug.Log($"[AudioController] Volúmenes inicializados - Music: {musicVolume:F2}, SFX: {sfxVolume:F2}, Master: {masterVolume:F2}");
        }
        else
        {
            Debug.LogWarning("[AudioController] AudioMixer no asignado. Usando valores por defecto.");
        }
    }

    /// <summary>
    /// Reproduce un AudioSource con un clip específico y configuraciones dadas
    /// </summary>
    /// <param name="audioSource">El AudioSource a usar</param>
    /// <param name="clip">El clip a reproducir</param>
    /// <param name="loop">Si debe reproducirse en loop</param>
    /// <param name="mixerGroup">El grupo de mixer a usar</param>
    private void PlayAudioSource(AudioSource audioSource, AudioClip clip, bool loop, AudioMixerGroup mixerGroup)
    {
        if (audioSource == null || clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = loop;
        
        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
        }
        
        audioSource.Play();
    }

    /// <summary>
    /// Reproduce un sonido SFX usando el sistema AudioType (obsoleto, mantener compatibilidad)
    /// NOTA: Este método necesita un AudioSource en la escena para funcionar
    /// </summary>
    public void PlaySFX(AudioType type, bool loop = false)
    {
        AudioClip clip = soundClips[(int)type];
        if (clip == null) return;

        // Buscar un AudioSource disponible en el GameObject o crear uno temporal
        AudioSource tempAudioSource = GetOrCreateTempAudioSource();
        
        if (tempAudioSource == null)
        {
            Debug.LogWarning("[AudioController] No se encontró AudioSource para reproducir SFX. Usa PlaySFXClip con AudioSource específico.");
            return;
        }

        AudioMixerGroup sfxGroup = GetMixerGroup(sfxGroupName);
        PlayAudioSource(tempAudioSource, clip, loop, sfxGroup);
    }

    /// <summary>
    /// Reproduce música usando el sistema AudioType (obsoleto, mantener compatibilidad)  
    /// NOTA: Este método necesita un AudioSource en la escena para funcionar
    /// </summary>
    public void PlayMusic(AudioType type, bool loop = true)
    {
        AudioClip clip = soundClips[(int)type];
        if (clip != null)
        {
            PlayMusic(clip, loop);
        }
    }

    /// <summary>
    /// Reproduce música con clip específico (obsoleto, mantener compatibilidad)
    /// NOTA: Este método necesita un AudioSource en la escena para funcionar
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        
        AudioSource tempAudioSource = GetOrCreateTempAudioSource();
        if (tempAudioSource == null)
        {
            Debug.LogWarning("[AudioController] No se encontró AudioSource para reproducir música. Usa los métodos específicos con AudioSource.");
            return;
        }

        AudioMixerGroup musicGroup = GetMixerGroup(musicGroupName);
        PlayAudioSource(tempAudioSource, clip, loop, musicGroup);
    }

    /// <summary>
    /// Para la música (obsoleto, mantener compatibilidad)
    /// </summary>
    public void StopMusic()
    {
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var audioSource in audioSources)
        {
            if (audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Reproduce un clip SFX específico (obsoleto, mantener compatibilidad)
    /// </summary>
    public void PlaySFXClip(AudioClip clip, bool loop = false)
    {
        if (clip == null) return;

        AudioSource tempAudioSource = GetOrCreateTempAudioSource();
        if (tempAudioSource == null)
        {
            Debug.LogWarning("[AudioController] No se encontró AudioSource para reproducir SFX clip.");
            return;
        }

        AudioMixerGroup sfxGroup = GetMixerGroup(sfxGroupName);
        PlayAudioSource(tempAudioSource, clip, loop, sfxGroup);
    }

    /// <summary>
    /// Reproduce un clip de música específico (obsoleto, mantener compatibilidad)
    /// </summary>
    public void PlayMusicClip(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        AudioSource tempAudioSource = GetOrCreateTempAudioSource();
        if (tempAudioSource == null)
        {
            Debug.LogWarning("[AudioController] No se encontró AudioSource para reproducir música clip.");
            return;
        }

        AudioMixerGroup musicGroup = GetMixerGroup(musicGroupName);
        PlayAudioSource(tempAudioSource, clip, loop, musicGroup);
    }

    /// <summary>
    /// Obtiene o crea un AudioSource temporal para compatibilidad con métodos antiguos
    /// </summary>
    private AudioSource GetOrCreateTempAudioSource()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[AudioController] AudioSource temporal creado para compatibilidad.");
        }
        return audioSource;
    }

    /// <summary>
    /// Obtiene un AudioMixerGroup por nombre
    /// </summary>
    private AudioMixerGroup GetMixerGroup(string groupName)
    {
        if (audioMixer == null) return null;

        // Nota: Unity no proporciona una manera directa de obtener un AudioMixerGroup por nombre
        // desde el código. Necesitarás asignar las referencias manualmente en el inspector
        // o usar un sistema más complejo de referencias.
        return null; // Por ahora retornamos null, el AudioSource usará su grupo asignado por defecto
    }

    //Detectamos el cambio de escena para poner la música adecuada
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Escena cargada: " + scene.name);
        if (scene.name == "Game")
            PlayMusic(AudioType.BackgroundMusicMenu);
    }

}
