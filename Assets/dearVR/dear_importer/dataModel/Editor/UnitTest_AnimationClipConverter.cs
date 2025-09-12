using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using SpatialConnect.dearVRAnimations;

namespace SpatialConnect.Tests
{
	public class UnitTest_AnimationClipConverter
	{
		[Test]
		public void UnitTest_ConvertToAnimationClip_linearAutomation()
		{
			const int samples = 77;
			var animationData = new List<PositionAutomation>();
			for (var i = 0; i < samples; i++)
				animationData.Add(new PositionAutomation(i*0.1f, new Vector3(i*0.3f,i*0.7f,i*0.9f)));

			var result = AnimationClipConverter.ConvertToAnimationClip(animationData);

			var gameObject = new GameObject();
			for (var i = 0; i < samples; i++)
			{
				result.SampleAnimation(gameObject, animationData[i].Timecode);
				Assert.AreEqual(gameObject.transform.position, animationData[i].Position);
			}
		}
		
		[Test]
		public void UnitTest_ConvertToAnimationClip_AnimationClip_is_set_legacy()
		{
			const int samples = 77;
			var animationData = new List<PositionAutomation>();
			for (var i = 0; i < samples; i++)
				animationData.Add(new PositionAutomation(i*0.1f, new Vector3(i*0.3f,i*0.7f,i*0.9f)));

			var result = AnimationClipConverter.ConvertToAnimationClip(animationData);
			
			Assert.IsTrue(result.legacy);
		}
	}
}
