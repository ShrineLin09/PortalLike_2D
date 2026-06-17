using NUnit.Framework;
using SidePortal.Player;

namespace SidePortal.Tests.EditMode
{
    public sealed class PlayerControllerMomentumTests
    {
        [Test]
        public void ResolveHorizontalVelocity_PreservesPortalMomentumWithoutInput()
        {
            var result = PlayerController.ResolveHorizontalVelocity(28f, 0f, 8f);

            Assert.That(result, Is.EqualTo(28f));
        }

        [Test]
        public void ResolveHorizontalVelocity_PreservesPortalMomentumWithSameDirectionInput()
        {
            var result = PlayerController.ResolveHorizontalVelocity(28f, 1f, 8f);

            Assert.That(result, Is.EqualTo(28f));
        }

        [Test]
        public void ResolveHorizontalVelocity_UsesWalkSpeedWhenNoExternalMomentum()
        {
            var result = PlayerController.ResolveHorizontalVelocity(3f, 1f, 8f);

            Assert.That(result, Is.EqualTo(8f));
        }
    }
}
