using NUnit.Framework;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class TeleportResolverTests
    {
        [Test]
        public void RemapVelocity_ConvertsForwardSpeedToExitNormal()
        {
            var result = TeleportResolver.RemapVelocity(Vector2.right * 8f, Vector2.left, Vector2.up, 0f);

            Assert.That(result.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.y, Is.EqualTo(8f).Within(0.001f));
        }

        [Test]
        public void RemapVelocity_AppliesMinimumExitSpeed_WhenVelocityIsTooSmall()
        {
            var result = TeleportResolver.RemapVelocity(Vector2.zero, Vector2.left, Vector2.right, 3f);

            Assert.That(result, Is.EqualTo(Vector2.right * 3f));
        }
    }
}
