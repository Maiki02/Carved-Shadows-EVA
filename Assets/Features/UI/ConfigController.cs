using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfigController : MonoBehaviour
{
    public static ConfigController Instance { get; private set; }

    [SerializeField] private GameObject configPanel;   // Panel de configuración
    [SerializeField] private GameObject mainMenuPanel; // Panel del menú principal

    [Header("Game Settings UI")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertMouseToggle;

    [Header("Audio Settings UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider masterVolumeSlider;

    [Header("UI Focus")]
    [SerializeField] private GameObject firstConfigSelected; // primer slider/botón al abrir config

    [Header("Input")]
    [SerializeField] private UIInputBinder uiBinder; // opcional, se autocompleta

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

        // Game settings
        if (GameController.Instance != null)
        {
            mouseSensitivitySlider.value = GameController.Instance.MouseSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            LoadEventSystem();
        }

        // Audio settings
        if (AudioController.Instance != null)
        {
            Debug.Log("AudioController Instance found, setting up audio sliders." +
                AudioController.Instance.MusicVolume + " " + AudioController.Instance.SfxVolume);

            musicVolumeSlider.value = AudioController.Instance.MusicVolume;
            sfxVolumeSlider.value   = AudioController.Instance.SfxVolume;

            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = AudioController.Instance.MasterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            Debug.Log("Music Volume: Start  " + AudioController.Instance.MusicVolume);
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        if (GameController.Instance != null)
            GameController.Instance.MouseSensitivity = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioController.Instance != null)
            AudioController.Instance.MusicVolume = value;
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (AudioController.Instance != null)
            AudioController.Instance.SfxVolume = value;
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioController.Instance != null)
            AudioController.Instance.MasterVolume = value;
    }

    private void LoadEventSystem()
    {
        // Nada por ahora; dejamos hook por si querés manejar algo especial
    }

    /// Mostrar configuración
    public void ShowConfiguration()
    {
        if (configPanel) configPanel.SetActive(true);

        Debug.Log("Configuración mostrada");


        if (SceneController.Instance.IsInScene("MenuScene"))
        {
            Debug.Log("Ocultamos menú principal");
            //Ocultamos el Canvas, no el objeto padre porque tiene la música sonando
            mainMenuPanel.SetActive(false);
        } // Si está en pausa, ocultar el pause menu
        else if (GameController.Instance != null && GameController.Instance.IsPaused())
        {
            Debug.Log("Ocultamos menú de pausa");
            PauseController.Instance?.SetShowPauseUI(false);
        }

        // Navegación UI + foco inicial
        GetBinder()?.SwitchToUI();
        if (firstConfigSelected)
            StartCoroutine(SelectNextFrame(firstConfigSelected));

        // En menús mostramos cursor (si además hay soporte mouse)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    /// Ocultar configuración
    public void HideConfiguration()
    {
        if (configPanel) configPanel.SetActive(false);

        // Si está en pausa, volver a mostrar el pause menu (que ya maneja selección)
        if (GameController.Instance != null && GameController.Instance.IsPaused())
        {
            PauseController.Instance?.SetShowPauseUI(true);
            GetBinder()?.SwitchToUI(); // seguimos en UI mientras hay pausa
        }
        else if (mainMenuPanel)
        {
            // Si veníamos del menú principal, lo volvemos a mostrar (seguimos en UI)
            mainMenuPanel.SetActive(true);
            GetBinder()?.SwitchToUI();
        }

        Debug.Log("Configuración ocultada");
    }

    public void ReturnToPreviousScene()
    {
        HideConfiguration();
    }

    // -------- helpers --------

    private IEnumerator SelectNextFrame(GameObject go)
    {
        yield return null;
        if (!go || !EventSystem.current) yield break;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }

    private UIInputBinder GetBinder()
    {
        if (uiBinder) return uiBinder;
#if UNITY_2023_1_OR_NEWER
        uiBinder = FindAnyObjectByType<UIInputBinder>();
#else
        uiBinder = FindObjectOfType<UIInputBinder>();
#endif
        return uiBinder;
    }
}
