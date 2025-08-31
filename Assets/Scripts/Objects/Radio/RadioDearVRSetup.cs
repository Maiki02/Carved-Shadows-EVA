using UnityEngine;
using DearVR;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Utilidades de editor para configurar automáticamente radios con DearVR
/// </summary>
public static class RadioDearVRSetup
{
    [MenuItem("Radio/Setup Radio with DearVR")]
    public static void SetupRadioWithDearVR()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("[RadioDearVRSetup] Selecciona un GameObject para configurar como Radio");
            return;
        }

        // Agregar componentes necesarios
        AudioSource audioSource = selectedObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = selectedObject.AddComponent<AudioSource>();
            Debug.Log("[RadioDearVRSetup] AudioSource agregado");
        }

        DearVRSource dearVRSource = selectedObject.GetComponent<DearVRSource>();
        if (dearVRSource == null)
        {
            dearVRSource = selectedObject.AddComponent<DearVRSource>();
            Debug.Log("[RadioDearVRSetup] DearVRSource agregado");
        }

        Radio radio = selectedObject.GetComponent<Radio>();
        if (radio == null)
        {
            radio = selectedObject.AddComponent<Radio>();
            Debug.Log("[RadioDearVRSetup] Radio script agregado");
        }

        // Configurar Radio para usar DearVR
        radio.ConfigureForRadio();

        // Configurar AudioSource básico
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // DearVR maneja la espacialización

        Debug.Log($"[RadioDearVRSetup] {selectedObject.name} configurado como Radio con DearVR");
        
        // Marcar como modificado para guardado
        EditorUtility.SetDirty(selectedObject);
    }

    [MenuItem("Radio/Setup Radio with DearVR", true)]
    public static bool SetupRadioWithDearVRValidation()
    {
        return Selection.activeGameObject != null;
    }

    [MenuItem("Radio/Create Radio GameObject")]
    public static void CreateRadioGameObject()
    {
        GameObject radioGO = new GameObject("Radio");
        
        // Agregar componentes
        AudioSource audioSource = radioGO.AddComponent<AudioSource>();
        DearVRSource dearVRSource = radioGO.AddComponent<DearVRSource>();
        Radio radio = radioGO.AddComponent<Radio>();

        // Configurar
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        radio.ConfigureForRadio();

        // Posicionar en la escena
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.MoveToView(radioGO.transform);
        }

        // Seleccionar el nuevo objeto
        Selection.activeGameObject = radioGO;

        Debug.Log("[RadioDearVRSetup] Radio GameObject creado y configurado");
    }
}
#endif

/// <summary>
/// Extensiones para facilitar el trabajo con Radio y DearVR
/// </summary>
public static class RadioExtensions
{
    /// <summary>
    /// Configura rápidamente una radio existente
    /// </summary>
    public static void QuickSetupRadio(this Radio radio)
    {
        radio.ConfigureForRadio();
        Debug.Log($"[RadioExtensions] {radio.name} configurado rápidamente");
    }

    /// <summary>
    /// Verifica si la radio está correctamente configurada
    /// </summary>
    public static bool IsProperlyConfigured(this Radio radio)
    {
        AudioSource audioSource = radio.GetComponent<AudioSource>();
        DearVRSource dearVRSource = radio.GetComponent<DearVRSource>();

        bool isConfigured = audioSource != null && dearVRSource != null;
        
        if (!isConfigured)
        {
            Debug.LogWarning($"[RadioExtensions] {radio.name} no está completamente configurado");
        }

        return isConfigured;
    }
}
