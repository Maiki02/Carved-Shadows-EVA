using UnityEngine;

namespace DearVR
{
    /// <summary>
    /// Dear VR serialized reverb. a container for reverb sends with indices
    /// </summary>
    [System.Serializable]
    public class DearVRSerializedReverb
    {
        /// <summary>
        /// The index of the reverb room, to send the reverb to.
        /// </summary>
        public int roomIndex;

        /// <summary>
        /// how much of the signal should be sent, to the reverb channel
        /// </summary>
        [Range(-96, 24)] public float send;
    }
}