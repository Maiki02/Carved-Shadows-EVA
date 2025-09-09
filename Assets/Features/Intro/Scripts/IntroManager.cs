using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [SerializeField] private string introText = "La noche se oscurece... El día se aclara...";
    [SerializeField] private float textFadeInDuration = 1.5f;
    [SerializeField] private float textDisplayDuration = 3f;
    [SerializeField] private float textFadeOutDuration = 2f;
    [SerializeField] private float textSize = 48f;

    [Header("Audio - Lluvia")]
    [SerializeField] private float rainStartDelay = 1f;
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private AudioClip rainClip;

    [Header("Audio - Gritos")]
    [SerializeField] private float screamsStartDelay = 4f;
    [SerializeField] private AudioSource screamsAudioSource;
    [SerializeField] private AudioClip womanScreamClip;
    [SerializeField] private AudioClip childScreamClip;
    [SerializeField] private float screamsFadeInDuration = 2f;
    [SerializeField] private float screamsMaxVolume = 0.3f;

    [Header("Duración Total")]
    [SerializeField] private float totalIntroDuration = 8f;
    
    // Getter público para acceder a la duración total
    public float GetTotalIntroDuration() => totalIntroDuration + textFadeInDuration + textDisplayDuration + textFadeOutDuration + 2f; // +2f para fade final

    // TODO: Casa en llamas (implementar con gif o animación)
    /*
    [Header("Casa en Llamas")]
    [SerializeField] private GameObject burningHouseGif;
    [SerializeField] private float houseShowDelay = 6f;
    */

    void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        float startTime = Time.time;

        // 1. Mostrar texto con fade usando FadeManager
        StartCoroutine(ShowIntroText());

        // 2. Iniciar lluvia después del delay
        StartCoroutine(StartRainAfterDelay());

        // 3. Iniciar gritos después del delay
        StartCoroutine(StartScreamsAfterDelay());

        // 4. TODO: Mostrar casa en llamas
        /*
        StartCoroutine(ShowBurningHouseAfterDelay());
        */

        // 5. Esperar duración total de la intro
        yield return new WaitForSeconds(totalIntroDuration);

        // 6. Fade final para aumentar volumen de gritos durante fade out
        yield return StartCoroutine(FinalFadeWithScreamIntensity());

        Debug.Log("Intro completada");
    }

    private IEnumerator ShowIntroText()
    {
        yield return StartCoroutine(
            FadeManager.Instance.ShowTextWithFadeCoroutine(
                introText, 
                textFadeInDuration, 
                textDisplayDuration, 
                textFadeOutDuration, 
                textSize
            )
        );
    }

    private IEnumerator StartRainAfterDelay()
    {
        yield return new WaitForSeconds(rainStartDelay);
        
        if (rainAudioSource != null && rainClip != null)
        {
            rainAudioSource.clip = rainClip;
            rainAudioSource.loop = true;
            rainAudioSource.volume = 0.4f; // Volumen moderado para la lluvia
            rainAudioSource.Play();
            Debug.Log("Iniciando sonido de lluvia");
        }
        else if (AudioController.Instance != null)
        {
            // Fallback usando AudioController si no hay AudioSource específico
            AudioController.Instance.PlaySFX(AudioType.Rain, true);
        }
    }

    private IEnumerator StartScreamsAfterDelay()
    {
        yield return new WaitForSeconds(screamsStartDelay);
        
        if (screamsAudioSource != null)
        {
            // Configurar AudioSource para gritos
            screamsAudioSource.volume = 0f; // Empezar en silencio
            screamsAudioSource.loop = true;
            
            // Alternar entre grito de mujer y niña (puedes ajustar esta lógica)
            AudioClip screamToPlay = Random.value > 0.5f ? womanScreamClip : childScreamClip;
            if (screamToPlay != null)
            {
                screamsAudioSource.clip = screamToPlay;
                screamsAudioSource.Play();
                
                // Fade in gradual de los gritos
                yield return StartCoroutine(FadeAudioVolume(screamsAudioSource, 0f, screamsMaxVolume, screamsFadeInDuration));
                Debug.Log("Iniciando gritos de fondo");
            }
        }
    }

    private IEnumerator FinalFadeWithScreamIntensity()
    {
        // Durante el fade out final, intensificar los gritos
        if (screamsAudioSource != null && screamsAudioSource.isPlaying)
        {
            float targetVolume = screamsMaxVolume * 1.5f; // Aumentar 50% más
            yield return StartCoroutine(FadeAudioVolume(screamsAudioSource, screamsAudioSource.volume, targetVolume, 1f));
        }
        
        yield return new WaitForSeconds(1f); // Esperar un poco más para el efecto dramático
    }

    private IEnumerator FadeAudioVolume(AudioSource audioSource, float fromVolume, float toVolume, float duration)
    {
        if (audioSource == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            audioSource.volume = Mathf.Lerp(fromVolume, toVolume, normalizedTime);
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
            
        if (screamsAudioSource != null && screamsAudioSource.isPlaying)
            screamsAudioSource.Stop();
    }
}
