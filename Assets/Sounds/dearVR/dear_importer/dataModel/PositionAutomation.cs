using System;
using UnityEngine;

namespace SpatialConnect.dearVRAnimations
{
    [Serializable]
    public class PositionAutomation
    {
        [SerializeField] private float timecode_;
        [SerializeField] private Vector3 position_;

        public float Timecode
        {
            get { return timecode_; }
        }
        public Vector3 Position
        {
            get { return position_; }
        }
        public PositionAutomation(float timecode, Vector3 position)
        {
            this.timecode_ = timecode;
            this.position_ = position;
        }
    }
}
