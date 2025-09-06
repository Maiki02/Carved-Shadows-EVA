using System.Collections;
using UnityEngine;

public class Candle : MonoBehaviour
{
    [Header("Configuración de Vela")]
    [SerializeField] private CandleType candleType = CandleType.Normal;
    [SerializeField] private Light candleLight;
    
    [Header("Configuración de Intensidad")]
    [SerializeField] private float targetIntensity = 1f; // Intensidad objetivo para encendido
    [SerializeField] private bool useOriginalIntensity = true; // Si usar intensidad original o la configurada
    
    [Header("Configuración de Transiciones")]
    [SerializeField] private float transitionDuration = 2f; // Tiempo para transiciones de encendido/apagado
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Configuración de Variación de Intensidad")]
    [SerializeField] private float baseIntensity = 1f; // Intensidad base inicial
    [SerializeField] private float minIntensity = 0.3f; // Intensidad mínima
    [SerializeField] private float maxIntensity = 1.5f; // Intensidad máxima
    [SerializeField] private float minRandomOffset = 0f; // Offset aleatorio mínimo
    [SerializeField] private float maxRandomOffset = 0.2f; // Offset aleatorio máximo
    [SerializeField] private float variationCycleDuration = 3f; // Duración base de un ciclo completo de variación
    [SerializeField] private float cycleDurationRandomRange = 0.5f; // Rango aleatorio para la duración del ciclo (+/- segundos)
    
    [Header("Configuración de Intermitencia")]
    [SerializeField] private float offDuration = 2f; // Tiempo que permanece apagada (segundos)
    [SerializeField] private float onDuration = 3f; // Tiempo que permanece encendida (segundos)
    [SerializeField] private int intermittentCycles = 1; // Cantidad de ciclos de intermitencia (por defecto 1)
    
    private float originalIntensity;
    private float storedIntensity; // Intensidad almacenada antes del apagado
    private bool isLit = true;
    private bool isChangingState = false;
    private bool isVariationActive = false; // Control de variación de intensidad
    private float variationTimer = 0f; // Timer para el ciclo de variación
    private float currentRandomOffset = 0f; // Offset aleatorio actual
    private float currentCycleDuration = 0f; // Duración actual del ciclo (con random aplicado)
    private bool isIncreasing = true; // true = incrementando hacia máximo, false = decrementando hacia mínimo
    
    // Variables para intermitencia
    private bool isIntermittentActive = false;
    private int currentCycleCount = 0;
    private float intermittentTimer = 0f;
    private bool isInOffPhase = false; // true = apagada, false = encendida

    private void Awake()
    {
        // Buscar el componente Light si no está asignado
        if (candleLight == null)
            candleLight = GetComponentInChildren<Light>();
        
        if (candleLight == null)
        {
            Debug.LogError($"No se encontró componente Light en {gameObject.name} o sus hijos");
            return;
        }
        
        // Guardar la intensidad original para restauración futura
        originalIntensity = candleLight.intensity;
        storedIntensity = originalIntensity; // Inicializar con la intensidad original
        
        // Configurar intensidad objetivo si no se usa la original
        if (!useOriginalIntensity)
        {
            candleLight.intensity = targetIntensity;
            storedIntensity = targetIntensity;
        }
    }

    private void Start()
    {
        // Ejecutar comportamiento específico según el tipo de vela configurado
        ExecuteCandleBehavior();
    }

    private void Update()
    {
        // Ejecutar variación de intensidad si está activa
        if (isVariationActive && isLit && !isChangingState)
        {
            UpdateIntensityVariation();
        }
        
        // Ejecutar lógica de intermitencia si está activa
        if (isIntermittentActive && !isChangingState)
        {
            UpdateIntermittentBehavior();
        }
    }

    private void UpdateIntensityVariation()
    {
        // Incrementar timer de variación
        variationTimer += Time.deltaTime;
        
        // Verificar si completamos el ciclo actual
        if (variationTimer >= currentCycleDuration)
        {
            // Reiniciar timer y generar nueva duración aleatoria
            variationTimer = 0f;
            GenerateNewCycleDuration();
            
            // Cambiar dirección (ida y vuelta)
            isIncreasing = !isIncreasing;
        }
        
        // Calcular progreso del ciclo actual (0 a 1)
        float cycleProgress = variationTimer / currentCycleDuration;
        
        // Determinar intensidad objetivo según la dirección
        float targetIntensity = isIncreasing ? maxIntensity : minIntensity;
        float startIntensity = isIncreasing ? minIntensity : maxIntensity;
        
        // Interpolar linealmente entre intensidades (sin curva)
        float baseVariation = Mathf.Lerp(startIntensity, targetIntensity, cycleProgress);
        
        // Agregar offset aleatorio si está configurado
        if (maxRandomOffset > 0f)
        {
            // Cambiar offset aleatorio cada cierto tiempo para evitar cambios bruscos
            if (variationTimer % 0.1f < Time.deltaTime) // Cambiar cada 0.1 segundos
            {
                currentRandomOffset = Random.Range(minRandomOffset, maxRandomOffset);
                // Aplicar signo aleatorio al offset
                currentRandomOffset *= Random.Range(0, 2) == 0 ? -1f : 1f;
            }
            
            baseVariation += currentRandomOffset;
        }
        
        // Asegurar que la intensidad no salga de los límites absolutos
        float finalIntensity = Mathf.Clamp(baseVariation, 0f, maxIntensity + maxRandomOffset);
        
        // Aplicar la intensidad calculada
        candleLight.intensity = finalIntensity;
    }

    private void GenerateNewCycleDuration()
    {
        // Generar duración aleatoria dentro del rango especificado
        float randomOffset = Random.Range(-cycleDurationRandomRange, cycleDurationRandomRange);
        currentCycleDuration = Mathf.Max(0.1f, variationCycleDuration + randomOffset);
    }

    private void UpdateIntermittentBehavior()
    {
        intermittentTimer += Time.deltaTime;
        
        // Verificar si hemos completado todos los ciclos
        if (currentCycleCount >= intermittentCycles)
        {
            StopIntermittentBehavior();
            return;
        }
        
        if (isInOffPhase)
        {
            // Estamos en fase de apagado
            if (intermittentTimer >= offDuration)
            {
                // Cambiar a fase de encendido
                TurnOnImmediate();
                isInOffPhase = false;
                intermittentTimer = 0f;
                
                Debug.Log($"Vela {gameObject.name} - Ciclo {currentCycleCount + 1}/{intermittentCycles}: Encendida");
            }
        }
        else
        {
            // Estamos en fase de encendido
            if (intermittentTimer >= onDuration)
            {
                // Cambiar a fase de apagado
                TurnOffImmediate();
                isInOffPhase = true;
                intermittentTimer = 0f;
                currentCycleCount++;
                
                Debug.Log($"Vela {gameObject.name} - Ciclo {currentCycleCount}/{intermittentCycles}: Apagada");
            }
        }
    }

    private void ExecuteCandleBehavior()
    {
        switch (candleType)
        {
            case CandleType.Normal:
                // Vela normal, mantiene comportamiento estándar sin modificaciones
                StopIntensityVariation(baseIntensity);
                StopIntermittentBehavior();
                break;
                
            case CandleType.TurnOffPermanent:
                StopIntensityVariation();
                StopIntermittentBehavior();
                TurnOffPermanent();
                break;
                
            case CandleType.TurnOn:
                // TODO: Implementar funcionalidad de encendido gradual
                StopIntensityVariation();
                StopIntermittentBehavior();
                break;
                
            case CandleType.Flickering:
                // Activar variación continua de intensidad
                StopIntermittentBehavior();
                StartIntensityVariation();
                break;
                
            case CandleType.Intermittent:
                // Activar comportamiento intermitente
                StopIntensityVariation();
                StartIntermittentBehavior();
                break;
        }
    }

    /// Apaga la vela gradualmente y la mantiene apagada
    public void TurnOffPermanent()
    {
        if (!isLit || isChangingState) return;
        
        // Almacenar la intensidad actual antes de apagar
        storedIntensity = candleLight.intensity;
        StartCoroutine(TurnOffCoroutine());
    }

    /// Enciende la vela gradualmente
    public void TurnOnGradual()
    {
        if (isLit || isChangingState) return;
        
        StartCoroutine(TurnOnCoroutine());
    }

    private IEnumerator TurnOffCoroutine()
    {
        isChangingState = true;
        float elapsedTime = 0f;
        float startIntensity = candleLight.intensity;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // Aplicar curva de animación para transición suave del apagado
            float curveValue = transitionCurve.Evaluate(progress);
            candleLight.intensity = startIntensity * curveValue;
            
            yield return null;
        }

        // Finalizar apagado asegurando estado completamente inactivo
        candleLight.intensity = 0f;
        candleLight.enabled = false;
        isLit = false;
        isChangingState = false;

        Debug.Log($"Vela {gameObject.name} se ha apagado permanentemente");
    }

    private IEnumerator TurnOnCoroutine()
    {
        isChangingState = true;
        candleLight.enabled = true;
        float elapsedTime = 0f;
        float targetIntensityValue = storedIntensity; // Usar intensidad almacenada

        // Empezar desde intensidad 0
        candleLight.intensity = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // Invertir la curva para encendido (1 - curveValue para invertir la lógica)
            float curveValue = 1f - transitionCurve.Evaluate(1f - progress);
            candleLight.intensity = targetIntensityValue * curveValue;
            
            yield return null;
        }

        // Finalizar encendido asegurando intensidad objetivo
        candleLight.intensity = targetIntensityValue;
        isLit = true;
        isChangingState = false;

        Debug.Log($"Vela {gameObject.name} se ha encendido gradualmente");
    }

    /// Fuerza el apagado inmediato de la vela
    public void TurnOffImmediate()
    {
        if (candleLight != null)
        {
            // Almacenar intensidad actual antes de apagar
            storedIntensity = candleLight.intensity;
            candleLight.intensity = 0f;
            candleLight.enabled = false;
            isLit = false;
        }
    }

    /// Enciende la vela inmediatamente con la intensidad almacenada
    public void TurnOnImmediate()
    {
        if (candleLight != null)
        {
            candleLight.enabled = true;
            candleLight.intensity = storedIntensity; // Usar intensidad almacenada
            isLit = true;
        }
    }

    /// Enciende la vela con una intensidad específica
    public void TurnOnWithIntensity(float intensity)
    {
        if (candleLight != null)
        {
            candleLight.enabled = true;
            candleLight.intensity = intensity;
            storedIntensity = intensity; // Actualizar intensidad almacenada
            isLit = true;
        }
    }

    /// Establece la intensidad objetivo para futuros encendidos
    public void SetTargetIntensity(float intensity)
    {
        targetIntensity = Mathf.Max(0f, intensity);
        if (isLit && !useOriginalIntensity)
        {
            candleLight.intensity = targetIntensity;
            storedIntensity = targetIntensity;
        }
    }

    /// Inicia la variación continua de intensidad
    public void StartIntensityVariation()
    {
        isVariationActive = true;
        variationTimer = 0f;
        currentRandomOffset = 0f;
        isIncreasing = true; // Empezar incrementando hacia el máximo
        GenerateNewCycleDuration(); // Generar primera duración aleatoria
    }

    /// Detiene la variación de intensidad y establece una intensidad fija
    public void     StopIntensityVariation(float? fixedIntensity = null)
    {
        isVariationActive = false;
        if (fixedIntensity.HasValue && candleLight != null)
        {
            candleLight.intensity = fixedIntensity.Value;
        }
        else if (candleLight != null)
        {
            candleLight.intensity = baseIntensity;
        }
    }

    /// Configura los parámetros de variación en tiempo de ejecución
    public void ConfigureVariation(float newMinIntensity, float newMaxIntensity, float newCycleDuration, float newRandomRange = 0f)
    {
        minIntensity = Mathf.Max(0f, newMinIntensity);
        maxIntensity = Mathf.Max(minIntensity, newMaxIntensity);
        variationCycleDuration = Mathf.Max(0.1f, newCycleDuration);
        cycleDurationRandomRange = Mathf.Max(0f, newRandomRange);
        
        // Si está activa la variación, regenerar duración
        if (isVariationActive)
        {
            GenerateNewCycleDuration();
        }
    }

    /// Inicia el comportamiento intermitente
    public void StartIntermittentBehavior()
    {
        isIntermittentActive = true;
        currentCycleCount = 0;
        intermittentTimer = 0f;
        isInOffPhase = false; // Empezar en fase encendida
        
        Debug.Log($"Vela {gameObject.name} - Iniciando intermitencia: {intermittentCycles} ciclos");
    }

    /// Detiene el comportamiento intermitente
    public void StopIntermittentBehavior()
    {
        isIntermittentActive = false;
        currentCycleCount = 0;
        intermittentTimer = 0f;
        isInOffPhase = false;
        
        // Asegurar que la vela quede encendida al finalizar
        if (!isLit)
        {
            TurnOnImmediate();
        }
        
        Debug.Log($"Vela {gameObject.name} - Intermitencia completada");
    }

    /// Configura los parámetros de intermitencia en tiempo de ejecución
    public void ConfigureIntermittent(float newOffDuration, float newOnDuration, int newCycles)
    {
        offDuration = Mathf.Max(0.1f, newOffDuration);
        onDuration = Mathf.Max(0.1f, newOnDuration);
        intermittentCycles = Mathf.Max(1, newCycles);
    }

    // Propiedades públicas de solo lectura para consulta del estado
    public bool IsLit => isLit;
    public bool IsChangingState => isChangingState;
    public bool IsVariationActive => isVariationActive;
    public bool IsIntermittentActive => isIntermittentActive;
    public bool IsIncreasingIntensity => isIncreasing; // Nueva propiedad para consultar dirección
    public CandleType Type => candleType;
    public float StoredIntensity => storedIntensity;
    public float TargetIntensity => targetIntensity;
    public float OriginalIntensity => originalIntensity;
    public float BaseIntensity => baseIntensity;
    public float CurrentIntensity => candleLight != null ? candleLight.intensity : 0f;
    public int CurrentCycleCount => currentCycleCount;
    public int TotalCycles => intermittentCycles;
    public bool IsInOffPhase => isInOffPhase;

    // Cambio dinámico de tipo de vela durante ejecución
    public void SetCandleType(CandleType newType)
    {
        candleType = newType;
        ExecuteCandleBehavior();
    }

    private void OnValidate()
    {
        // Validación en editor: asegurar duración mínima válida
        if (transitionDuration <= 0f)
            transitionDuration = 0.1f;
            
        // Validación de intensidad objetivo
        if (targetIntensity < 0f)
            targetIntensity = 0f;
            
        // Validaciones de variación de intensidad
        if (baseIntensity < 0f)
            baseIntensity = 0f;
            
        if (minIntensity < 0f)
            minIntensity = 0f;
            
        if (maxIntensity < minIntensity)
            maxIntensity = minIntensity + 0.1f;
            
        if (minRandomOffset < 0f)
            minRandomOffset = 0f;
            
        if (maxRandomOffset < minRandomOffset)
            maxRandomOffset = minRandomOffset;
            
        if (variationCycleDuration <= 0f)
            variationCycleDuration = 0.1f;
            
        if (cycleDurationRandomRange < 0f)
            cycleDurationRandomRange = 0f;
            
        // Validaciones de intermitencia
        if (offDuration <= 0f)
            offDuration = 0.1f;
            
        if (onDuration <= 0f)
            onDuration = 0.1f;
            
        if (intermittentCycles <= 0)
            intermittentCycles = 1;
    }
}
