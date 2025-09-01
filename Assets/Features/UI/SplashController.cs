using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image splashImage;          // La imagen del splash del estudio
    [SerializeField] private Image backgroundImage;      // El objeto UI "Background" en Canvas
    [SerializeField] private GameObject headphonesInfoObject; // GameObject con información de auriculares recomendados

    [Header("Configuración de Tiempo - Splash del Estudio")]
    [SerializeField] private float splashFadeInDuration = 1f;     // Duración del fade in del splash
    [SerializeField] private float splashDisplayTime = 2f;       // Tiempo que se muestra el splash
    [SerializeField] private float splashFadeOutDuration = 1f;   // Duración del fade out del splash

    [Header("Configuración de Tiempo - Información de Auriculares")]
    [SerializeField] private float headphonesFadeInDuration = 1f;  // Duración del fade in de auriculares
    [SerializeField] private float headphonesDisplayTime = 2f;     // Tiempo que se muestra la info de auriculares
    [SerializeField] private float headphonesFadeOutDuration = 1f; // Duración del fade out de auriculares

    [Header("Configuración General")]
    [SerializeField] private float finalWait = 0.5f;        // Tiempo de espera final antes de cambiar escena
    [SerializeField] private string nextSceneName = "LoadingScene"; // Nombre de la escena siguiente

    private void Start()
    {
        // Inicializa el fondo UI en negro y completamente opaco
        backgroundImage.color = new Color(0f, 0f, 0f, 1f);

        // Inicializar elementos con transparencia
        InitializeUI();

        // Comenzar la secuencia de splash
        StartCoroutine(SplashSequence());
    }

    private void InitializeUI()
    {
        // Inicializar splash del estudio con opacidad 0
        if (splashImage != null)
        {
            Color splashColor = splashImage.color;
            splashColor.a = 0f;
            splashImage.color = splashColor;
            splashImage.enabled = true;
        }

        // Inicializar información de auriculares con opacidad 0
        if (headphonesInfoObject != null)
        {
            CanvasGroup headphonesCanvasGroup = headphonesInfoObject.GetComponent<CanvasGroup>();
            if (headphonesCanvasGroup == null)
            {
                headphonesCanvasGroup = headphonesInfoObject.AddComponent<CanvasGroup>();
            }
            headphonesCanvasGroup.alpha = 0f;
            headphonesInfoObject.SetActive(true);
        }
    }

    private IEnumerator SplashSequence()
    {
        // 1. Mostrar splash del estudio
        yield return StartCoroutine(ShowStudioSplash());

        // 2. Mostrar información de auriculares
        if (headphonesInfoObject != null)
        {
            yield return StartCoroutine(ShowHeadphonesInfo());
        }

        // 3. Espera final antes de cargar la siguiente escena
        yield return new WaitForSeconds(finalWait);

        // 4. Cargar siguiente escena
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator ShowStudioSplash()
    {
        if (splashImage == null) yield break;

        // Fade in del splash del estudio
        yield return StartCoroutine(FadeInSplash());

        // Mantener visible por el tiempo especificado
        yield return new WaitForSeconds(splashDisplayTime);

        // Fade out del splash del estudio
        yield return StartCoroutine(FadeOutSplash());
    }

    private IEnumerator FadeInSplash()
    {
        if (splashImage == null) yield break;

        float elapsedTime = 0f;
        Color splashColor = splashImage.color;
        float startAlpha = splashColor.a;

        while (elapsedTime < splashFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / splashFadeInDuration);

            splashColor.a = alpha;
            splashImage.color = splashColor;

            yield return null;
        }

        // Asegurar que el alpha final sea exactamente 1
        splashColor.a = 1f;
        splashImage.color = splashColor;
    }

    private IEnumerator FadeOutSplash()
    {
        if (splashImage == null) yield break;

        float elapsedTime = 0f;
        Color splashColor = splashImage.color;
        float startAlpha = splashColor.a;

        while (elapsedTime < splashFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / splashFadeOutDuration);

            splashColor.a = alpha;
            splashImage.color = splashColor;

            yield return null;
        }

        // Asegurar que el alpha final sea exactamente 0
        splashColor.a = 0f;
        splashImage.color = splashColor;
    }

    private IEnumerator ShowHeadphonesInfo()
    {
        if (headphonesInfoObject == null) yield break;

        // Fade in de la información de auriculares
        yield return StartCoroutine(FadeInHeadphonesInfo());

        // Mantener visible por el tiempo especificado
        yield return new WaitForSeconds(headphonesDisplayTime);

        // Fade out de la información de auriculares
        yield return StartCoroutine(FadeOutHeadphonesInfo());
    }

    private IEnumerator FadeInHeadphonesInfo()
    {
        if (headphonesInfoObject == null) yield break;

        CanvasGroup headphonesCanvasGroup = headphonesInfoObject.GetComponent<CanvasGroup>();
        if (headphonesCanvasGroup == null)
        {
            headphonesCanvasGroup = headphonesInfoObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        float startAlpha = headphonesCanvasGroup.alpha;

        while (elapsedTime < headphonesFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / headphonesFadeInDuration);

            headphonesCanvasGroup.alpha = alpha;

            yield return null;
        }

        // Asegurar que el alpha final sea exactamente 1
        headphonesCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutHeadphonesInfo()
    {
        if (headphonesInfoObject == null) yield break;

        CanvasGroup headphonesCanvasGroup = headphonesInfoObject.GetComponent<CanvasGroup>();
        if (headphonesCanvasGroup == null)
        {
            headphonesCanvasGroup = headphonesInfoObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        float startAlpha = headphonesCanvasGroup.alpha;

        while (elapsedTime < headphonesFadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / headphonesFadeOutDuration);

            headphonesCanvasGroup.alpha = alpha;

            yield return null;
        }

        // Asegurar que el alpha final sea exactamente 0
        headphonesCanvasGroup.alpha = 0f;
    }

    // Método para debugging en el editor
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        // Validar que las duraciones sean positivas
        splashFadeInDuration = Mathf.Max(0.1f, splashFadeInDuration);
        splashDisplayTime = Mathf.Max(0.1f, splashDisplayTime);
        splashFadeOutDuration = Mathf.Max(0.1f, splashFadeOutDuration);

        headphonesFadeInDuration = Mathf.Max(0.1f, headphonesFadeInDuration);
        headphonesDisplayTime = Mathf.Max(0.1f, headphonesDisplayTime);
        headphonesFadeOutDuration = Mathf.Max(0.1f, headphonesFadeOutDuration);

        finalWait = Mathf.Max(0f, finalWait);
    }

#if UNITY_EDITOR
    [Header("Debug (Solo en Editor)")]
    [SerializeField] private bool debugMode = false;

    private void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(SplashSequence());
        }
    }
#endif
}
