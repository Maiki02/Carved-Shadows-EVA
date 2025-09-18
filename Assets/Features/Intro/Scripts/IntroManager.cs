using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [SerializeField] private TextMeshProUGUI introTextUI; // Referencia al componente TextMeshPro
    [SerializeField] private string introText = "La noche se oscurece... El día se aclara...";
    [SerializeField] private float initialWaitTime = 2f; // Tiempo de espera antes de comenzar la escritura
    [SerializeField] private float typingDuration = 20f; // Duración total del efecto máquina de escribir
    [SerializeField] private float textDisplayDuration = 1f; // Tiempo que el texto permanece visible después de escribirse
    [SerializeField] private float textFadeOutDuration = 2f;
    [SerializeField] private float textFadeOutDelay = 2f;

    [Header("Configuración de Escena")]
    [SerializeField] private string nextSceneName = "Loop_01"; // Escena a cargar después de la intro
    
    private AsyncOperation sceneLoadOperation; // Para cargar la escena de forma asíncrona

    [Header("Audio - Lluvia")]
    [SerializeField] private float rainStartDelay = 2f;
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private AudioClip rainClip;
    [SerializeField] private float rainFadeInDuration = 4f; // Aumentado a 4 segundos para fade más gradual
    [SerializeField] private float rainMaxVolume = 0.4f;

    public void Awake()
    {

        // Hacer el texto invisible al inicio
        introTextUI.alpha = 0f;
        introTextUI.text = "";
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void Start()
    {
        // Iniciar carga asíncrona de la siguiente escena
        StartCoroutine(LoadNextSceneAsync());

        // Iniciar efectos de intro
        StartCoroutine(ShowIntroText());
        StartCoroutine(StartRainAfterDelay());
    }

    // Getter público para acceder a la duración total
    public float GetTotalIntroDuration() => initialWaitTime + typingDuration + textDisplayDuration + textFadeOutDuration + textFadeOutDelay; // +2f para fade final

    // TODO: Casa en llamas (implementar con gif o animación)
    /*
    [Header("Casa en Llamas")]
    [SerializeField] private GameObject burningHouseGif;
    [SerializeField] private float houseShowDelay = 6f;
    */

    public IEnumerator ShowIntroText()
    {
        Debug.Log($"[IntroManager] ShowIntroText iniciado. typingDuration={typingDuration}s");
        
        if (introTextUI == null)
        {
            Debug.LogError("[IntroManager] introTextUI es null! Asigna un TextMeshProUGUI en el inspector.");
            yield break;
        }
        
        // Esperar el tiempo inicial antes de comenzar
        yield return new WaitForSeconds(initialWaitTime);
        
        
        // Fade in inicial del texto (opcional, para suavizar la aparición)
        yield return FadeTextAlpha(introTextUI, 0f, 1f, 0.5f);
        
        // Efecto máquina de escribir
        yield return TypewriterEffect(introText, typingDuration);
        
        // Mantener el texto visible
        yield return new WaitForSeconds(textDisplayDuration);
        
        // Fade out final del texto
        yield return FadeTextAlpha(introTextUI, 1f, 0f, textFadeOutDuration);
        
        Debug.Log("[IntroManager] ShowIntroText completado");
        
        // Cambiar a la siguiente escena una vez terminada la intro
        yield return ChangeToNextScene();
    }

    /// <summary>
    /// Carga la siguiente escena de forma asíncrona en segundo plano
    /// </summary>
    private IEnumerator LoadNextSceneAsync()
    {
        Debug.Log($"[IntroManager] Iniciando carga asíncrona de escena: {nextSceneName}");
        
        sceneLoadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        sceneLoadOperation.allowSceneActivation = false; // No activar automáticamente
        
        // Esperar hasta que la escena esté cargada (pero no activada)
        while (sceneLoadOperation.progress < 0.9f)
        {
            Debug.Log($"[IntroManager] Progreso de carga: {sceneLoadOperation.progress * 100f}%");
            yield return null;
        }
        
        Debug.Log($"[IntroManager] Escena {nextSceneName} cargada y lista para activar");
    }

    /// <summary>
    /// Activa la escena que se cargó de forma asíncrona
    /// </summary>
    private IEnumerator ChangeToNextScene()
    {
        Debug.Log("[IntroManager] Cambiando a la siguiente escena...");
        
        // Detener todos los sonidos antes del cambio
        StopSounds();
        
        // Esperar a que la carga asíncrona esté completa
        if (sceneLoadOperation != null)
        {
            while (sceneLoadOperation.progress < 0.9f)
            {
                yield return null;
            }
            
            // Activar la nueva escena
            sceneLoadOperation.allowSceneActivation = true;
        }
        else
        {
            // Fallback: cargar la escena de forma directa si no se cargó antes
            Debug.LogWarning("[IntroManager] No se encontró carga asíncrona, cargando escena directamente");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void StopSounds() {
        if (rainAudioSource != null && rainAudioSource.isPlaying) {
            rainAudioSource.Stop();
        }
    }

    public IEnumerator StartRainAfterDelay()
    {
        yield return new WaitForSeconds(rainStartDelay);
        
        if (rainClip == null)
        {
            yield break;
        }

        // Configurar AudioSource
        rainAudioSource.volume = 0f; // Empezar en silencio para fade in
        rainAudioSource.loop = true;
        rainAudioSource.clip = rainClip;

        rainAudioSource.Play();


        if (rainAudioSource.isPlaying)
        {            
            // Fade in muy gradual para evitar inicio abrupto
            yield return FadeAudioVolume(rainAudioSource, 0f, rainMaxVolume, rainFadeInDuration);
        }
    }

    /// <summary>
    /// Efecto máquina de escribir que muestra el texto carácter por carácter
    /// </summary>
    private IEnumerator TypewriterEffect(string textToShow, float duration)
    {
        if (introTextUI == null || string.IsNullOrEmpty(textToShow)) yield break;
        
        // Convertir \n a saltos de línea reales antes de procesar
        string processedText = textToShow.Replace("\\n", "\n");
        
        int totalCharacters = processedText.Length;
        float timePerCharacter = duration / totalCharacters;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            introTextUI.text = processedText.Substring(0, i);
            yield return new WaitForSeconds(timePerCharacter);
        }
    }

    /// <summary>
    /// Hace fade in/out del alpha del texto TextMeshPro
    /// </summary>
    private IEnumerator FadeTextAlpha(TextMeshProUGUI textComponent, float fromAlpha, float toAlpha, float duration)
    {
        if (textComponent == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            textComponent.alpha = Mathf.Lerp(fromAlpha, toAlpha, normalizedTime);
            yield return null;
        }
        
        textComponent.alpha = toAlpha;
    }


    private IEnumerator FadeAudioVolume(AudioSource audioSource, float fromVolume, float toVolume, float duration)
    {
        if (audioSource == null) yield break;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Usar una curva suave para el fade (SmoothStep para transición más natural)
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            audioSource.volume = Mathf.Lerp(fromVolume, toVolume, smoothTime);
            
            yield return null;
        }
        
        audioSource.volume = toVolume;
    }

    // TODO: Implementar casa en llamas con gif
    /*
    private IEnumerator ShowBurningHouseAfterDelay()
    {
        yield return new WaitForSeconds(houseShowDelay);
        
        if (burningHouseGif != null)
        {
            burningHouseGif.SetActive(true);
            Debug.Log("Mostrando casa en llamas");
        }
    }
    */

    private void OnDestroy()
    {
        // Cleanup: detener todos los audios al destruir el objeto
        if (rainAudioSource != null && rainAudioSource.isPlaying)
            rainAudioSource.Stop();
    }
}
