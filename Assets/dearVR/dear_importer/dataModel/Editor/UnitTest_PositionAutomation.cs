using UnityEngine;
using NUnit.Framework;

namespace SpatialConnect.dearVRAnimations.Tests
{
    public class UnitTest_PositionAutomation
    {
        [Test]
        public void UnitTest_Timecode()
        {
            var positionAutomation = new PositionAutomation(77.7f, new Vector3());

            Assert.AreEqual(77.7f, positionAutomation.Timecode);
        }

        [Test]
        public void UnitTest_Position()
        {
            var positionAutomation = new PositionAutomation(77.7f, new Vector3(77.7f, 66.6f, 99.9f));

            Assert.AreEqual(new Vector3(77.7f, 66.6f, 99.9f), positionAutomation.Position);
        }
    }
}