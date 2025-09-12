using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class MenuController : MonoBehaviour
{
    [Header("Referencias del Menú")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Image fadeImage; // Imagen negra para fade out del menú
    [SerializeField] private float menuFadeOutDuration = 2f;
    
    [Header("Managers")]
    private IntroManager introManager;
    private MenuInitializer initializer;


    [Header("Configuración")]
    [SerializeField] private bool skipIntro = false; // Para saltar la intro en desarrollo

    private void Awake()
    {
        initializer = FindFirstObjectByType<MenuInitializer>();
        introManager = FindFirstObjectByType<IntroManager>();

        // Asegurar que la imagen de fade esté inicialmente invisible
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.enabled = false;
        }
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
        
        // Detener música del menú
        if (AudioController.Instance != null)
            AudioController.Instance.StopMusic();
        
        // Hacer fade a negro con la imagen del menú
        if (fadeImage != null)
        {
            fadeImage.enabled = true;
            Color startColor = new Color(0, 0, 0, 0);
            Color endColor = new Color(0, 0, 0, 1);
            fadeImage.color = startColor;
            
            float elapsed = 0f;
            while (elapsed < menuFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / menuFadeOutDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            
            fadeImage.color = endColor;
        }
        
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
            StartCoroutine(introManager.StartScreamsAfterDelay()); // Gritos: después de 6s

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
            
            // Configurar jugador
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                var playerController = playerGO.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetControlesActivos(true);
                    playerController.SetCamaraActiva(true);
                }
            }
            
            // Activar canvas del juego
            initializer.ShowCanvasToActivate();
            
            // Configurar cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Desactivar cámara del menú
            var menuCamera = GameObject.Find("MenuCamera");
            if (menuCamera != null)
                menuCamera.SetActive(false);
            
            GameFlowManager.Instance.SetTransitionStatus(false);
            
            // Hacer fade in para mostrar el juego usando el FadeManager
            if (FadeManager.Instance != null)
            {
                yield return StartCoroutine(FadeManager.Instance.FadeInCoroutine(3f));
            }
            
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
