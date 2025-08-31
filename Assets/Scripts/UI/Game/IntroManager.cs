using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    private string sceneGame = "Main"; // Nombre de la escena a cargar
    [SerializeField] private float fadeInDuration = 2f; // Duración del fade in
    [SerializeField] private float fadeOutDuration = 2f; // Duración del
    [SerializeField] private float minIntroDuration = 5f;
    [SerializeField] private Image fadeImage;

    void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        float startTime = Time.time;
        // 1. Cargar escena en segundo plano sin activarla aún
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneGame);
        loadOp.allowSceneActivation = false;

        // 2. Fade In (oscurece)
        yield return FadeManager.Instance.StartCoroutine(
            FadeImage(1f, 0f, fadeInDuration)
        );

        AudioController.Instance.PlaySFX(AudioType.Rain, true);

        // 3. Esperar a que termine de cargar o que pasen al menos los 3 segundos
        while (!loadOp.isDone && loadOp.progress < 0.9f)
        {
            yield return null;
        }

        // 4. Esperar si la intro fue demasiado rápida
        float elapsed = Time.time - startTime;
        if (elapsed < minIntroDuration)
            yield return new WaitForSeconds(minIntroDuration - elapsed);

        // 5. Fade Out (desoscurece)
        yield return FadeManager.Instance.StartCoroutine(
            FadeImage(0f, 1f, fadeOutDuration)
        );

        // 6. Activar la escena cargada
        loadOp.allowSceneActivation = true;
    }
    
    private IEnumerator FadeImage(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.enabled = true;
        Color c = fadeImage.color;
        c.a = fromAlpha;
        fadeImage.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(fromAlpha, toAlpha, t / duration);
            fadeImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, toAlpha);
        if (Mathf.Approximately(toAlpha, 0f))
            fadeImage.enabled = false;
    }
}
