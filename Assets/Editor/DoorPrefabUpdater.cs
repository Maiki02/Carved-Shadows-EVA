using UnityEngine;
using UnityEditor;

public class DoorPrefabUpdater : EditorWindow
{
    [Header("Audio Clips to assign")]
    public AudioClip defaultOpenClip;
    public AudioClip defaultCloseClip;
    public AudioClip defaultKnockClip;
    
    [Header("Default Values")]
    public float defaultKnockAmount = 4f;
    public float defaultKnockSpeed = 5f;
    public float defaultSlowCloseDuration = 3f;
    public float defaultSlowCloseSpeed = 0.5f;

    [MenuItem("Tools/Update Door Prefabs")]
    static void ShowWindow()
    {
        GetWindow<DoorPrefabUpdater>("Door Prefab Updater");
    }

    void OnGUI()
    {
        GUILayout.Label("Update Door Prefabs", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        defaultOpenClip = (AudioClip)EditorGUILayout.ObjectField("Default Open Clip", defaultOpenClip, typeof(AudioClip), false);
        defaultCloseClip = (AudioClip)EditorGUILayout.ObjectField("Default Close Clip", defaultCloseClip, typeof(AudioClip), false);
        defaultKnockClip = (AudioClip)EditorGUILayout.ObjectField("Default Knock Clip", defaultKnockClip, typeof(AudioClip), false);
        
        EditorGUILayout.Space();
        
        defaultKnockAmount = EditorGUILayout.FloatField("Default Knock Amount", defaultKnockAmount);
        defaultKnockSpeed = EditorGUILayout.FloatField("Default Knock Speed", defaultKnockSpeed);
        defaultSlowCloseDuration = EditorGUILayout.FloatField("Default Slow Close Duration", defaultSlowCloseDuration);
        defaultSlowCloseSpeed = EditorGUILayout.FloatField("Default Slow Close Speed", defaultSlowCloseSpeed);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Update All Door Prefabs"))
        {
            UpdateDoorPrefabs();
        }
    }

    void UpdateDoorPrefabs()
    {
        // Buscar todos los prefabs con componente Door
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int updatedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                Door doorComponent = prefab.GetComponent<Door>();
                if (doorComponent != null)
                {
                    // Usar SerializedObject para modificar los campos privados
                    SerializedObject serializedDoor = new SerializedObject(doorComponent);
                    
                    // Actualizar clips de audio
                    if (defaultOpenClip != null)
                        serializedDoor.FindProperty("openDoorClip").objectReferenceValue = defaultOpenClip;
                    if (defaultCloseClip != null)
                        serializedDoor.FindProperty("closeDoorClip").objectReferenceValue = defaultCloseClip;
                    if (defaultKnockClip != null)
                        serializedDoor.FindProperty("knockClip").objectReferenceValue = defaultKnockClip;
                    
                    // Actualizar valores numéricos
                    serializedDoor.FindProperty("knockAmount").floatValue = defaultKnockAmount;
                    serializedDoor.FindProperty("knockSpeed").floatValue = defaultKnockSpeed;
                    serializedDoor.FindProperty("slowCloseDuration").floatValue = defaultSlowCloseDuration;
                    serializedDoor.FindProperty("slowCloseSpeed").floatValue = defaultSlowCloseSpeed;
                    
                    // Aplicar los cambios
                    serializedDoor.ApplyModifiedProperties();
                    
                    // Marcar como sucio para guardar
                    EditorUtility.SetDirty(prefab);
                    
                    updatedCount++;
                    Debug.Log($"Updated Door prefab: {path}");
                }
            }
        }
        
        // Guardar los cambios
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Updated {updatedCount} Door prefabs successfully!");
        EditorUtility.DisplayDialog("Update Complete", $"Successfully updated {updatedCount} Door prefabs.", "OK");
    }
}
