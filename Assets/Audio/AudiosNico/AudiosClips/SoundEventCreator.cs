using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Audio;

public class SoundEventCreator : MonoBehaviour
{
    [Header("Carpeta con clips")]
    public string clipsFolder = "Assets/Audio/ClipsTemp";  // Ajustá según tu ruta
    [Header("Carpeta para crear SO")]
    public string eventsFolder = "Assets/Audio/EventsTemp";  // Ajustá según tu ruta
    public AudioMixerGroup defaultMixerGroup;
    public float defaultVolume = 1f;
    public float defaultSpatialBlend = 1f; // 3D
    public bool defaultLoop = false;

    [ContextMenu("Generate SoundEvents")]
    public void GenerateSoundEvents()
    {
        // Asegurar que la carpeta de Events existe
        if (!AssetDatabase.IsValidFolder(eventsFolder))
        {
            Directory.CreateDirectory(eventsFolder);
            AssetDatabase.Refresh();
        }

        string[] files = Directory.GetFiles(clipsFolder, "*.wav", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/");
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null) continue;

            // Nombre del SO basado en clip
            string soName = "SE_" + clip.name;
            string soPath = Path.Combine(eventsFolder, soName + ".asset").Replace("\\", "/");

            // Crear SO
            SoundEvent newSO = ScriptableObject.CreateInstance<SoundEvent>();
            newSO.clip = clip;
            newSO.volume = defaultVolume;
            newSO.spatialBlend = defaultSpatialBlend;
            newSO.loop = defaultLoop;
            newSO.mixerGroup = defaultMixerGroup;

            AssetDatabase.CreateAsset(newSO, soPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Se generaron {files.Length} SoundEvent ScriptableObjects.");
    }
}
