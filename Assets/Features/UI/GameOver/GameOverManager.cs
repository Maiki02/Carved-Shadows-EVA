using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private TextMeshProUGUI gameTitle;
    [SerializeField] private Button returnToMenuButton;
    
    [Header("Referencias de Audio")]
    [SerializeField] private AudioClip gameOverAudioClip;
    
    [Header("Información del Estudio")]
    [SerializeField] private GameObject studioInfoObject;
    
    [Header("Créditos")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private CreditEntry[] credits;
    
    [System.Serializable]
    public struct CreditEntry
    {
        public string name;
        public string role;
    }
    
    [Header("Configuración de Tiempo - Título")]
    [SerializeField] private float titleFadeInDuration = 2f;
    [SerializeField] private float titleDisplayTime = 3f;
    [SerializeField] private float titleFadeOutDuration = 2f;
    
    [Header("Configuración de Tiempo - Información del Estudio")]
    [SerializeField] private float studioInfoStartDelay = 0.5f;
    [SerializeField] private float studioInfoFadeInDuration = 1.5f;
    [SerializeField] private float studioInfoDisplayTime = 3f;
    [SerializeField] private float studioInfoFadeOutDuration = 1.5f;
    
    [Header("Configuración de Tiempo - Créditos")]
    [SerializeField] private float creditsFadeInDuration = 1.5f;
    [SerializeField] private float creditsDisplayTime = 2f;
    [SerializeField] private float creditsFadeOutDuration = 1f;
    [SerializeField] private float buttonFadeInDuration = 1f;
    
    [Header("Configuración de la Pantalla")]
    [SerializeField] private bool startWithBlackScreen = true;
    
    private bool gameOverSequenceStarted = false;
    private bool canReturnToMenu = false;

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        // Reproducir audio al iniciar
        PlayGameOverAudio();

        Cursor.lockState = CursorLockMode.None; // Desbloquear el cursor
        Cursor.visible = true; // Hacer visible el cursor

        
        if (startWithBlackScreen)
        {
            StartGameOverSequence();
        }
    }

    private void InitializeUI()
    {
        // Inicializar pantalla negra
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.enabled = true;
        }
        
        // Inicializar título con opacidad 0
        if (gameTitle != null)
        {
            Color titleColor = gameTitle.color;
            titleColor.a = 0;
            gameTitle.color = titleColor;
            gameTitle.gameObject.SetActive(true);
        }
        
        // Inicializar texto de créditos con opacidad 0
        if (creditsText != null)
        {
            Color creditsColor = creditsText.color;
            creditsColor.a = 0;
            creditsText.color = creditsColor;
            creditsText.gameObject.SetActive(true);
            creditsText.text = "";
        }
        
        // Inicializar información del estudio con opacidad 0
        if (studioInfoObject != null)
        {
            // Intentar obtener CanvasGroup si existe, si no, crearlo
            CanvasGroup studioCanvasGroup = studioInfoObject.GetComponent<CanvasGroup>();
            if (studioCanvasGroup == null)
            {
                studioCanvasGroup = studioInfoObject.AddComponent<CanvasGroup>();
            }
            studioCanvasGroup.alpha = 0f;
            studioInfoObject.SetActive(true);
        }
        
        // Inicializar botón de retorno con opacidad 0
        if (returnToMenuButton != null)
        {
            CanvasGroup buttonCanvasGroup = returnToMenuButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
            {
                buttonCanvasGroup = returnToMenuButton.gameObject.AddComponent<CanvasGroup>();
            }
            buttonCanvasGroup.alpha = 0f;
            buttonCanvasGroup.interactable = false;
            returnToMenuButton.gameObject.SetActive(true);
        }
        
        canReturnToMenu = false;
    }

    public void StartGameOverSequence()
    {
        if (gameOverSequenceStarted) return;
        
        gameOverSequenceStarted = true;
        StartCoroutine(GameOverSequenceCoroutine());
    }

    private void PlayGameOverAudio()
    {
        if (AudioController.Instance != null && gameOverAudioClip != null)
        {

            AudioController.Instance.PlaySFXClip(gameOverAudioClip);
        }
    }

    private IEnumerator GameOverSequenceCoroutine()
    {
        // Asegurar que la pantalla esté negra
        EnsureBlackScreen();
        
        // 1. Hacer aparecer el título del juego
        yield return StartCoroutine(FadeInTitle());
        
        // 2. Mantener el título visible por el tiempo especificado
        yield return new WaitForSeconds(titleDisplayTime);
        
        // 3. Hacer desaparecer el título
        yield return StartCoroutine(FadeOutTitle());
        
        // 4. Esperar el delay inicial y mostrar información del estudio
        if (studioInfoObject != null)
        {
            yield return new WaitForSeconds(studioInfoStartDelay);
            yield return StartCoroutine(ShowStudioInfo());
        }
        
        // 5. Mostrar créditos uno por uno
        yield return StartCoroutine(ShowCreditsSequence());
        
        // 6. Mostrar botón de regreso al menú
        yield return StartCoroutine(ShowReturnButton());
        
        // 7. Habilitar el botón de regreso al menú
        canReturnToMenu = true;
    }

    private void EnsureBlackScreen()
    {
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.enabled = true;
        }
    }

    private IEnumerator FadeInTitle()
    {
        if (gameTitle == null) yield break;

        float elapsedTime = 0f;
        Color titleColor = gameTitle.color;
        float startAlpha = titleColor.a;
        
        while (elapsedTime < titleFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / titleFadeInDuration);
            
            titleColor.a = alpha;
            gameTitle.color = titleColor;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 1
        titleColor.a = 1f;
        gameTitle.color = titleColor;
    }

    private IEnumerator FadeOutTitle()
    {
        if (gameTitle == null) yield break;

        float elapsedTime = 0f;
        Color titleColor = gameTitle.color;
        float startAlpha = titleColor.a;
        
        while (elapsedTime < titleFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / titleFadeOutDuration);
            
            titleColor.a = alpha;
            gameTitle.color = titleColor;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 0
        titleColor.a = 0f;
        gameTitle.color = titleColor;
    }

    private IEnumerator ShowStudioInfo()
    {
        if (studioInfoObject == null) yield break;

        // Fade in de la información del estudio
        yield return StartCoroutine(FadeInStudioInfo());
        
        // Mantener visible por el tiempo especificado
        yield return new WaitForSeconds(studioInfoDisplayTime);
        
        // Fade out de la información del estudio
        yield return StartCoroutine(FadeOutStudioInfo());
    }

    private IEnumerator FadeInStudioInfo()
    {
        if (studioInfoObject == null) yield break;

        CanvasGroup studioCanvasGroup = studioInfoObject.GetComponent<CanvasGroup>();
        if (studioCanvasGroup == null)
        {
            studioCanvasGroup = studioInfoObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        float startAlpha = studioCanvasGroup.alpha;
        
        while (elapsedTime < studioInfoFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / studioInfoFadeInDuration);
            
            studioCanvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 1
        studioCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutStudioInfo()
    {
        if (studioInfoObject == null) yield break;

        CanvasGroup studioCanvasGroup = studioInfoObject.GetComponent<CanvasGroup>();
        if (studioCanvasGroup == null)
        {
            studioCanvasGroup = studioInfoObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        float startAlpha = studioCanvasGroup.alpha;
        
        while (elapsedTime < studioInfoFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / studioInfoFadeOutDuration);
            
            studioCanvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 0
        studioCanvasGroup.alpha = 0f;
    }

    private IEnumerator ShowCreditsSequence()
    {
        if (creditsText == null || credits == null || credits.Length == 0) yield break;

        // Mostrar cada crédito uno por uno
        for (int i = 0; i < credits.Length; i++)
        {
            // Configurar el texto del crédito actual
            string creditText = $"{credits[i].name}\n{credits[i].role}";
            creditsText.text = creditText;
            
            // Fade in del crédito
            yield return StartCoroutine(FadeInCreditsText());
            
            // Mantener visible por el tiempo especificado
            yield return new WaitForSeconds(creditsDisplayTime);
            
            // Fade out del crédito
            yield return StartCoroutine(FadeOutCreditsText());
        }
    }

    private IEnumerator FadeInCreditsText()
    {
        if (creditsText == null) yield break;

        float elapsedTime = 0f;
        Color creditsColor = creditsText.color;
        float startAlpha = creditsColor.a;
        
        while (elapsedTime < creditsFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / creditsFadeInDuration);
            
            creditsColor.a = alpha;
            creditsText.color = creditsColor;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 1
        creditsColor.a = 1f;
        creditsText.color = creditsColor;
    }

    private IEnumerator FadeOutCreditsText()
    {
        if (creditsText == null) yield break;

        float elapsedTime = 0f;
        Color creditsColor = creditsText.color;
        float startAlpha = creditsColor.a;
        
        while (elapsedTime < creditsFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / creditsFadeOutDuration);
            
            creditsColor.a = alpha;
            creditsText.color = creditsColor;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 0
        creditsColor.a = 0f;
        creditsText.color = creditsColor;
    }

    private IEnumerator ShowReturnButton()
    {
        if (returnToMenuButton == null) yield break;

        CanvasGroup buttonCanvasGroup = returnToMenuButton.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = returnToMenuButton.gameObject.AddComponent<CanvasGroup>();
        }

        // Inicializar con alpha 0
        buttonCanvasGroup.alpha = 0f;
        buttonCanvasGroup.interactable = false;
        
        // Fade in del botón
        float elapsedTime = 0f;
        
        while (elapsedTime < buttonFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / buttonFadeInDuration);
            
            buttonCanvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        // Asegurar que el alpha final sea exactamente 1 y habilitar interacción
        buttonCanvasGroup.alpha = 1f;
        buttonCanvasGroup.interactable = true;
    }

    public void ReturnToMainMenu()
    {
        if (!canReturnToMenu)
        {
            Debug.LogWarning("No se puede volver al menú principal aún. La secuencia de Game Over no ha terminado.");
            return;
        }
        
        StartCoroutine(ReturnToMainMenuCoroutine());
    }

    private IEnumerator ReturnToMainMenuCoroutine()
    {
        GameController.Instance.ResetValues(); // Reiniciar valores del juego
        GameFlowManager.Instance.ResetGameFlow(); // Reiniciar flujo del juego
        //SceneController.Instance.LoadInitialScene(); // Ajustar el nombre de la escena según corresponda

        // Opcional: Hacer fade out antes de cambiar de escena
        /*if (FadeManager.Instance != null)
        {
            // Si existe un FadeManager, usarlo para transición suave
            // FadeManager.Instance.FadeToScene("MainMenu");
        }
        else
        {
            // Cargar directamente la escena del menú principal
            SceneController.Instance.LoadInitialScene(); // Ajustar el nombre de la escena según corresponda
        }*/

        yield return null;
    }

    // Método público para activar manualmente el Game Over desde otros scripts
    public void TriggerGameOver()
    {
        if (!gameOverSequenceStarted)
        {
            StartGameOverSequence();
        }
    }

    // Método para debugging en el editor
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        // Validar que las duraciones sean positivas
        titleFadeInDuration = Mathf.Max(0.1f, titleFadeInDuration);
        titleDisplayTime = Mathf.Max(0.1f, titleDisplayTime);
        titleFadeOutDuration = Mathf.Max(0.1f, titleFadeOutDuration);
        
        studioInfoStartDelay = Mathf.Max(0f, studioInfoStartDelay);
        studioInfoFadeInDuration = Mathf.Max(0.1f, studioInfoFadeInDuration);
        studioInfoDisplayTime = Mathf.Max(0.1f, studioInfoDisplayTime);
        studioInfoFadeOutDuration = Mathf.Max(0.1f, studioInfoFadeOutDuration);
        
        creditsFadeInDuration = Mathf.Max(0.1f, creditsFadeInDuration);
        creditsDisplayTime = Mathf.Max(0.1f, creditsDisplayTime);
        creditsFadeOutDuration = Mathf.Max(0.1f, creditsFadeOutDuration);
        buttonFadeInDuration = Mathf.Max(0.1f, buttonFadeInDuration);
    }

#if UNITY_EDITOR
    [Header("Debug (Solo en Editor)")]
    [SerializeField] private bool debugMode = false;
    
    private void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.G))
        {
            TriggerGameOver();
        }
    }
#endif
}
