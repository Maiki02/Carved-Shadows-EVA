using UnityEngine;
using UnityEditor;

namespace SpatialConnect
{
    public class AnimatedSourcePrefabBuilder
    {
        private GameObject gameObject_;
        private string destinationFolderPath_ = string.Empty;
        private string name_ = string.Empty;

        public AnimatedSourcePrefabBuilder()
        {
            gameObject_ = Object.Instantiate(Resources.Load("AnimatedSourceTemplate"), Vector3.zero, Quaternion.identity) as GameObject;
        }

        public void SetAnimationClip(AnimationClip clip)
        {
            clip.legacy = true;

            var animation = gameObject_.GetComponent<Animation>();
            animation.clip = clip;
            animation.enabled = true;
        }

        public void SetAudioClip(string audioClipPath)
        {
            gameObject_.GetComponent<AudioSource>().clip = AssetDatabase.LoadAssetAtPath(audioClipPath, typeof(AudioClip)) as AudioClip;
        }

        public void SetDestinationFolderPath(string destinationFolderPath)
        {
            destinationFolderPath_ = destinationFolderPath;
        }

        public void SetName(string name)
        {
            name_ = name;
        }

        public void Create()
        {
            if (gameObject_ == null)
                return;

            PrefabUtility.CreatePrefab(destinationFolderPath_ + name_ + ".prefab", gameObject_);
            Object.DestroyImmediate(gameObject_);
        }
    }
}
