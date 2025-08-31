using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DearVR.Editor
{
    [CustomEditor(typeof(DearVRSource))]
    public class DearVRSourceEditor : UnityEditor.Editor
    {
        string[] roomList_;

        private DearVRSource component_;

        private const float GainMin = -96.0f;
        private const float GainMax = 24.0f;
        private const int MaxReverbSend = 100;

        private DearVRVersionInfo versionInfo_;

        void OnEnable()
        {
            component_ = (DearVRSource)target;

            roomList_ = System.Enum.GetNames(typeof(DearVRSource.RoomList));

            versionInfo_ = new DearVRVersionInfo();
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(component_, "DearVRSource");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(versionInfo_.DearVRGetVersionString());
            Separator();

            Label("REVERB:");
            component_.RoomPreset = (DearVRSource.RoomList)EditorGUILayout.Popup("Room Preset", (int)component_.RoomPreset, roomList_);

            component_.InternalReverb =
                EditorGUILayout.ToggleLeft("Internal Reverb (Disables Reverb Bus Send)", component_.InternalReverb);

            GUI.enabled = !component_.InternalReverb;

            serializedObject.Update();
            var reverbSends = serializedObject.FindProperty("reverbSends");

            if (reverbSends.arraySize > MaxReverbSend)
            {
                Debug.LogWarning("Maximum of 100 Reverb sends can be used");
                reverbSends.arraySize = MaxReverbSend;
            }

            EditorGUILayout.PropertyField(reverbSends, true);
            serializedObject.ApplyModifiedProperties();
            GUI.enabled = true;

            GUI.enabled = component_.InternalReverb;
            component_.RoomSize = EditorGUILayout.Slider("Room Size (%)", component_.RoomSize, 50.0f, 100.0f);
            component_.ReverbLP = EditorGUILayout.Slider("Reverb Filter (Hz)", component_.ReverbLP, 500.0f, 20000.0f);
            GUI.enabled = true;

            component_.ReflectionLP =
                EditorGUILayout.Slider("Reflection Filter [Hz]", component_.ReflectionLP, 500.0f, 20000.0f);
            Separator();
            Label("LEVELS:");
            component_.GainLevel = EditorGUILayout.Slider("Master Gain [dB]", component_.GainLevel, GainMin, GainMax);
            component_.DirectLevel =
                EditorGUILayout.Slider("Direct Level [dB]", component_.DirectLevel, GainMin, GainMax);
            component_.ReflectionLevel =
                EditorGUILayout.Slider("Reflection Level [dB]", component_.ReflectionLevel, GainMin, GainMax);
            component_.ReverbLevel =
                EditorGUILayout.Slider("Reverb Level [dB]", component_.ReverbLevel, GainMin, GainMax);


            Separator();
            Label("SETTINGS:");
            component_.Auralization = EditorGUILayout.Toggle("Auralization", component_.Auralization);
            if (component_.Auralization)
            {
                EditorGUILayout.HelpBox(
                    "Auralization is realtime calculation of reflections and only active, if room geometries (Manual or Automatic) are active in DearVRManager.",
                    MessageType.Warning);
            }

            component_.BassBoost = EditorGUILayout.Toggle("Bass Boost", component_.BassBoost);

            component_.UseUnityDistance = EditorGUILayout.Toggle("Unity Distance Graph", component_.UseUnityDistance);
            component_.DistanceCorrection =
                EditorGUILayout.Slider("Distance Correction", component_.DistanceCorrection, 0.01f, 10.0f);
            component_.AzimuthCorrection =
                EditorGUILayout.Slider("Phi Angle Correction", component_.AzimuthCorrection, 0.2f, 4.0f);
            component_.Bypass = EditorGUILayout.Toggle("Bypass Plugin", component_.Bypass);
            Separator();
            component_.OcclusionActive = EditorGUILayout.BeginToggleGroup("OCCLUSION", component_.OcclusionActive);
            component_.occlusionWallMask = LayerMaskField("Occlusion Objects", component_.occlusionWallMask);
            component_.OcclusionLevel =
                EditorGUILayout.Slider("Occlusion (0 disabled)", component_.OcclusionLevel, 0.0f, 1.0f);
            component_.OcclusionRayUpdateFreq = EditorGUILayout.Slider("Occlusion Update Time (s)",
                component_.OcclusionRayUpdateFreq, 0.05f, 5.0f);
            component_.OcclusionDebugRayCast =
                EditorGUILayout.Toggle("Debug Occlusion (Gizmos)", component_.OcclusionDebugRayCast);
            component_.ForceOcclusion = EditorGUILayout.Toggle("Force Occlusion", component_.ForceOcclusion);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();
            Separator();

            component_.ObstructionActive =
                EditorGUILayout.BeginToggleGroup("OBSTRUCTION", component_.ObstructionActive);
            component_.obstructionWallMask = LayerMaskField("Obstruction Objects", component_.obstructionWallMask);
            component_.ObstructionLevel =
                EditorGUILayout.Slider("Obstruction (0 disabled)", component_.ObstructionLevel, 0.0f, 1.0f);
            component_.ObstructionRayUpdateFreq = EditorGUILayout.Slider("Obstruction Update Time (s)",
                component_.ObstructionRayUpdateFreq, 0.05f, 5.0f);
            component_.ObstructionDebugRayCast =
                EditorGUILayout.Toggle("Debug Obstruction (Gizmos)", component_.ObstructionDebugRayCast);
            component_.ForceObstruction = EditorGUILayout.Toggle("Force Obstruction", component_.ForceObstruction);
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Space();
            Separator();

            component_.PerformanceMode =
                EditorGUILayout.BeginToggleGroup("dearVR PLAY PERFORMANCE MODE", component_.PerformanceMode);
            if (component_.PerformanceMode == true)
            {
                EditorGUILayout.HelpBox(
                    "Processing bypassed during idle state. Play Audio Sources only with DearVRPlay(), DearVRPlayOneShot(AudioClip) or DearVRPlayOnAwake flag. WARNING: Do not use the Play() or PlayOnAwake flag in Performace Mode!"
                    , MessageType.Warning);
            }

            component_.DearVRPlayOnAwake = EditorGUILayout.Toggle("DearVRPlayOnAwake", component_.DearVRPlayOnAwake);

            component_.ReverbStopOverlap =
                EditorGUILayout.Slider("Reverb Tail After Stop (s)", component_.ReverbStopOverlap, 0.0f,
                    5.0f); // Process Stop Delay  
            EditorGUILayout.EndToggleGroup();

            IsProcessing(component_.IsProcessing);


            if (GUI.changed)
            {
                EditorUtility.SetDirty(component_);
                if (component_.ReverbSendsChanged())
                {
                    component_.SetReverbSends();
                }
            }
        }

        // Separator
        void Separator()
        {
            GUI.color = new Color(1, 1, 1, 0.25f);
            GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
            GUI.color = Color.white;
        }

        // 
        void IsProcessing(bool processing)
        {
            GUILayout.BeginHorizontal();

            GUI.color = new Color(0, 0, 0, 1f);

            GUILayout.Box("Processing", "label", GUILayout.ExpandWidth(false));

            GUI.color = Color.white;

            if (processing)
                GUI.color = new Color(0, 1, 0, 1f);
            else
                GUI.color = new Color(1, 1, 1, 1f);

            GUILayout.Box("Processing", "horizontalSliderThumb", GUILayout.Height(16));

            GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }

        // Label
        void Label(string label)
        {
            EditorGUILayout.LabelField(label);
        }


        private static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            var layers = new List<string>();
            var layerNumbers = new List<int>();

            for (var i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                if (layerName == "") continue;
                layers.Add(layerName);
                layerNumbers.Add(i);
            }

            var maskWithoutEmpty = 0;
            for (var i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            var mask = layerNumbers.Where((t, i) => (maskWithoutEmpty & (1 << i)) > 0)
                .Aggregate(0, (current, t) => current | (1 << t));

            layerMask.value = mask;
            return layerMask;
        }
    }
}