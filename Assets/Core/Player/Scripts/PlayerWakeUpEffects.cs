using System.Collections;
using UnityEngine;

/// <summary>
/// Añade efectos adicionales al despertar del protagonista como movimiento de respiración,
/// efectos de visión borrosa, etc.
/// </summary>
public class PlayerWakeUpEffects : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerController playerController;
    
    [Header("Efectos de Respiración")]
    [SerializeField] private bool enableBreathingEffect = true;
    [SerializeField] private float breathingDuration = 5f;
    [SerializeField] private float breathingIntensity = 0.02f;
    [SerializeField] private float breathingFrequency = 1.2f;
    
    [Header("Efectos de Visión")]
    [SerializeField] private bool enableBlurEffect = false; // Requiere Post-Processing
    [SerializeField] private float blurDuration = 3f;
    [SerializeField] private AnimationCurve blurCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Efectos de FOV")]
    [SerializeField] private bool enableFOVEffect = true;
    [SerializeField] private float fovChangeDuration = 2f;
    [SerializeField] private float targetFOV = 70f; // FOV más alto para sensación de despertar
    
    private float originalFOV;
    private Vector3 originalCameraPosition;
    private bool isPlayingEffects = false;
    
    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
            
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
            originalCameraPosition = playerCamera.transform.localPosition;
        }
    }
    
    /// <summary>
    /// Inicia todos los efectos de despertar
    /// </summary>
    public void StartWakeUpEffects()
    {
        if (isPlayingEffects) return;
        
        isPlayingEffects = true;
        StartCoroutine(PlayWakeUpEffectsCoroutine());
    }
    
    /// <summary>
    /// Corrutina principal para los efectos de despertar
    /// </summary>
    private IEnumerator PlayWakeUpEffectsCoroutine()
    {
        Debug.Log("[PlayerWakeUpEffects] Iniciando efectos de despertar");
        
        // Iniciar efectos en paralelo
        Coroutine breathingCoroutine = null;
        Coroutine fovCoroutine = null;
        
        if (enableBreathingEffect)
            breathingCoroutine = StartCoroutine(BreathingEffectCoroutine());
            
        if (enableFOVEffect)
            fovCoroutine = StartCoroutine(FOVEffectCoroutine());
        
        // Esperar a que terminen los efectos
        float maxDuration = Mathf.Max(
            enableBreathingEffect ? breathingDuration : 0f,
            enableFOVEffect ? fovChangeDuration : 0f
        );
        
        yield return new WaitForSeconds(maxDuration);
        
        // Asegurar que todo vuelva a la normalidad
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
            playerCamera.transform.localPosition = originalCameraPosition;
        }
        
        isPlayingEffects = false;
        Debug.Log("[PlayerWakeUpEffects] Efectos de despertar completados");
    }
    
    /// <summary>
    /// Efecto de respiración agitada moviendo la cámara sutilmente
    /// </summary>
    private IEnumerator BreathingEffectCoroutine()
    {
        if (playerCamera == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < breathingDuration)
        {
            float normalizedTime = elapsed / breathingDuration;
            float intensity = Mathf.Lerp(breathingIntensity, 0f, normalizedTime);
            
            // Movimiento vertical sutil para simular respiración
            float breathOffset = Mathf.Sin(elapsed * breathingFrequency * 2f * Mathf.PI) * intensity;
            Vector3 newPosition = originalCameraPosition + Vector3.up * breathOffset;
            
            playerCamera.transform.localPosition = newPosition;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Volver a posición original
        playerCamera.transform.localPosition = originalCameraPosition;
    }
    
    /// <summary>
    /// Efecto de cambio de FOV para simular desorientación
    /// </summary>
    private IEnumerator FOVEffectCoroutine()
    {
        if (playerCamera == null) yield break;
        
        float elapsed = 0f;
        float startFOV = playerCamera.fieldOfView;
        
        // Primera fase: ampliar FOV rápidamente
        while (elapsed < fovChangeDuration * 0.3f)
        {
            float t = elapsed / (fovChangeDuration * 0.3f);
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Pausa en FOV amplio
        yield return new WaitForSeconds(fovChangeDuration * 0.4f);
        
        // Segunda fase: volver a FOV normal
        elapsed = 0f;
        while (elapsed < fovChangeDuration * 0.3f)
        {
            float t = elapsed / (fovChangeDuration * 0.3f);
            playerCamera.fieldOfView = Mathf.Lerp(targetFOV, originalFOV, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.fieldOfView = originalFOV;
    }
    
    /// <summary>
    /// Detiene todos los efectos inmediatamente
    /// </summary>
    public void StopAllEffects()
    {
        StopAllCoroutines();
        
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = originalFOV;
            playerCamera.transform.localPosition = originalCameraPosition;
        }
        
        isPlayingEffects = false;
    }
    
    private void OnValidate()
    {
        if (breathingIntensity < 0f) breathingIntensity = 0f;
        if (breathingFrequency < 0.1f) breathingFrequency = 0.1f;
        if (breathingDuration < 0f) breathingDuration = 0f;
        if (fovChangeDuration < 0f) fovChangeDuration = 0f;
    }
}
