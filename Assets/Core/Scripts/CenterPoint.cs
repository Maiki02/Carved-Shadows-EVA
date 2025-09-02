using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Script que anima un crosshair compuesto por 2 imágenes (exterior + interior)
/// Agranda la imagen interior y cambia colores para indicar interacción
/// </summary>
public class CrosshairAnimator : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image outerImage; // Imagen exterior (borde/marco)
    [SerializeField] private Image innerImage; // Imagen interior (centro)
    
    [Header("Configuración de Animación")]
    [SerializeField] private float animationDuration = 0.8f;
    [SerializeField] private float normalInnerSize = 4f;     // Tamaño normal de la imagen interior
    [SerializeField] private float expandedInnerSize = 16f;  // Tamaño expandido de la imagen interior
    [SerializeField] private float normalOuterSize = 8f;     // Tamaño normal de la imagen exterior
    [SerializeField] private float expandedOuterSize = 24f;  // Tamaño expandido de la imagen exterior
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    [Header("Configuración Visual")]
    [SerializeField] private Color interactableOuterColor = Color.yellow; // Color exterior cuando hay interacción
    [SerializeField] private Color interactableInnerColor = Color.yellow; // Color interior cuando hay interacción
    [SerializeField] private float defaultInnerTransparency = 0.0f; // Transparencia por defecto del interior (0 = invisible, 1 = opaco)
    
    // Colores auto-detectados de las imágenes
    private Color originalOuterColor;
    private Color originalInnerColor;
    
    private RectTransform outerRect;
    private RectTransform innerRect;
    private bool isInteracting = false;
    private Coroutine animationCoroutine;
    
    // Tamaños originales para restaurar
    private Vector2 originalOuterSize;
    private Vector2 originalInnerSize;
    
    private static CrosshairAnimator instance;
    public static CrosshairAnimator Instance => instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Auto-encontrar las imágenes si no están asignadas
        if (outerImage == null || innerImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>();
            if (images.Length >= 2)
            {
                outerImage = images[0]; // Primera imagen como exterior
                innerImage = images[1]; // Segunda imagen como interior
            }
        }
        
        // Obtener los RectTransforms
        if (outerImage != null) outerRect = outerImage.GetComponent<RectTransform>();
        if (innerImage != null) innerRect = innerImage.GetComponent<RectTransform>();
        
        // Auto-detectar colores originales de las imágenes
        if (outerImage != null) originalOuterColor = outerImage.color;
        if (innerImage != null) originalInnerColor = innerImage.color;
        
        // Guardar tamaños originales (o usar los configurados si las imágenes son muy pequeñas)
        if (outerRect != null) 
        {
            originalOuterSize = outerRect.sizeDelta;
            // Si el tamaño original es muy pequeño, usar el normalOuterSize
            if (originalOuterSize.magnitude < 1f)
                originalOuterSize = Vector2.one * normalOuterSize;
        }
        if (innerRect != null) 
        {
            originalInnerSize = innerRect.sizeDelta;
            // Si el tamaño original es muy pequeño, usar el normalInnerSize  
            if (originalInnerSize.magnitude < 1f)
                originalInnerSize = Vector2.one * normalInnerSize;
        }
        
        // Estado inicial
        SetNormalState();
    }
    
    /// <summary>
    /// Activa la animación de interacción
    /// </summary>
    public void StartInteractionAnimation()
    {
        if (isInteracting) return;
        
        isInteracting = true;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(InteractionAnimationLoop());
    }
    
    /// <summary>
    /// Para la animación y vuelve al estado normal
    /// </summary>
    public void StopInteractionAnimation()
    {
        if (!isInteracting) return;
        
        isInteracting = false;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(ReturnToNormal());
    }
    
    /// <summary>
    /// Loop de animación continua mientras hay interacción disponible
    /// </summary>
    private IEnumerator InteractionAnimationLoop()
    {
        // Cambiar colores a modo interacción con transparencia por defecto para efecto hueco
        Color hollowColor = new Color(interactableInnerColor.r, interactableInnerColor.g, interactableInnerColor.b, defaultInnerTransparency);
        
        // Transición suave de colores y tamaños al mismo tiempo
        yield return StartCoroutine(TransitionToInteractive(hollowColor));
        
        // Loop de expansión continua de ambas imágenes
        while (isInteracting)
        {
            yield return StartCoroutine(PulseBothImages());
        }
    }
    
    /// <summary>
    /// Vuelve suavemente al estado normal
    /// </summary>
    private IEnumerator ReturnToNormal()
    {
        // Transición suave de vuelta al estado normal
        Color hollowColor = new Color(interactableInnerColor.r, interactableInnerColor.g, interactableInnerColor.b, defaultInnerTransparency);
        
        yield return StartCoroutine(TransitionToNormal(hollowColor));
    }

    
    /// <summary>
    /// Transición suave al estado interactivo (colores + tamaños)
    /// </summary>
    private IEnumerator TransitionToInteractive(Color hollowColor)
    {
        float elapsedTime = 0f;
        float duration = 0.3f;
        
        Vector2 initialOuterSize = outerRect != null ? outerRect.sizeDelta : originalOuterSize;
        Vector2 initialInnerSize = innerRect != null ? innerRect.sizeDelta : originalInnerSize;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Transición de colores
            if (outerImage != null)
                outerImage.color = Color.Lerp(originalOuterColor, interactableOuterColor, progress);
            
            if (innerImage != null)
                innerImage.color = Color.Lerp(originalInnerColor, hollowColor, progress);
            
            // Transición de tamaños
            if (outerRect != null)
            {
                Vector2 targetOuterSize = Vector2.one * expandedOuterSize;
                outerRect.sizeDelta = Vector2.Lerp(initialOuterSize, targetOuterSize, progress);
            }
            
            if (innerRect != null)
            {
                Vector2 targetInnerSize = Vector2.one * expandedInnerSize;
                innerRect.sizeDelta = Vector2.Lerp(initialInnerSize, targetInnerSize, progress);
            }
            
            yield return null;
        }
        
        // Asegurar valores finales exactos
        if (outerImage != null) outerImage.color = interactableOuterColor;
        if (innerImage != null) innerImage.color = hollowColor;
        if (outerRect != null) outerRect.sizeDelta = Vector2.one * expandedOuterSize;
        if (innerRect != null) innerRect.sizeDelta = Vector2.one * expandedInnerSize;
    }
    
    /// <summary>
    /// Transición suave de vuelta al estado normal (colores + tamaños)
    /// </summary>
    private IEnumerator TransitionToNormal(Color hollowColor)
    {
        float elapsedTime = 0f;
        float duration = 0.2f;
        
        Vector2 currentOuterSize = outerRect != null ? outerRect.sizeDelta : Vector2.one * expandedOuterSize;
        Vector2 currentInnerSize = innerRect != null ? innerRect.sizeDelta : Vector2.one * expandedInnerSize;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Transición de colores
            if (outerImage != null)
                outerImage.color = Color.Lerp(interactableOuterColor, originalOuterColor, progress);
            
            if (innerImage != null)
                innerImage.color = Color.Lerp(hollowColor, originalInnerColor, progress);
            
            // Transición de tamaños de vuelta al original
            if (outerRect != null)
                outerRect.sizeDelta = Vector2.Lerp(currentOuterSize, originalOuterSize, progress);
            
            if (innerRect != null)
                innerRect.sizeDelta = Vector2.Lerp(currentInnerSize, originalInnerSize, progress);
            
            yield return null;
        }
        
        // Restaurar estado normal exacto
        SetNormalState();
    }
    
    /// <summary>
    /// Animación de pulso de ambas imágenes (expansión y contracción simultánea)
    /// </summary>
    private IEnumerator PulseBothImages()
    {
        if (innerRect == null && outerRect == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration && isInteracting)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Usar la curva para el efecto de pulso
            float curveValue = scaleCurve.Evaluate(progress);
            
            // Pulsar imagen interior
            if (innerRect != null)
            {
                float currentInnerSize = Mathf.Lerp(expandedInnerSize * 0.9f, expandedInnerSize * 1.1f, curveValue);
                innerRect.sizeDelta = Vector2.one * currentInnerSize;
            }
            
            // Pulsar imagen exterior
            if (outerRect != null)
            {
                float currentOuterSize = Mathf.Lerp(expandedOuterSize * 0.95f, expandedOuterSize * 1.05f, curveValue);
                outerRect.sizeDelta = Vector2.one * currentOuterSize;
            }
            
            yield return null;
        }
        
        // Si aún está interactuando, resetear para el siguiente pulso
        if (isInteracting)
        {
            if (innerRect != null) innerRect.sizeDelta = Vector2.one * expandedInnerSize;
            if (outerRect != null) outerRect.sizeDelta = Vector2.one * expandedOuterSize;
        }
    }
    
    /// <summary>
    /// Establece el estado visual normal usando los colores auto-detectados
    /// </summary>
    private void SetNormalState()
    {
        if (outerImage != null) outerImage.color = originalOuterColor;
        if (innerImage != null) innerImage.color = originalInnerColor;
        
        if (outerRect != null) outerRect.sizeDelta = originalOuterSize;
        if (innerRect != null) innerRect.sizeDelta = Vector2.one * normalInnerSize;
    }
}
