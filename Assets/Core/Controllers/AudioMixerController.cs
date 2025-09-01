using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Controlador de volumen basado en Audio Mixers para Unity 6
/// Reemplaza el sistema anterior basado en AudioSource individual
/// </summary>
public class AudioMixerController : MonoBehaviour
{
    public static AudioMixerController Instance { get; private set; }

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer mainMixer;
    
    [Header("Parámetros de Volumen")]
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    [SerializeField] private string objectsVolumeParam = "ObjectsVolume";
    [SerializeField] private string environmentVolumeParam = "EnvironmentVolume";
    [SerializeField] private string voiceVolumeParam = "VoiceVolume";

    [Header("Volúmenes (0-1)")]
    [Range(0f, 1f)] private float masterVolume = 1f;
    [Range(0f, 1f)] private float musicVolume = 1f;
    [Range(0f, 1f)] private float sfxVolume = 1f;
    [Range(0f, 1f)] private float objectsVolume = 1f;
    [Range(0f, 1f)] private float environmentVolume = 1f;
    [Range(0f, 1f)] private float voiceVolume = 1f;

    // Propiedades públicas
    public float MasterVolume
    {
        get => masterVolume;
        set { masterVolume = value; SetMixerVolume(masterVolumeParam, value); }
    }

    public float MusicVolume
    {
        get => musicVolume;
        set { musicVolume = value; SetMixerVolume(musicVolumeParam, value); }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = value; SetMixerVolume(sfxVolumeParam, value); }
    }

    public float ObjectsVolume
    {
        get => objectsVolume;
        set { objectsVolume = value; SetMixerVolume(objectsVolumeParam, value); }
    }

    public float EnvironmentVolume
    {
        get => environmentVolume;
        set { environmentVolume = value; SetMixerVolume(environmentVolumeParam, value); }
    }

    public float VoiceVolume
    {
        get => voiceVolume;
        set { voiceVolume = value; SetMixerVolume(voiceVolumeParam, value); }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMixerVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMixerVolumes()
    {
        // Cargar volúmenes guardados o usar valores por defecto
        LoadVolumeSettings();
        
        // Aplicar volúmenes a los mixers
        SetMixerVolume(masterVolumeParam, masterVolume);
        SetMixerVolume(musicVolumeParam, musicVolume);
        SetMixerVolume(sfxVolumeParam, sfxVolume);
        SetMixerVolume(objectsVolumeParam, objectsVolume);
        SetMixerVolume(environmentVolumeParam, environmentVolume);
        SetMixerVolume(voiceVolumeParam, voiceVolume);
    }

    /// <summary>
    /// Convierte volumen lineal (0-1) a decibelios y lo aplica al mixer
    /// </summary>
    private void SetMixerVolume(string parameterName, float volume)
    {
        if (mainMixer == null) return;

        // Convertir de lineal (0-1) a decibelios (-80 a 0)
        float dbValue = volume > 0f ? 20f * Mathf.Log10(volume) : -80f;
        mainMixer.SetFloat(parameterName, dbValue);
    }

    /// <summary>
    /// Obtiene el volumen actual del mixer en formato lineal (0-1)
    /// </summary>
    public float GetMixerVolume(string parameterName)
    {
        if (mainMixer == null) return 0f;

        mainMixer.GetFloat(parameterName, out float dbValue);
        return dbValue > -80f ? Mathf.Pow(10f, dbValue / 20f) : 0f;
    }

    // Métodos para guardar/cargar configuración
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        objectsVolume = PlayerPrefs.GetFloat("ObjectsVolume", 1f);
        environmentVolume = PlayerPrefs.GetFloat("EnvironmentVolume", 1f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("ObjectsVolume", objectsVolume);
        PlayerPrefs.SetFloat("EnvironmentVolume", environmentVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
        PlayerPrefs.Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveVolumeSettings();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveVolumeSettings();
    }

    private void OnDestroy()
    {
        SaveVolumeSettings();
    }
}
