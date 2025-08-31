using UnityEngine;

using System.IO;
using SpatialConnect.dearVRAnimations;
using UnityEditor;

namespace SpatialConnect
{
    [UnityEditor.AssetImporters.ScriptedImporter(1, "dear")]
    public class ChannelAutomationImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        private const string dearAnimationsPath = "Assets/dearVR/dearVR_Animations/";
        private const string dearPrefabsPath = "Assets/dearVR/dearVR_Prefabs/";

        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            var animationData = JsonUtility.FromJson<SerializableList<PositionAutomation>>(File.ReadAllText(ctx.assetPath));
            var clip = AnimationClipConverter.ConvertToAnimationClip(animationData.List);
            clip.name = Path.GetFileNameWithoutExtension(ctx.assetPath);

            if (!Directory.Exists(dearAnimationsPath))
                Directory.CreateDirectory(dearAnimationsPath);

            AssetDatabase.CreateAsset(clip, dearAnimationsPath + clip.name + "_clip.anim");

            CreateAutomatedSourcePrefab(ctx.assetPath, clip);
        }

        private void CreateAutomatedSourcePrefab(string importedAssetPath, AnimationClip clip)
        {
            if (!IsDearVrPluginPresent())
                return;

            if (!Directory.Exists(dearPrefabsPath))
                Directory.CreateDirectory(dearPrefabsPath);

            var animatedSourcePrefabBuilder = new AnimatedSourcePrefabBuilder();
            animatedSourcePrefabBuilder.SetName(Path.GetFileNameWithoutExtension(importedAssetPath));
            animatedSourcePrefabBuilder.SetDestinationFolderPath(dearPrefabsPath);
            animatedSourcePrefabBuilder.SetAnimationClip(clip);
            animatedSourcePrefabBuilder.SetAudioClip(FindMatchingAudioFile(importedAssetPath));

            animatedSourcePrefabBuilder.Create();
        }

        private bool IsDearVrPluginPresent()
        {
            var foundAssets = AssetDatabase.FindAssets("AudioPluginDearVR");
            return (foundAssets.Length != 0);
        }

        private string FindMatchingAudioFile(string importedAssetPath)
        {
            var assetPathWithoutExtension = importedAssetPath.Substring(0, importedAssetPath.Length - 5);

            var audioFilePath = assetPathWithoutExtension + ".wav";
            if (File.Exists(audioFilePath))
                return audioFilePath;
            return string.Empty;
        }
    }
}
