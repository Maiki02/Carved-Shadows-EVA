using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigController : MonoBehaviour
{
    public static ConfigController Instance { get; private set; }

    [SerializeField] private GameObject configPanel; // Panel visual de configuración
    [SerializeField] private GameObject mainMenuPanel; // Panel visual del menú principal

    [Header("Game Settings UI")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertMouseToggle;

    [Header("Audio Settings UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider masterVolumeSlider; // Nueva slider para volumen master

    //[Header("Event System")]
    //[SerializeField] private GameObject eventSystem;

    public void Awake()
    {
        Debug.Log("Awake ConfigController");
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
        Debug.Log("Starting ConfigController...");
        // Obtenemos valores desde GameController
        if (GameController.Instance != null)
        {
            mouseSensitivitySlider.value = GameController.Instance.MouseSensitivity;
            //invertMouseToggle.isOn = GameController.Instance.InvertMouse;

            // Agregamos listeners para detectar cambios en UI
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            //invertMouseToggle.onValueChanged.AddListener(OnInvertMouseChanged);

            this.LoadEventSystem();
        }

        // Obtenemos valores desde AudioController
        if (AudioController.Instance != null)
        {
            Debug.Log("AudioController Instance found, setting up audio sliders." +
            AudioController.Instance.MusicVolume + " " + AudioController.Instance.SfxVolume);
            
            musicVolumeSlider.value = AudioController.Instance.MusicVolume;
            sfxVolumeSlider.value = AudioController.Instance.SfxVolume;
            
            // Configurar slider de volumen master si está disponible
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioController.Instance.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            // Agregamos listeners para detectar cambios en UI
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            Debug.Log("Music Volume: Start  " + AudioController.Instance.MusicVolume);
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.MouseSensitivity = value;
        }
    }

    /*private void OnInvertMouseChanged(bool isInverted)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.InvertMouse = isInverted;
        }
    }*/

    private void OnMusicVolumeChanged(float value)
    {
        Debug.Log("Changed music volume");
        // Aseguramos que AudioController esté inicializado antes de cambiar el volumen
        // Esto es importante para evitar errores si se accede antes de que AudioController se inicialice
        // o si no existe en la escena actual.
        if (AudioController.Instance != null)
        {
            AudioController.Instance.MusicVolume = value;
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        Debug.Log("Changed SFX volume");
        if (AudioController.Instance != null)
        {
            AudioController.Instance.SfxVolume = value;
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        Debug.Log("Changed master volume");
        if (AudioController.Instance != null)
        {
            AudioController.Instance.MasterVolume = value;
        }
    }

    private void LoadEventSystem()
    {
        if (GameController.Instance.IsPaused())
        {
            //eventSystem.SetActive(false); //Creo que el event system, puede estar siempre desactivado
        }
    }

    /// Muestra la UI de configuración
    public void ShowConfiguration()
    {
        if (configPanel != null)
            configPanel.SetActive(true);

        // Si está en pausa, ocultar el menú de pausa
        if (GameController.Instance != null && GameController.Instance.IsPaused())
        {
            if (PauseController.Instance != null)
                PauseController.Instance.SetShowPauseUI(false);
        }
        // Si NO está en pausa, ocultar el menú principal
        else if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    /// Oculta la UI de configuración
    public void HideConfiguration()
    {
        if (configPanel != null)
            configPanel.SetActive(false);

        // Si está en pausa, mostrar el menú de pausa
        if (GameController.Instance != null && GameController.Instance.IsPaused())
        {
            if (PauseController.Instance != null)
                PauseController.Instance.SetShowPauseUI(true);
        }
        // Si NO está en pausa, mostrar el menú principal
        else if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        Debug.Log("Configuración ocultada");
    }

    public void ReturnToPreviousScene()
    {
        this.HideConfiguration();

        // Ocultar la configuración
        /*HideConfiguration();
        
        // Si hay un PauseController activo, volver al menú de pausa
        if (PauseController.Instance != null)
        {
            PauseController.Instance.SetShowPauseUI(true);
        }*/

        // Si no hay PauseController, asumimos que estamos en el menú principal
        // y simplemente ocultamos la configuración (el menú principal sigue visible)
    }
}
