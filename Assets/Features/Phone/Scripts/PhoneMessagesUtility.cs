using UnityEngine;
using UnityEditor;

/// <summary>
/// Utilidades de editor para crear y configurar PhoneMessages assets
/// </summary>
public static class PhoneMessagesUtility
{
#if UNITY_EDITOR
    [MenuItem("Phone/Create Default Phone Messages Asset")]
    public static void CreateDefaultPhoneMessagesAsset()
    {
        PhoneMessages asset = ScriptableObject.CreateInstance<PhoneMessages>();
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Crear PhoneMessages Asset",
            "DefaultPhoneMessages",
            "asset",
            "Selecciona donde guardar el asset de PhoneMessages");
            
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            Debug.Log($"[PhoneMessagesUtility] Asset creado en: {path}");
        }
    }
    
    [MenuItem("Phone/Log Current Phone Messages Info")]
    public static void LogPhoneMessagesInfo()
    {
        PhoneMessages selected = Selection.activeObject as PhoneMessages;
        if (selected != null)
        {
            Debug.Log($"=== PHONE MESSAGES INFO: {selected.name} ===");
            Debug.Log($"Primer Loop: {selected.GetMessageCount(PhoneLoopType.PrimerLoop)} mensajes, duración total: {selected.GetTotalDuration(PhoneLoopType.PrimerLoop):F1}s");
            Debug.Log($"Segundo Loop: {selected.GetMessageCount(PhoneLoopType.SegundoLoop)} mensajes, duración total: {selected.GetTotalDuration(PhoneLoopType.SegundoLoop):F1}s");
        }
        else
        {
            Debug.LogWarning("[PhoneMessagesUtility] Selecciona un asset de PhoneMessages para ver su información");
        }
    }
#endif
}
