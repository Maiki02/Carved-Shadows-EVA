using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Script de configuración para migrar del sistema AudioController viejo al nuevo sistema con AudioMixer
/// Ejecuta este script para configurar automáticamente tu proyecto
/// </summary>
[System.Serializable]
public class AudioMixerSetup : MonoBehaviour
{
    [Header("STEP 1: Assign your MainMixer here")]
    [SerializeField] private AudioMixer mainMixer;
    
    [Header("STEP 2: Verify Group Names (must match your mixer exactly)")]
    [SerializeField] private string masterGroupName = "Master";
    [SerializeField] private string musicGroupName = "Music";
    [SerializeField] private string sfxGroupName = "SFX";
    
    [Header("STEP 3: Optional - Assign Mixer Groups for Helper")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup radioMixerGroup;
    [SerializeField] private AudioMixerGroup footstepsMixerGroup;
    [SerializeField] private AudioMixerGroup doorMixerGroup;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup organicMixerGroup;
    [SerializeField] private AudioMixerGroup respiracionMixerGroup;

    [Header("Configuration Options")]
    [SerializeField] private bool setupAudioControllerAutomatically = true;
    [SerializeField] private bool setupAudioMixerHelper = true;
    [SerializeField] private bool configureExistingAudioSources = true;

    [Space]
    [Header("Runtime Setup - Press these buttons in Play Mode or use from code")]
    [SerializeField] private bool showDebugLogs = true;

    private void Start()
    {
        if (setupAudioControllerAutomatically)
        {
            SetupAudioController();
        }

        if (setupAudioMixerHelper)
        {
            SetupMixerHelper();
        }

        if (configureExistingAudioSources)
        {
            ConfigureExistingAudioSources();
        }
    }

    /// <summary>
    /// Configura automáticamente el AudioController con el mixer y nombres de grupos
    /// </summary>
    [ContextMenu("Setup AudioController")]
    public void SetupAudioController()
    {
        AudioController audioController = FindObjectOfType<AudioController>();
        if (audioController == null)
        {
            LogMessage("AudioController no encontrado en la escena.", true);
            return;
        }

        // Usar reflexión para asignar el mixer y nombres de grupos
        var audioControllerType = typeof(AudioController);
        
        // Asignar mixer
        var mixerField = audioControllerType.GetField("audioMixer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (mixerField != null && mainMixer != null)
        {
            mixerField.SetValue(audioController, mainMixer);
            LogMessage("AudioMixer asignado al AudioController.");
        }

        // Asignar nombres de grupos
        var musicNameField = audioControllerType.GetField("musicGroupName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (musicNameField != null)
        {
            musicNameField.SetValue(audioController, musicGroupName);
            LogMessage($"Nombre del grupo Music configurado: {musicGroupName}");
        }

        var sfxNameField = audioControllerType.GetField("sfxGroupName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (sfxNameField != null)
        {
            sfxNameField.SetValue(audioController, sfxGroupName);
            LogMessage($"Nombre del grupo SFX configurado: {sfxGroupName}");
        }

        var masterNameField = audioControllerType.GetField("masterGroupName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (masterNameField != null)
        {
            masterNameField.SetValue(audioController, masterGroupName);
            LogMessage($"Nombre del grupo Master configurado: {masterGroupName}");
        }

        LogMessage("AudioController configurado exitosamente!");
    }

    /// <summary>
    /// Configura el AudioMixerHelper con las referencias de grupos
    /// </summary>
    [ContextMenu("Setup AudioMixer Helper")]
    public void SetupMixerHelper()
    {
        AudioMixerHelper mixerHelper = FindObjectOfType<AudioMixerHelper>();
        if (mixerHelper == null)
        {
            // Crear el helper si no existe
            GameObject helperGO = new GameObject("AudioMixerHelper");
            mixerHelper = helperGO.AddComponent<AudioMixerHelper>();
            LogMessage("AudioMixerHelper creado.");
        }

        // Asignar referencias de grupos usando reflexión
        var helperType = typeof(AudioMixerHelper);
        
        AssignMixerGroupToHelper(helperType, mixerHelper, "musicMixerGroup", musicMixerGroup, "Music");
        AssignMixerGroupToHelper(helperType, mixerHelper, "sfxMixerGroup", sfxMixerGroup, "SFX");
        AssignMixerGroupToHelper(helperType, mixerHelper, "radioMixerGroup", radioMixerGroup, "Radio");
        AssignMixerGroupToHelper(helperType, mixerHelper, "footstepsMixerGroup", footstepsMixerGroup, "FootSteps");
        AssignMixerGroupToHelper(helperType, mixerHelper, "doorMixerGroup", doorMixerGroup, "Door");
        AssignMixerGroupToHelper(helperType, mixerHelper, "ambientMixerGroup", ambientMixerGroup, "Ambient");
        AssignMixerGroupToHelper(helperType, mixerHelper, "organicMixerGroup", organicMixerGroup, "Organic");
        AssignMixerGroupToHelper(helperType, mixerHelper, "respiracionMixerGroup", respiracionMixerGroup, "Respiracion");

        LogMessage("AudioMixerHelper configurado exitosamente!");
    }

    /// <summary>
    /// Asigna un AudioMixerGroup al Helper usando reflexión
    /// </summary>
    private void AssignMixerGroupToHelper(System.Type helperType, AudioMixerHelper helper, string fieldName, AudioMixerGroup group, string groupDisplayName)
    {
        var field = helperType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && group != null)
        {
            field.SetValue(helper, group);
            LogMessage($"Grupo {groupDisplayName} asignado al helper.");
        }
    }

    /// <summary>
    /// Configura AudioSources existentes en la escena
    /// </summary>
    [ContextMenu("Configure Existing AudioSources")]
    public void ConfigureExistingAudioSources()
    {
        AudioMixerHelper helper = FindObjectOfType<AudioMixerHelper>();
        if (helper != null)
        {
            helper.ConfigureAllAudioSources();
            LogMessage("AudioSources configurados usando AudioMixerHelper.");
        }
        else
        {
            LogMessage("AudioMixerHelper no encontrado. Configúralo primero.", true);
        }
    }

    /// <summary>
    /// Prueba el sistema de volumen
    /// </summary>
    [ContextMenu("Test Volume System")]
    public void TestVolumeSystem()
    {
        if (AudioController.Instance == null)
        {
            LogMessage("AudioController no encontrado para pruebas.", true);
            return;
        }

        LogMessage("Probando sistema de volumen...");
        
        // Probar volúmenes
        AudioController.Instance.MasterVolume = 0.8f;
        AudioController.Instance.MusicVolume = 0.6f;
        AudioController.Instance.SfxVolume = 0.7f;

        LogMessage($"Volúmenes configurados: Master={AudioController.Instance.MasterVolume:F2}, Music={AudioController.Instance.MusicVolume:F2}, SFX={AudioController.Instance.SfxVolume:F2}");
    }

    /// <summary>
    /// Muestra información de configuración actual
    /// </summary>
    [ContextMenu("Show Current Configuration")]
    public void ShowCurrentConfiguration()
    {
        LogMessage("=== CONFIGURACIÓN ACTUAL ===");
        LogMessage($"AudioMixer: {(mainMixer != null ? mainMixer.name : "NO ASIGNADO")}");
        LogMessage($"Nombres de grupos: Master='{masterGroupName}', Music='{musicGroupName}', SFX='{sfxGroupName}'");
        
        AudioController controller = FindObjectOfType<AudioController>();
        LogMessage($"AudioController encontrado: {(controller != null ? "SÍ" : "NO")}");
        
        AudioMixerHelper helper = FindObjectOfType<AudioMixerHelper>();
        LogMessage($"AudioMixerHelper encontrado: {(helper != null ? "SÍ" : "NO")}");
        
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        LogMessage($"AudioSources en escena: {audioSources.Length}");
        
        LogMessage("=== FIN CONFIGURACIÓN ===");
    }

    private void LogMessage(string message, bool isWarning = false)
    {
        if (!showDebugLogs) return;

        string prefix = "[AudioMixerSetup] ";
        if (isWarning)
        {
            Debug.LogWarning(prefix + message);
        }
        else
        {
            Debug.Log(prefix + message);
        }
    }

#if UNITY_EDITOR
    [Header("Editor Tools")]
    [SerializeField] private bool showEditorButtons = true;

    private void OnValidate()
    {
        if (showEditorButtons && Application.isPlaying)
        {
            // Validaciones en tiempo de ejecución
        }
    }
#endif
}
