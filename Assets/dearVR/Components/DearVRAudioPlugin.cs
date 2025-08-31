using System.Runtime.InteropServices;

namespace DearVR
{

    public class DearVRAudioPlugin
    {

#if !UNITY_IOS
        private const string entryPoint = "AudioPluginDearVR";
#else
        private const string entryPoint = "__Internal";
#endif


        [DllImport(entryPoint)]
        private static extern short SetRoomDistances(float up, float down, float front, float back, float left, float right);

        [DllImport(entryPoint)]
        private static extern short SetExternalRoomGeo(bool isOn);

        [DllImport(entryPoint)]
        private static extern bool SetLoudspeakerMode(bool isOn);

        [DllImport(entryPoint)]
        private static extern int GetErrorCode();

        [DllImport(entryPoint)]
        private static extern bool SetLoudspeakerModeReverb(bool isOn);

        [DllImport(entryPoint)]
        private static extern bool SetIRDataPath(string irDataPath);
 

        public short DearVRSetRoomDistances(float up, float down, float front, float back, float left, float right)
        {
            return SetRoomDistances(up, down, front, back, left, right);
        }

        public short DearVRSetExternalRoomGeo(bool isOn)
        {
            return SetExternalRoomGeo(isOn);
        }

        public bool DearVRSetLoudspeakerMode(bool isOn)
        {
            return SetLoudspeakerMode(isOn);
        }

        public int DearVRGetErrorCode()
        {
            return GetErrorCode();
        }

        public bool DearVRSetLoudspeakerModeReverb(bool isOn)
        {
            return SetLoudspeakerModeReverb(isOn);
        }

        public bool DearVRSetIRDataPath(string irDataPath)
        {
            return SetIRDataPath(irDataPath);
        }
    }

}
