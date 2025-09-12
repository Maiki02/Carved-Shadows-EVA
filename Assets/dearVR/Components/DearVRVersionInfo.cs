using System;
using System.Runtime.InteropServices;


namespace DearVR
{
    public class DearVRVersionInfo
    {
#if !UNITY_IOS
        private const string entryPoint = "AudioPluginDearVR";
#else
        private const string entryPoint = "__Internal";
#endif

        [DllImport(entryPoint)]
        private static extern IntPtr GetVersionInfo();

        public string DearVRGetVersionString()
        {
            var versionInfo = Marshal.PtrToStringAnsi(GetVersionInfo());
            return "dearVR engine " + versionInfo + " by Dear Reality";
        }
    }

}
