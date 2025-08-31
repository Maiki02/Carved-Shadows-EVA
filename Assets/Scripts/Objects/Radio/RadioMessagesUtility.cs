using UnityEngine;
using UnityEditor;

/// <summary>
/// Utilidades de editor para crear y configurar RadioMessages assets
/// </summary>
public static class RadioMessagesUtility
{
#if UNITY_EDITOR
    [MenuItem("Radio/Create Default Radio Messages Asset")]
    public static void CreateDefaultRadioMessagesAsset()
    {
        RadioMessages asset = ScriptableObject.CreateInstance<RadioMessages>();
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Crear RadioMessages Asset",
            "DefaultRadioMessages",
            "asset",
            "Selecciona donde guardar el asset de RadioMessages");
            
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            
            Debug.Log($"[RadioMessagesUtility] Asset creado en: {path}");
        }
    }
    
    [MenuItem("Radio/Log Current Radio Messages Info")]
    public static void LogRadioMessagesInfo()
    {
        RadioMessages selected = Selection.activeObject as RadioMessages;
        if (selected != null)
        {
            Debug.Log($"=== RADIO MESSAGES INFO: {selected.name} ===");
            Debug.Log($"Primer Loop: {selected.GetMessageCount(RadioLoopType.PrimerLoop)} mensajes, duración total: {selected.GetTotalDuration(RadioLoopType.PrimerLoop):F1}s");
            Debug.Log($"Segundo Loop: {selected.GetMessageCount(RadioLoopType.SegundoLoop)} mensajes, duración total: {selected.GetTotalDuration(RadioLoopType.SegundoLoop):F1}s");
        }
        else
        {
            Debug.LogWarning("[RadioMessagesUtility] Selecciona un asset de RadioMessages para ver su información");
        }
    }
#endif
}
