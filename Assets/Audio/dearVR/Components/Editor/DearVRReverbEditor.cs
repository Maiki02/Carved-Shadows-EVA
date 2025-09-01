using UnityEditor;
using UnityEngine;

namespace DearVR.Editor
{
    public class DearVRReverbEditor : IAudioEffectPluginGUI
    {
        readonly string[] roomList_;

		public override string Name { get { return  "dearVR Reverb"; } }

		public override string Description { get { return "dearVR Reverb Settings"; } }

		public override string Vendor { get { return "Dear Reality"; } }

        private DearVRVersionInfo versionInfo_;

        public DearVRReverbEditor()
        {
            versionInfo_ = new DearVRVersionInfo();

            roomList_ = System.Enum.GetNames(typeof(DearVRSource.RoomList));
        }

        public override bool OnGUI(IAudioEffectPlugin plugin)
        {
            bool tempBool;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(versionInfo_.DearVRGetVersionString());
            Separator();

            EditorGUILayout.Space();

			float tempFloat;
            plugin.GetFloatParameter("Room Selection", out tempFloat);
            plugin.SetFloatParameter("Room Selection", EditorGUILayout.Popup("Room Preset", (int)tempFloat, roomList_));

            plugin.GetFloatParameter("ReverbID", out tempFloat);
            var tempInt = (int)tempFloat;

            plugin.SetFloatParameter("ReverbID", EditorGUILayout.IntField("Reverb ID [1 - 100]", tempInt));

            EditorGUILayout.LabelField("(Set individual ID for each dearVR Reverb!)");

            if (tempInt == 0)
            {
                EditorGUILayout.HelpBox(
                    "Reverb ID is 0 and needs to be (1 - 100)! Always set different IDs for each dearVR Reverb instance! (Do not put dearVR Reverb on Master-Group)"
                    , MessageType.Warning);
            }

            Separator();

            plugin.GetFloatParameter("Gain", out tempFloat);
            plugin.SetFloatParameter("Gain", EditorGUILayout.Slider("Master Gain [dB]", tempFloat, -96.0f, 24.0f));

            plugin.GetFloatParameter("Reverb", out tempFloat);
            plugin.SetFloatParameter("Reverb", EditorGUILayout.Slider("Reverb Level [dB]", tempFloat, -96.0f, 24.0f));

            plugin.GetFloatParameter("RoomSize", out tempFloat);
            plugin.SetFloatParameter("RoomSize", EditorGUILayout.Slider("Room Size [%]", tempFloat, 50.0f, 100.0f));

            plugin.GetFloatParameter("ReverbLP", out tempFloat);
            plugin.SetFloatParameter("ReverbLP", EditorGUILayout.Slider("Reverb LP [Hz]", tempFloat, 500.0f, 20000.0f));


            Separator();
            EditorGUILayout.LabelField("Stop Processing:");


            plugin.GetFloatParameter("Bypass", out tempFloat);
            tempBool = (tempFloat > 0.0f);
            tempBool = EditorGUILayout.Toggle("Bypass", tempBool);
            plugin.SetFloatParameter("Bypass", (tempBool == false) ? 0.0f : 2.0f);


            plugin.GetFloatParameter("Mute", out tempFloat);
            tempBool = (tempFloat > 0.0f);
            tempBool = EditorGUILayout.Toggle("Mute", tempBool);
            plugin.SetFloatParameter("Mute", (tempBool == false) ? 0.0f : 2.0f);

            return false;
        }

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
    }
}
//#endif