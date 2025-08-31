using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace DearVR.Editor
{
    /// <summary>
    /// Editor class for <see cref="DearVRManager"/>
    /// </summary>
    [CustomEditor(typeof(DearVRManager))]
    public class DearVRManagerEditor : UnityEditor.Editor
    {
        private DearVRVersionInfo versionInfo_;
        private DearVRManager manager_;

        void OnEnable()
        {
            manager_ = (DearVRManager) target;
            versionInfo_ = new DearVRVersionInfo();

            if (!Resources.Load<DearVRManagerState>("DearVRManagerState"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/dearVR/Resources"))
                    AssetDatabase.CreateFolder("Assets/dearVR", "Resources");
                AssetDatabase.CreateAsset(DearVRManagerState.Instance, "Assets/dearVR/Resources/DearVRManagerState.asset");
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(DearVRManagerState.Instance, "DearVRManagerState");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(versionInfo_.DearVRGetVersionString());
            Separator();

            Label("GLOBAL SETTINGS:");

            DearVRManagerState.Instance.Bypass3DAudio = EditorGUILayout.Toggle(
                new GUIContent("Loudspeaker Mode", "Bypassed Binaural Processing"), DearVRManagerState.Instance.Bypass3DAudio);
            Separator();
            Label("ROOM GEOMETRY FOR AURALIZATION:");

            DearVRManagerState.Instance.RoomAnalyzer =
                EditorGUILayout.BeginToggleGroup("Automatic Room Analyzer", DearVRManagerState.Instance.RoomAnalyzer);

            DearVRManagerState.Instance.RoomMask = LayerMaskField("Room Boundaries", DearVRManagerState.Instance.RoomMask);

            DearVRManagerState.Instance.RoomUpdateFreq =
                EditorGUILayout.Slider("Analyzer Update Time (s)", DearVRManagerState.Instance.RoomUpdateFreq, 0.1f, 10.0f);

            DearVRManagerState.Instance.DebugRoomAnalyzer =
                EditorGUILayout.Toggle("Debug Room Analyzer (Gizmos)", DearVRManagerState.Instance.DebugRoomAnalyzer);

            EditorGUILayout.EndToggleGroup();

            DearVRManagerState.Instance.SetRoomGeo =
                EditorGUILayout.BeginToggleGroup("Manual Room Geometry", DearVRManagerState.Instance.SetRoomGeo);

            DearVRManagerState.Instance.UpDownGeo = EditorGUILayout.Vector2Field("UP | DOWN (m)", DearVRManagerState.Instance.UpDownGeo);

            DearVRManagerState.Instance.FrontBackGeo = EditorGUILayout.Vector2Field("FRONT | BACK (m)", DearVRManagerState.Instance.FrontBackGeo);

            DearVRManagerState.Instance.LeftRightGeo = EditorGUILayout.Vector2Field("LEFT | RIGHT (m)", DearVRManagerState.Instance.LeftRightGeo);

            EditorGUILayout.EndToggleGroup();

            Label("Listener to wall distance (-1.0 no wall)");
            Label("Geometry only affects sources with active Auralization");

            Separator();

            if (GUI.changed)
                EditorUtility.SetDirty(manager_);
                EditorUtility.SetDirty(DearVRManagerState.Instance);
        }

        // Separator
        void Separator()
        {
            GUI.color = new Color(1, 1, 1, 0.25f);
            GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
            GUI.color = Color.white;
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
