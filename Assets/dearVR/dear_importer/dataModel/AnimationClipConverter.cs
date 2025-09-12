using System.Collections.Generic;
using SpatialConnect.dearVRAnimations;
using UnityEngine;

namespace SpatialConnect
{
	public static class AnimationClipConverter
	{
		public static AnimationClip ConvertToAnimationClip(List<PositionAutomation> animationData)
		{
			var clip = new AnimationClip();
			clip.legacy = true;
			
			var keysX = new Keyframe[animationData.Count];
			var keysY = new Keyframe[animationData.Count];
			var keysZ = new Keyframe[animationData.Count];

			for (var i = 0; i < animationData.Count; i++)
			{
				keysX[i] = new Keyframe(animationData[i].Timecode, animationData[i].Position.x);
				keysY[i] = new Keyframe(animationData[i].Timecode, animationData[i].Position.y);
				keysZ[i] = new Keyframe(animationData[i].Timecode, animationData[i].Position.z);
			}
					
			var curveX = new AnimationCurve(keysX);
			var curveY = new AnimationCurve(keysY);
			var curveZ = new AnimationCurve(keysZ);
			clip.SetCurve("", typeof(Transform), "localPosition.x", curveX);
			clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
			clip.SetCurve("", typeof(Transform), "localPosition.z", curveZ);
			return clip;
		}
	}
}
