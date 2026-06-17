using NUnit.Framework;
using SidePortal.Core;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class AimDirectionTests
    {
        [TestCase(1f, 0.2f, 1f, 0f)]
        [TestCase(-1f, 0.2f, -1f, 0f)]
        [TestCase(0.2f, 1f, 0f, 1f)]
        [TestCase(0.2f, -1f, 0f, -1f)]
        public void FromInput_ReturnsOnlyCardinalDirections(float x, float y, float expectedX, float expectedY)
        {
            var result = AimDirection.FromInput(x, y, Vector2.left);

            Assert.That(result, Is.EqualTo(new Vector2(expectedX, expectedY)));
        }

        [Test]
        public void FromInput_UsesCardinalFallback_WhenInputIsEmpty()
        {
            var result = AimDirection.FromInput(0f, 0f, Vector2.up);

            Assert.That(result, Is.EqualTo(Vector2.up));
        }
    }
}
