using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Referencias UI (Canvas)")]
    [SerializeField] private Image blackImage;     // Imagen full-screen para parpadeos
    [SerializeField] private Image topBar;         // Barra superior para cierre de vista
    [SerializeField] private Image bottomBar;      // Barra inferior para cierre de vista
    private TextMeshProUGUI fadeText; // Texto para mostrar en pantalla
    [SerializeField] private GameObject fadeTextGameObject; // GameObject del texto para activar/desactivar

    [Header("Duraciones")]
    [SerializeField] private float blinkDuration = 1.5f;  // Tiempo de cada fase de parpadeo
    [SerializeField] private float blackImageDuration = 2f;  // Tiempo de espera con la pantalla en negro
    
    [Header("Configuración de Texto")]
    [SerializeField] private float defaultTextSize = 32f;  // Tamaño de texto por defecto
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicializar estados
            if (blackImage != null)
            {
                blackImage.color = new Color(0, 0, 0, 0);
                blackImage.enabled = false;
            }

            if (topBar != null)
            {
                topBar.enabled = false;
                topBar.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            }

            if (bottomBar != null)
            {
                bottomBar.enabled = false;
                bottomBar.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            }

            if (fadeTextGameObject != null)
            {
                fadeText = fadeTextGameObject.GetComponent<TextMeshProUGUI>();
                fadeText.text = "";
                fadeText.fontSize = defaultTextSize;
                fadeText.color = new Color(fadeText.color.r, fadeText.color.g, fadeText.color.b, 0);
                fadeTextGameObject.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Ejecuta la secuencia de “desmayo” y al terminar, teleporta al punto deseado
    public void SimulateFaintAndLoad(GameObject gameObjectToTeleport, Transform pointToTeleport)
    {
        StartCoroutine(FaintAndLoadRoutine(gameObjectToTeleport, pointToTeleport));
    }

    public IEnumerator Oscurecer()
    {
        yield return StartCoroutine(FadeImage(blackImage, 0f, 1f, blinkDuration));
    }

    /// <summary>
    /// Fade Out público - Oscurece la pantalla con duración específica
    /// </summary>
    /// <param name="duration">Duración del fade out en segundos</param>
    public void FadeOut(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    /// <summary>
    /// Fade In público - Aclara la pantalla con duración específica
    /// </summary>
    /// <param name="duration">Duración del fade in en segundos</param>
    public void FadeIn(float duration)
    {
        StartCoroutine(FadeInCoroutine(duration));
    }

    /// <summary>
    /// Fade Out que retorna corrutina para poder ser esperada
    /// </summary>
    /// <param name="duration">Duración del fade out en segundos</param>
    public IEnumerator FadeOutCoroutine(float duration)
    {
        yield return StartCoroutine(FadeImage(blackImage, 0f, 1f, duration));
    }

    /// <summary>
    /// Fade In que retorna corrutina para poder ser esperada
    /// </summary>
    /// <param name="duration">Duración del fade in en segundos</param>
    public IEnumerator FadeInCoroutine(float duration)
    {
        yield return StartCoroutine(FadeImage(blackImage, 1f, 0f, duration));
    }

    public IEnumerator FaintAndLoadRoutine(GameObject go, Transform pt)
    {
        // 1) Fade out (oscurecer)
        yield return StartCoroutine(FadeImage(blackImage, 0f, 1f, blinkDuration));

        // 2) Caída del jugador
        var pc = go.GetComponent<PlayerController>();
        // Lanza la caída y espera a que termine
        pc.FallToTheGround();
        yield return new WaitForSeconds(pc.GetFallDuration() + 0.1f);

        // 3) Asegurar pantalla completamente negra
        blackImage.color = new Color(0, 0, 0, 1f);

        // 4) Teleport (preservando Y actual)
        pc.SetStatusCharacterController(false); // Desactiva el CharacterController para evitar problemas al teleportar
        Debug.Log("Teleporting player to: " + pt.position);
        Vector3 cur = go.transform.position;
        Vector3 tgt = pt.position;
        Vector3 teleportPos = new Vector3(tgt.x, cur.y, tgt.z);
        go.transform.SetPositionAndRotation(teleportPos, pt.rotation);
        pc.SetStatusCharacterController(true);

        Debug.Log("Player teleported to: " + go.transform.position);

        // 5) Fade in (abrir los ojos)
        yield return StartCoroutine(FadeImage(blackImage, 1f, 0f, blinkDuration));
    }

    /// <summary>
    /// Muestra texto en pantalla con animaciones de fade
    /// </summary>
    /// <param name="text">Contenido del texto a mostrar</param>
    /// <param name="fadeInDuration">Duración de aparición del texto</param>
    /// <param name="displayDuration">Duración de permanencia del texto</param>
    /// <param name="fadeOutDuration">Duración de desaparición del texto</param>
    /// <param name="textSize">Tamaño del texto (opcional, por defecto usa defaultTextSize)</param>
    public void ShowTextWithFade(string text, float fadeInDuration, float displayDuration, float fadeOutDuration, float textSize = -1f)
    {
        StartCoroutine(ShowTextWithFadeCoroutine(text, fadeInDuration, displayDuration, fadeOutDuration, textSize));
    }

    /// <summary>
    /// Corrutina para mostrar texto con fade que puede ser esperada
    /// </summary>
    /// <param name="text">Contenido del texto a mostrar</param>
    /// <param name="fadeInDuration">Duración de aparición del texto</param>
    /// <param name="displayDuration">Duración de permanencia del texto</param>
    /// <param name="fadeOutDuration">Duración de desaparición del texto</param>
    /// <param name="textSize">Tamaño del texto (opcional, por defecto usa defaultTextSize)</param>
    public IEnumerator ShowTextWithFadeCoroutine(string text, float fadeInDuration, float displayDuration, float fadeOutDuration, float textSize = -1f)
    {
        Debug.Log($"[FadeManager] ShowTextWithFadeCoroutine iniciado: fadeIn={fadeInDuration}s, display={displayDuration}s, fadeOut={fadeOutDuration}s");
        
        if (fadeText == null) 
        {
            Debug.LogError("[FadeManager] fadeText es null!");
            yield break;
        }
        
        this.fadeTextGameObject.SetActive(true);
        // Configurar el texto
        fadeText.text = text;
        fadeText.fontSize = textSize > 0 ? textSize : defaultTextSize;

        Debug.Log($"[FadeManager] Iniciando Fade In por {fadeInDuration}s...");
        // Fade In del texto
        yield return StartCoroutine(FadeText(fadeText, 0f, 1f, fadeInDuration));

        Debug.Log($"[FadeManager] Fade In completado. Mostrando texto por {displayDuration}s...");
        // Mantener el texto visible
        yield return new WaitForSeconds(displayDuration);

        Debug.Log($"[FadeManager] Iniciando Fade Out por {fadeOutDuration}s...");
        // Fade Out del texto
        yield return StartCoroutine(FadeText(fadeText, 1f, 0f, fadeOutDuration));
        
        Debug.Log("[FadeManager] ShowTextWithFadeCoroutine completado");
        this.fadeTextGameObject.SetActive(false);
    }

    private IEnumerator BlinkCoroutine(float fromA, float toA, float duration)
    {

        yield return StartCoroutine(FadeImage(blackImage, fromA, toA, duration));
        //  yield return StartCoroutine(FadeImage(blackImage,   1f, 0f, blinkDuration ));

    }

    private IEnumerator FadeImage(Image img, float fromA, float toA, float dur)
    {
        if (img == null) yield break;

        // Validar duración - si es 0 o negativa, aplicar el valor final inmediatamente
        if (dur <= 0f)
        {
            img.enabled = true;
            img.color = new Color(0, 0, 0, toA);
            if (Mathf.Approximately(toA, 0f))
                img.enabled = false;
            yield break;
        }

        img.enabled = true;
        Color c = img.color;
        c.a = fromA;
        img.color = c;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / dur); // Clamp para evitar valores > 1
            float a = Mathf.Lerp(fromA, toA, normalizedTime);
            img.color = new Color(0, 0, 0, a);
            yield return null;
        }

        // asegurar alpha final
        img.color = new Color(0, 0, 0, toA);
        if (Mathf.Approximately(toA, 0f))
            img.enabled = false;
    }

    private IEnumerator FadeText(TextMeshProUGUI textComponent, float fromA, float toA, float dur)
    {
        if (textComponent == null) yield break;
        
        Debug.Log($"[FadeManager] FadeText iniciado: fromA={fromA}, toA={toA}, duration={dur}");

        // Validar duración - si es 0 o negativa, aplicar el valor final inmediatamente
        if (dur <= 0f)
        {
            Debug.LogWarning($"[FadeManager] Duración inválida ({dur}), aplicando fade inmediato");
            textComponent.enabled = true;
            Color immediateColor = textComponent.color;
            immediateColor.a = toA;
            textComponent.color = immediateColor;

            if (Mathf.Approximately(toA, 0f))
                textComponent.enabled = false;
            yield break;
        }

        textComponent.enabled = true;
        Color c = textComponent.color;
        c.a = fromA;
        textComponent.color = c;

        float t = 0f;
        float startTime = Time.unscaledTime;
        Debug.Log($"[FadeManager] Iniciando fade loop. Time.timeScale={Time.timeScale}");
        
        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // Usar unscaledDeltaTime para que no se vea afectado por timeScale
            float normalizedTime = Mathf.Clamp01(t / dur); // Clamp para evitar valores > 1
            float a = Mathf.Lerp(fromA, toA, normalizedTime);
            Color newColor = textComponent.color;
            newColor.a = a;
            textComponent.color = newColor;
            
            // Debug cada segundo aproximadamente
            if (Mathf.FloorToInt(t) != Mathf.FloorToInt(t - Time.unscaledDeltaTime))
            {
                Debug.Log($"[FadeManager] Fade progress: t={t:F2}s/{dur}s, alpha={a:F2}, unscaledDeltaTime={Time.unscaledDeltaTime:F4}");
            }
            
            yield return null;
        }

        float totalTime = Time.unscaledTime - startTime;
        Debug.Log($"[FadeManager] FadeText completado en {totalTime:F2}s (esperado: {dur}s)");

        // asegurar alpha final
        Color finalColor = textComponent.color;
        finalColor.a = toA;
        textComponent.color = finalColor;
        
        if (Mathf.Approximately(toA, 0f))
            textComponent.enabled = false;
    }

    /*private IEnumerator BarsCloseCoroutine()
    {
        if (topBar == null || bottomBar == null)
            yield break;

        topBar.enabled    = true;
        bottomBar.enabled = true;

        RectTransform topRT    = topBar.GetComponent<RectTransform>();
        RectTransform bottomRT = bottomBar.GetComponent<RectTransform>();
        float t                = 0f;
        float halfH            = Screen.height / 2f;

        while (t < barsCloseDuration)
        {
            t += Time.deltaTime;
            float h = Mathf.Lerp(0f, halfH, t / barsCloseDuration);
            topRT.sizeDelta    = new Vector2(topRT.sizeDelta.x,    h);
            bottomRT.sizeDelta = new Vector2(bottomRT.sizeDelta.x, h);
            yield return null;
        }

        topRT.sizeDelta    = new Vector2(topRT.sizeDelta.x,    halfH);
        bottomRT.sizeDelta = new Vector2(bottomRT.sizeDelta.x, halfH);
    }*/
}
