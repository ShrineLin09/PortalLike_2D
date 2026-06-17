using NUnit.Framework;
using SidePortal.Configuration;
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

        [Test]
        public void RemapVelocity_ConvertsDownwardEntryThroughFloorToWallExit()
        {
            var result = TeleportResolver.RemapVelocity(Vector2.down * 28f, Vector2.up, Vector2.right, 4f);

            Assert.That(result.x, Is.EqualTo(28f).Within(0.001f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.magnitude, Is.EqualTo(28f).Within(0.001f));
        }

        [Test]
        public void RemapVelocity_DoesNotDowngradeHighSpeedToMinimumExitSpeed()
        {
            var result = TeleportResolver.RemapVelocity(Vector2.down * 28f, Vector2.up, Vector2.right, 4f);

            Assert.That(result.magnitude, Is.EqualTo(28f).Within(0.001f));
        }

        [Test]
        public void ClampExitVelocity_LimitsOverallSpeed()
        {
            var result = TeleportResolver.ClampExitVelocity(Vector2.right * 30f, 12f, 0f);

            Assert.That(result.magnitude, Is.EqualTo(12f).Within(0.001f));
        }

        [Test]
        public void ClampExitVelocity_LimitsDownwardSpeed()
        {
            var result = TeleportResolver.ClampExitVelocity(new Vector2(2f, -18f), 0f, 10f);

            Assert.That(result.x, Is.EqualTo(2f).Within(0.001f));
            Assert.That(result.y, Is.EqualTo(-10f).Within(0.001f));
        }

        [Test]
        public void ClampExitVelocity_UsesPortalMomentumDefaults()
        {
            var momentum = PrototypeTuning.PortalMomentum;
            var result = TeleportResolver.ClampExitVelocityDetailed(new Vector2(24f, -24f), momentum.MaxExitSpeed, momentum.MaxDownwardExitSpeed);

            Assert.That(result.WasClamped, Is.False);
            Assert.That(result.Velocity.magnitude, Is.GreaterThan(30f));
            Assert.That(result.Velocity.y, Is.GreaterThanOrEqualTo(-momentum.MaxDownwardExitSpeed - 0.001f));
        }
    }
}
