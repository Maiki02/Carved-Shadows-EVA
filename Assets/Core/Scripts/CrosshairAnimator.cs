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
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    [Header("Configuración Visual")]
    [SerializeField] private Color normalOuterColor = Color.white;
    [SerializeField] private Color normalInnerColor = Color.white;
    [SerializeField] private Color interactableOuterColor = Color.yellow;
    [SerializeField] private Color interactableInnerColor = Color.yellow;
    
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
        
        // Guardar tamaños originales
        if (outerRect != null) originalOuterSize = outerRect.sizeDelta;
        if (innerRect != null) originalInnerSize = innerRect.sizeDelta;
        
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
        // Cambiar colores a modo interacción
        yield return StartCoroutine(ChangeColors(
            normalOuterColor, normalInnerColor,
            interactableOuterColor, interactableInnerColor,
            0.3f
        ));
        
        // Loop de expansión continua de la imagen interior
        while (isInteracting)
        {
            yield return StartCoroutine(PulseInnerImage());
        }
    }
    
    /// <summary>
    /// Vuelve suavemente al estado normal
    /// </summary>
    private IEnumerator ReturnToNormal()
    {
        // Cambiar colores de vuelta
        yield return StartCoroutine(ChangeColors(
            interactableOuterColor, interactableInnerColor,
            normalOuterColor, normalInnerColor,
            0.2f
        ));
        
        // Restaurar tamaño normal
        SetNormalState();
    }
    
    /// <summary>
    /// Animación de pulso de la imagen interior (expansión y contracción)
    /// </summary>
    private IEnumerator PulseInnerImage()
    {
        if (innerRect == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration && isInteracting)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Usar la curva para el efecto de pulso
            float curveValue = scaleCurve.Evaluate(progress);
            float currentSize = Mathf.Lerp(normalInnerSize, expandedInnerSize, curveValue);
            
            innerRect.sizeDelta = Vector2.one * currentSize;
            
            yield return null;
        }
        
        // Si aún está interactuando, resetear para el siguiente pulso
        if (isInteracting)
        {
            innerRect.sizeDelta = Vector2.one * normalInnerSize;
        }
    }
    
    /// <summary>
    /// Cambia suavemente los colores de ambas imágenes
    /// </summary>
    private IEnumerator ChangeColors(Color fromOuter, Color fromInner, Color toOuter, Color toInner, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            if (outerImage != null)
                outerImage.color = Color.Lerp(fromOuter, toOuter, progress);
            
            if (innerImage != null)
                innerImage.color = Color.Lerp(fromInner, toInner, progress);
            
            yield return null;
        }
        
        if (outerImage != null) outerImage.color = toOuter;
        if (innerImage != null) innerImage.color = toInner;
    }
    
    /// <summary>
    /// Establece el estado visual normal
    /// </summary>
    private void SetNormalState()
    {
        if (outerImage != null) outerImage.color = normalOuterColor;
        if (innerImage != null) innerImage.color = normalInnerColor;
        
        if (outerRect != null) outerRect.sizeDelta = originalOuterSize;
        if (innerRect != null) innerRect.sizeDelta = Vector2.one * normalInnerSize;
    }
}
