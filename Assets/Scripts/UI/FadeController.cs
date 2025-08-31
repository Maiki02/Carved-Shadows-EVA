using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Referencias UI (Canvas)")]
    [SerializeField] private Image blackImage;     // Imagen full-screen para parpadeos
    [SerializeField] private Image topBar;         // Barra superior para cierre de vista
    [SerializeField] private Image bottomBar;      // Barra inferior para cierre de vista

    [Header("Duraciones")]
    [SerializeField] private float blinkDuration = 1.5f;  // Tiempo de cada fase de parpadeo
    [SerializeField] private float blackImageDuration = 2f;  // Tiempo de espera con la pantalla en negro
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

    private IEnumerator BlinkCoroutine(float fromA, float toA, float duration)
    {

        yield return StartCoroutine(FadeImage(blackImage, fromA, toA, duration));
        //  yield return StartCoroutine(FadeImage(blackImage,   1f, 0f, blinkDuration ));

    }

    private IEnumerator FadeImage(Image img, float fromA, float toA, float dur)
    {
        if (img == null) yield break;

        img.enabled = true;
        Color c = img.color;
        c.a = fromA;
        img.color = c;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(fromA, toA, t / dur);
            img.color = new Color(0, 0, 0, a);
            yield return null;
        }

        // asegurar alpha final
        img.color = new Color(0, 0, 0, toA);
        if (Mathf.Approximately(toA, 0f))
            img.enabled = false;
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
