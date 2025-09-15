using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [SerializeField] private string introText = "La noche se oscurece... El día se aclara...";
    [SerializeField] public float textFadeInDuration = 10f;
    [SerializeField] private float textDisplayDuration = 10f;
    [SerializeField] private float textFadeOutDuration = 5f;
    [SerializeField] private float textSize = 48f;

    [Header("Audio - Lluvia")]
    [SerializeField] private float rainStartDelay = 2f;
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private AudioClip rainClip;
    [SerializeField] private float rainFadeInDuration = 4f; // Aumentado a 4 segundos para fade más gradual
    [SerializeField] private float rainMaxVolume = 0.4f;

    [Header("Audio - Gritos")]
    [SerializeField] private float screamsStartDelay = 6f;
    [SerializeField] private AudioSource screamsAudioSource;
    [SerializeField] private AudioClip womanScreamClip;
    [SerializeField] private AudioClip childScreamClip;
    [SerializeField] private float screamsFadeInDuration = 2f;
    [SerializeField] private float screamsMaxVolume = 0.3f;
    
    
    // Getter público para acceder a la duración total
    public float GetTotalIntroDuration() => textFadeInDuration + textDisplayDuration + textFadeOutDuration + 2f; // +2f para fade final

    // TODO: Casa en llamas (implementar con gif o animación)
    /*
    [Header("Casa en Llamas")]
    [SerializeField] private GameObject burningHouseGif;
    [SerializeField] private float houseShowDelay = 6f;
    */

    public IEnumerator ShowIntroText()
    {
        Debug.Log($"[IntroManager] ShowIntroText iniciado. textFadeInDuration={textFadeInDuration}s");
        
        if (FadeManager.Instance == null)
        {
            Debug.LogError("[IntroManager] FadeManager.Instance es null!");
            yield break;
        }
        
        yield return FadeManager.Instance.ShowTextWithFadeCoroutine(
            introText,
            textFadeInDuration,
            textDisplayDuration,
            textFadeOutDuration,
            textSize
        );
        
        Debug.Log("[IntroManager] ShowIntroText completado");
    }

    public void StopSounds() {
        if (rainAudioSource != null && rainAudioSource.isPlaying) {
            rainAudioSource.Stop();
        }
        if (screamsAudioSource != null && screamsAudioSource.isPlaying) {
            screamsAudioSource.Stop();
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

    public IEnumerator StartScreamsAfterDelay()
    {
        yield return new WaitForSeconds(screamsStartDelay);
        
        if (screamsAudioSource != null)
        {
            // Asegurar que el AudioSource esté habilitado
            screamsAudioSource.enabled = true;
            screamsAudioSource.volume = 0f; // Empezar en silencio
            screamsAudioSource.loop = true;
            
            // Alternar entre grito de mujer y niña (puedes ajustar esta lógica)
            AudioClip screamToPlay = Random.value > 0.5f ? womanScreamClip : childScreamClip;
                        
            if (screamToPlay != null)
            {
                screamsAudioSource.clip = screamToPlay;
                screamsAudioSource.Play();
                
                
                // Fade in gradual de los gritos
                yield return FadeAudioVolume(screamsAudioSource, 0f, screamsMaxVolume, screamsFadeInDuration);
            }
        }
    }

    public IEnumerator FinalFadeWithScreamIntensity()
    {
        // Durante el fade out final, intensificar los gritos
        if (screamsAudioSource != null && screamsAudioSource.isPlaying)
        {
            float targetVolume = screamsMaxVolume * 1.5f; // Aumentar 50% más
            yield return FadeAudioVolume(rainAudioSource, rainAudioSource.volume, targetVolume, 2f);
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
            
        if (screamsAudioSource != null && screamsAudioSource.isPlaying)
            screamsAudioSource.Stop();
    }
}
