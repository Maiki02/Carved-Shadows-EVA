using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.Cinemachine;
using BlendStyle = Unity.Cinemachine.CinemachineBlendDefinition.Styles;

public class MenuController : MonoBehaviour
{
    [Header("Referencias del Menú")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Image fadeImage; // Imagen negra para fade out del menú
    [SerializeField] private float menuFadeOutDuration = 2f;
    
    [Header("Audio del Menú")]
    [SerializeField] private AudioClip menuMusicClip; // Clip de música para el menú
    [SerializeField] private float menuMusicVolume = 0.5f; // Volumen de la música del menú
    
    [Header("Cámaras Cinemachine")]
    [SerializeField] private CinemachineCamera menuVirtualCamera;
    [SerializeField] private CinemachineCamera playerVirtualCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private int menuCameraPriority = 20;
    [SerializeField] private int playerCameraPriority = 10;
    
    [Header("Managers")]
    private IntroManager introManager;
    private MenuInitializer initializer;
    private AudioSource menuAudioSource; // AudioSource para la música del menú


    [Header("Configuración")]
    [SerializeField] private bool skipIntro = false; // Para saltar la intro en desarrollo

    private void Awake()
    {
        initializer = FindFirstObjectByType<MenuInitializer>();
        introManager = FindFirstObjectByType<IntroManager>();

        // Inicializar AudioSource para la música del menú
        InitializeMenuAudioSource();

        // Inicializar referencias de Cinemachine si no están asignadas
        InitializeCinemachineReferences();

        // Asegurar que la imagen de fade esté inicialmente invisible
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.enabled = false;
        }
    }

    private void InitializeCinemachineReferences()
    {
        // Buscar CinemachineBrain en la cámara principal
        if (cinemachineBrain == null)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
            }
        }

        // Buscar cámara virtual del menú (debe tener un tag específico o nombre)
        if (menuVirtualCamera == null)
        {
            var menuCameraGO = GameObject.Find("MenuCamera");
            if (menuCameraGO != null)
            {
                menuVirtualCamera = menuCameraGO.GetComponent<CinemachineCamera>();
            }
        }

        // Buscar cámara virtual del player
        if (playerVirtualCamera == null)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                var playerController = playerGO.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // Acceder a la mainCam del PlayerController a través de reflexión o hacer la propiedad pública
                    var mainCamField = typeof(PlayerController).GetField("mainCam", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (mainCamField != null)
                    {
                        playerVirtualCamera = mainCamField.GetValue(playerController) as CinemachineCamera;
                    }
                }
            }
        }

        if (cinemachineBrain == null)
            Debug.LogWarning("[MenuController] CinemachineBrain no encontrado");
        if (menuVirtualCamera == null)
            Debug.LogWarning("[MenuController] Cámara virtual del menú no encontrada");
        if (playerVirtualCamera == null)
            Debug.LogWarning("[MenuController] Cámara virtual del player no encontrada");
    }

    private void InitializeMenuAudioSource()
    {
        // Buscar o crear AudioSource para la música del menú
        menuAudioSource = GetComponent<AudioSource>();
        if (menuAudioSource == null)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configurar el AudioSource para música de fondo
        menuAudioSource.clip = menuMusicClip;
        menuAudioSource.volume = menuMusicVolume;
        menuAudioSource.loop = true;
        menuAudioSource.playOnAwake = false;

        // Reproducir la música del menú si hay un clip asignado
        if (menuMusicClip != null)
        {
            menuAudioSource.Play();
            Debug.Log("[MenuController] Música del menú iniciada");
        }
        else
        {
            Debug.LogWarning("[MenuController] No se ha asignado un clip de música para el menú");
        }
    }

    /// <summary>
    /// Fuerza un corte instantáneo en Cinemachine sin transición
    /// </summary>
    private void ForceInstantCameraCut()
    {
        if (cinemachineBrain == null) return;

        var originalBlend = cinemachineBrain.DefaultBlend;
        var cutBlend = new CinemachineBlendDefinition(BlendStyle.Cut, 0f);
        cinemachineBrain.DefaultBlend = cutBlend;

        StartCoroutine(RestoreBlendAfterFrame(originalBlend));
    }

    private IEnumerator RestoreBlendAfterFrame(CinemachineBlendDefinition originalBlend)
    {
        yield return null; // Esperar 1 frame
        if (cinemachineBrain != null)
            cinemachineBrain.DefaultBlend = originalBlend;
    }

    public void OnStartButton()
    {
        StartCoroutine(CompleteGameStartSequence());
    }

    private IEnumerator CompleteGameStartSequence()
    {

        // 1. FADE OUT DEL MENÚ (pantalla negra)
        yield return StartCoroutine(FadeOutMenu());

        if (!skipIntro)
        {
            // 2. EJECUTAR INTRO
            yield return StartCoroutine(ExecuteIntro());
        }
        
        // 3. INICIAR JUEGO
        yield return StartCoroutine(StartGame());
    }

    private IEnumerator FadeOutMenu()
    {
        
        // Hacer fade a negro con la imagen del menú Y fade out de la música simultáneamente
        if (fadeImage != null)
        {
            fadeImage.enabled = true;
            Color startColor = new Color(0, 0, 0, 0);
            Color endColor = new Color(0, 0, 0, 1);
            fadeImage.color = startColor;
            
            // Valores iniciales para el fade de música
            float initialMusicVolume = menuAudioSource != null ? menuAudioSource.volume : 0f;
            
            float elapsed = 0f;
            while (elapsed < menuFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / menuFadeOutDuration;
                
                // Fade visual
                float alpha = Mathf.Lerp(0f, 1f, progress);
                fadeImage.color = new Color(0, 0, 0, alpha);
                
                // Fade de audio
                if (menuAudioSource != null && menuAudioSource.isPlaying)
                {
                    menuAudioSource.volume = Mathf.Lerp(initialMusicVolume, 0f, progress);
                }
                
                yield return null;
            }
            
            fadeImage.color = endColor;
        }
        
        // Detener completamente la música del menú
        if (menuAudioSource != null && menuAudioSource.isPlaying)
        {
            menuAudioSource.Stop();
            Debug.Log("[MenuController] Música del menú detenida");
        }
        
        // Detener música del AudioController (por compatibilidad)
        if (AudioController.Instance != null)
            AudioController.Instance.StopMusic();
        
        // Desactivar el menú
        if (menuRoot != null)
            menuRoot.SetActive(false);
        
        // IMPORTANTE: Desactivar la fadeImage del menú y activar la blackImage del FadeManager
        if (fadeImage != null)
        {
            fadeImage.enabled = false;
        }
        
    }

    private IEnumerator ExecuteIntro()
    {
        if (introManager != null)
        {
            // Activar IntroManager si está inactivo
            if (!introManager.gameObject.activeInHierarchy)
                introManager.gameObject.SetActive(true);

            Debug.Log("[MenuController] Iniciando todas las secuencias de intro EN PARALELO");

            // EJECUTAR TODO EN PARALELO - Sin yield return para que no esperen
            StartCoroutine(introManager.ShowIntroText());        // Texto: inmediato
            StartCoroutine(introManager.StartRainAfterDelay());  // Lluvia: después de 2s
            //StartCoroutine(introManager.StartScreamsAfterDelay()); // Gritos: después de 6s

            // Esperar la duración total de la intro
            float totalDuration = introManager.GetTotalIntroDuration();
            Debug.Log($"[MenuController] Esperando {totalDuration}s para que termine toda la intro");
            yield return new WaitForSeconds(totalDuration - 2f); // -2f para el fade final

            // Fade final con intensificación de gritos
            yield return StartCoroutine(introManager.FinalFadeWithScreamIntensity());
            
            // Limpiar al final
            introManager.StopSounds();
        }
    }

    private IEnumerator StartGame()
    {
        if (initializer != null)
        {
            // Configurar estados del juego
            GameController.Instance.SetGameStarted(true);
            GameFlowManager.Instance.SetTransitionStatus(true);

            // NUEVA FUNCIONALIDAD: Cambio instantáneo de cámaras usando Cinemachine
            SwitchToPlayerCameraInstantly();

            Debug.Log("[MenuController] Cambiando a cámara del player sin transición");
            
            // Configurar jugador
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                var playerController = playerGO.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetCamaraActiva(true);
                }
            }

            Debug.Log("[MenuController] Activando canvas del juego y comenzando secuencia de despertar");
            // Activar canvas del juego
            initializer.ShowCanvasToActivate();

            GameFlowManager.Instance.SetTransitionStatus(false);

            // Hacer fade in para mostrar el juego usando el FadeManager
            /*      if (FadeManager.Instance != null)
            {
                yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(3f));
            }*/

            // NUEVA FUNCIONALIDAD: Iniciar secuencia de despertar del protagonista
            if (playerGO != null)
            {
                var wakeUpComponent = playerGO.GetComponent<PlayerWakeUpSequence>();
                if (wakeUpComponent != null)
                {
                    Debug.Log("[MenuController] Iniciando secuencia de despertar del protagonista");
                    wakeUpComponent.StartWakeUpSequence();
                }
                else
                {
                    Debug.LogWarning("[MenuController] PlayerWakeUpSequence no encontrado en el Player");
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Cambia instantáneamente de la cámara del menú a la del player usando prioridades de Cinemachine
    /// </summary>
    private void SwitchToPlayerCameraInstantly()
    {
        // Forzar corte instantáneo
        ForceInstantCameraCut();

        // Cambiar prioridades de las cámaras
        if (menuVirtualCamera != null)
        {
            menuVirtualCamera.Priority = 0; // Baja prioridad
            Debug.Log("[MenuController] Prioridad de cámara del menú cambiada a 0");
        }

        if (playerVirtualCamera != null)
        {
            playerVirtualCamera.Priority = playerCameraPriority; // Alta prioridad
            Debug.Log($"[MenuController] Prioridad de cámara del player cambiada a {playerCameraPriority}");
        }

        // Desactivar el GameObject de la cámara del menú si existe (por compatibilidad)
        var menuCamera = GameObject.Find("MenuCamera");
        if (menuCamera != null)
        {
            menuCamera.SetActive(false);
            Debug.Log("[MenuController] GameObject MenuCamera desactivado");
        }
    }

    public void OnConfigurationButton()
    {
        // Mostrar la configuración usando el Singleton
        if (ConfigController.Instance != null)
        {
            ConfigController.Instance.ShowConfiguration();
        }
        else
        {
            Debug.LogWarning("ConfigController.Instance no encontrado en la escena del menú");
        }
    }
    

    public void OnExitButton()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
