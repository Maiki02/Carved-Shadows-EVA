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
    [SerializeField] private float transitionDuration = 0.3f; // Duración de la transición de aparición/desaparición
    
    // Código no utilizado - configuración de tamaños comentado
    // [SerializeField] private float normalInnerSize = 4f;     // Tamaño normal de la imagen interior
    // [SerializeField] private float expandedInnerSize = 4f;  // Tamaño expandido de la imagen interior
    // [SerializeField] private float normalOuterSize = 8f;     // Tamaño normal de la imagen exterior
    // [SerializeField] private float expandedOuterSize = 6f;  // Tamaño expandido de la imagen exterior
    // [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    // [Header("Configuración Visual")]
    // [SerializeField] private Color interactableOuterColor = Color.yellow; // Color exterior cuando hay interacción
    // [SerializeField] private Color interactableInnerColor = Color.yellow; // Color interior cuando hay interacción
    // [SerializeField] private float defaultInnerTransparency = 0.0f; // Transparencia por defecto del interior
    
    // Colores auto-detectados de las imágenes
    private Color originalOuterColor;
    private Color originalInnerColor;
    
    private RectTransform outerRect;
    private RectTransform innerRect;
    private bool isInteracting = false;
    private Coroutine animationCoroutine;
    
    // Referencia al PlayerInteractor para validar consistencia
    private PlayerInteractor playerInteractor;
    
    // Código no utilizado - tamaños comentados
    // private Vector2 originalOuterSize;
    // private Vector2 originalInnerSize;
    
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
        
        // Buscar referencia al PlayerInteractor en la escena para validación de consistencia
        playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        if (playerInteractor == null)
        {
            Debug.LogWarning("CrosshairAnimator: No se encontró PlayerInteractor en la escena. La validación de consistencia no funcionará.");
        }
        
        // Código no utilizado - guardado de tamaños comentado
        // if (outerRect != null) 
        // {
        //     originalOuterSize = outerRect.sizeDelta;
        //     if (originalOuterSize.magnitude < 1f)
        //         originalOuterSize = Vector2.one * normalOuterSize;
        // }
        // if (innerRect != null) 
        // {
        //     originalInnerSize = innerRect.sizeDelta;
        //     if (originalInnerSize.magnitude < 1f)  
        //         originalInnerSize = Vector2.one * normalInnerSize;
        // }
        
        // Estado inicial
        SetNormalState();
    }
    
    void Update()
    {
        // Validar consistencia entre el estado del crosshair y el PlayerInteractor
        // Solo verificamos cada 30 frames (~0.5s a 60fps) para optimizar performance
        if (Time.frameCount % 30 == 0)
        {
            ValidateConsistency();
        }
    }
    
    /// <summary>
    /// Valida que el estado del crosshair sea consistente con el estado del PlayerInteractor
    /// Auto-corrige cualquier inconsistencia detectada para evitar bugs de sincronización
    /// </summary>
    private void ValidateConsistency()
    {
        // Verificar si tenemos referencia válida al PlayerInteractor
        if (playerInteractor == null) return;
        
        // Verificar si debería estar visible según el PlayerInteractor
        // HasCurrentInteractable se actualiza automáticamente cuando currentInteractable cambia
        bool shouldBeVisible = playerInteractor.HasCurrentInteractable;
        
        // Detectar inconsistencia: mira activa pero no hay objeto interactuable
        if (isInteracting && !shouldBeVisible)
        {
            // BUG DETECTADO: Crosshair activo sin objeto interactuable
            // Forzar desactivación para corregir el estado
            StopInteractionAnimation();
        }
        // Detectar inconsistencia: mira inactiva pero hay objeto interactuable
        else if (!isInteracting && shouldBeVisible)
        {
            // BUG DETECTADO: Hay objeto interactuable pero crosshair inactivo
            // Forzar activación para corregir el estado
            StartInteractionAnimation();
        }
        // Si ambos estados coinciden, no hay inconsistencia (comportamiento normal)
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
        // Transición suave para mostrar las imágenes con sus colores originales
        yield return StartCoroutine(TransitionToInteractive());
        
        // Mantener visible mientras hay interacción (sin pulsos ni animaciones adicionales)
        while (isInteracting)
        {
            yield return null;
        }
    }
    
    /// <summary>
    /// Vuelve suavemente al estado normal
    /// </summary>
    private IEnumerator ReturnToNormal()
    {
        // Transición suave de vuelta al estado transparente
        yield return StartCoroutine(TransitionToNormal());
    }

    
    /// <summary>
    /// Transición suave al estado interactivo (solo colores)
    /// </summary>
    private IEnumerator TransitionToInteractive()
    {
        float elapsedTime = 0f;
        
        // Colores iniciales transparentes
        Color initialOuterColor = new Color(originalOuterColor.r, originalOuterColor.g, originalOuterColor.b, 0f);
        Color initialInnerColor = new Color(originalInnerColor.r, originalInnerColor.g, originalInnerColor.b, 0f);
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // Transición de colores desde transparente a los colores originales
            if (outerImage != null)
                outerImage.color = Color.Lerp(initialOuterColor, originalOuterColor, progress);
            
            if (innerImage != null)
                innerImage.color = Color.Lerp(initialInnerColor, originalInnerColor, progress);
            
            yield return null;
        }
        
        // Asegurar valores finales exactos
        if (outerImage != null) outerImage.color = originalOuterColor;
        if (innerImage != null) innerImage.color = originalInnerColor;
    }
    
    /// <summary>
    /// Transición suave de vuelta al estado normal (solo colores)
    /// </summary>
    private IEnumerator TransitionToNormal()
    {
        float elapsedTime = 0f;
        
        // Colores finales transparentes
        Color finalOuterColor = new Color(originalOuterColor.r, originalOuterColor.g, originalOuterColor.b, 0f);
        Color finalInnerColor = new Color(originalInnerColor.r, originalInnerColor.g, originalInnerColor.b, 0f);
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // Transición de colores hacia transparente
            if (outerImage != null)
                outerImage.color = Color.Lerp(originalOuterColor, finalOuterColor, progress);
            
            if (innerImage != null)
                innerImage.color = Color.Lerp(originalInnerColor, finalInnerColor, progress);
            
            yield return null;
        }
        
        // Restaurar estado normal exacto
        SetNormalState();
    }
    
    
    // Código no utilizado - método de pulso comentado
    /*
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
    */
    
    /// <summary>
    /// Establece el estado visual normal usando los colores auto-detectados
    /// </summary>
    private void SetNormalState()
    {
        // Hacer las imágenes completamente transparentes cuando no hay interacción
        if (outerImage != null) outerImage.color = new Color(originalOuterColor.r, originalOuterColor.g, originalOuterColor.b, 0f);
        if (innerImage != null) innerImage.color = new Color(originalInnerColor.r, originalInnerColor.g, originalInnerColor.b, 0f);
        
        // Código no utilizado - restauración de tamaños comentado
        // if (outerRect != null) outerRect.sizeDelta = originalOuterSize;
        // if (innerRect != null) innerRect.sizeDelta = Vector2.one * normalInnerSize;
    }
}
