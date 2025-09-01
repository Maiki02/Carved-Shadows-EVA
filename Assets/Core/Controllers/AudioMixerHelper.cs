using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Helper para configurar automáticamente AudioSources con los grupos correctos del AudioMixer
/// Usa este script para asegurar que todos los AudioSources usen los grupos de mixer apropiados
/// </summary>
public class AudioMixerHelper : MonoBehaviour
{
    [Header("Mixer Groups References")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup radioMixerGroup;
    [SerializeField] private AudioMixerGroup footstepsMixerGroup;
    [SerializeField] private AudioMixerGroup doorMixerGroup;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup organicMixerGroup;
    [SerializeField] private AudioMixerGroup respiracionMixerGroup;

    [Header("Auto-Configuration")]
    [SerializeField] private bool autoConfigureOnStart = true;
    [SerializeField] private bool logConfigurations = true;

    public static AudioMixerHelper Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureAllAudioSources();
        }
    }

    /// <summary>
    /// Configura automáticamente todos los AudioSources en la escena
    /// </summary>
    public void ConfigureAllAudioSources()
    {
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            ConfigureAudioSourceByTag(audioSource);
        }

        if (logConfigurations)
        {
            Debug.Log($"[AudioMixerHelper] Configurados {allAudioSources.Length} AudioSources con sus grupos de mixer apropiados.");
        }
    }

    /// <summary>
    /// Configura un AudioSource específico basado en el tag o nombre de su GameObject
    /// </summary>
    private void ConfigureAudioSourceByTag(AudioSource audioSource)
    {
        if (audioSource == null) return;

        GameObject go = audioSource.gameObject;
        string objectName = go.name.ToLower();
        string tag = go.tag.ToLower();

        // Configurar basado en tag del GameObject
        switch (tag)
        {
            case "music":
            case "backgroundmusic":
                SetAudioSourceMixerGroup(audioSource, musicMixerGroup, "Music");
                break;
            case "sfx":
            case "soundeffect":
                SetAudioSourceMixerGroup(audioSource, sfxMixerGroup, "SFX");
                break;
            case "radio":
                SetAudioSourceMixerGroup(audioSource, radioMixerGroup, "Radio");
                break;
            case "footsteps":
            case "steps":
                SetAudioSourceMixerGroup(audioSource, footstepsMixerGroup, "FootSteps");
                break;
            case "door":
                SetAudioSourceMixerGroup(audioSource, doorMixerGroup, "Door");
                break;
            case "ambient":
            case "environment":
                SetAudioSourceMixerGroup(audioSource, ambientMixerGroup, "Ambient sounds");
                break;
            default:
                // Configurar basado en el nombre del GameObject
                ConfigureAudioSourceByName(audioSource, objectName);
                break;
        }
    }

    /// <summary>
    /// Configura un AudioSource basado en el nombre de su GameObject
    /// </summary>
    private void ConfigureAudioSourceByName(AudioSource audioSource, string objectName)
    {
        if (objectName.Contains("radio"))
        {
            SetAudioSourceMixerGroup(audioSource, radioMixerGroup, "Radio");
        }
        else if (objectName.Contains("door") || objectName.Contains("puerta"))
        {
            SetAudioSourceMixerGroup(audioSource, doorMixerGroup, "Door");
        }
        else if (objectName.Contains("footstep") || objectName.Contains("paso") || objectName.Contains("step"))
        {
            SetAudioSourceMixerGroup(audioSource, footstepsMixerGroup, "FootSteps");
        }
        else if (objectName.Contains("music") || objectName.Contains("musica") || objectName.Contains("background"))
        {
            SetAudioSourceMixerGroup(audioSource, musicMixerGroup, "Music");
        }
        else if (objectName.Contains("ambient") || objectName.Contains("environment") || objectName.Contains("viento"))
        {
            SetAudioSourceMixerGroup(audioSource, ambientMixerGroup, "Ambient sounds");
        }
        else if (objectName.Contains("organic") || objectName.Contains("beat") || objectName.Contains("breathing"))
        {
            SetAudioSourceMixerGroup(audioSource, organicMixerGroup, "Organic");
        }
        else if (objectName.Contains("respiracion") || objectName.Contains("respiration"))
        {
            SetAudioSourceMixerGroup(audioSource, respiracionMixerGroup, "Respiracion");
        }
        else
        {
            // Por defecto, asignar al grupo SFX
            SetAudioSourceMixerGroup(audioSource, sfxMixerGroup, "SFX (Default)");
        }
    }

    /// <summary>
    /// Asigna un AudioMixerGroup a un AudioSource específico
    /// </summary>
    private void SetAudioSourceMixerGroup(AudioSource audioSource, AudioMixerGroup mixerGroup, string groupName)
    {
        if (audioSource == null) return;

        if (mixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
            
            if (logConfigurations)
            {
                Debug.Log($"[AudioMixerHelper] '{audioSource.gameObject.name}' configurado para grupo '{groupName}'");
            }
        }
        else
        {
            Debug.LogWarning($"[AudioMixerHelper] Grupo '{groupName}' no está asignado para '{audioSource.gameObject.name}'");
        }
    }

    /// <summary>
    /// Métodos públicos para configurar AudioSources específicos
    /// </summary>
    public void SetMusicGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, musicMixerGroup, "Music");
    public void SetSFXGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, sfxMixerGroup, "SFX");
    public void SetRadioGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, radioMixerGroup, "Radio");
    public void SetFootstepsGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, footstepsMixerGroup, "FootSteps");
    public void SetDoorGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, doorMixerGroup, "Door");
    public void SetAmbientGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, ambientMixerGroup, "Ambient sounds");
    public void SetOrganicGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, organicMixerGroup, "Organic");
    public void SetRespiracionGroup(AudioSource audioSource) => SetAudioSourceMixerGroup(audioSource, respiracionMixerGroup, "Respiracion");

    /// <summary>
    /// Obtener referencias a los grupos de mixer (útil para otros scripts)
    /// </summary>
    public AudioMixerGroup GetMusicGroup() => musicMixerGroup;
    public AudioMixerGroup GetSFXGroup() => sfxMixerGroup;
    public AudioMixerGroup GetRadioGroup() => radioMixerGroup;
    public AudioMixerGroup GetFootstepsGroup() => footstepsMixerGroup;
    public AudioMixerGroup GetDoorGroup() => doorMixerGroup;
    public AudioMixerGroup GetAmbientGroup() => ambientMixerGroup;
    public AudioMixerGroup GetOrganicGroup() => organicMixerGroup;
    public AudioMixerGroup GetRespiracionGroup() => respiracionMixerGroup;
}
