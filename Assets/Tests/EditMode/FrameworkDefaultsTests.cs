using NUnit.Framework;
using SidePortal.Configuration;
using SidePortal.Level;
using SidePortal.Player;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class FrameworkDefaultsTests
    {
        [Test]
        public void PlayerPhysicsDefaults_MatchPrototypeBaseline()
        {
            var physics = PrototypeTuning.PlayerPhysics;

            Assert.That(physics.MoveSpeed, Is.EqualTo(8f));
            Assert.That(physics.JumpImpulse, Is.EqualTo(11.5f));
            Assert.That(physics.RisingGravityScale, Is.EqualTo(2.8f));
            Assert.That(physics.FallingGravityScale, Is.EqualTo(5.2f));
            Assert.That(physics.MaxFallSpeed, Is.EqualTo(18f));
            Assert.That(physics.JumpBufferTime, Is.EqualTo(0.12f));
            Assert.That(physics.CoyoteTime, Is.EqualTo(0.1f));
            Assert.That(physics.JumpCutMultiplier, Is.EqualTo(0.45f));
            Assert.That(physics.GroundCheckSize, Is.EqualTo(new Vector2(0.7f, 0.08f)));
        }

        [Test]
        public void PortalMomentumDefaults_MatchPrototypeBaseline()
        {
            var momentum = PrototypeTuning.PortalMomentum;

            Assert.That(momentum.ExitOffset, Is.EqualTo(1.1f));
            Assert.That(momentum.TeleportCooldown, Is.EqualTo(0.22f));
            Assert.That(momentum.MinExitSpeed, Is.EqualTo(4f));
            Assert.That(momentum.ExitClearancePadding, Is.EqualTo(0.2f));
            Assert.That(momentum.MaxExitSpeed, Is.EqualTo(36f));
            Assert.That(momentum.MaxDownwardExitSpeed, Is.EqualTo(36f));
        }

        [Test]
        public void CameraDefaults_MatchReadablePuzzleView()
        {
            var cameraView = PrototypeTuning.CameraView;

            Assert.That(cameraView.OrthographicSize, Is.EqualTo(4.15f));
            Assert.That(cameraView.FollowSmoothTime, Is.EqualTo(0.08f));
            Assert.That(cameraView.FollowOffset, Is.EqualTo(new Vector3(0f, 1.5f, -10f)));
            Assert.That(cameraView.ReferenceWidth, Is.EqualTo(1280));
            Assert.That(cameraView.ReferenceHeight, Is.EqualTo(720));
        }

        [Test]
        public void LevelDesignScaleDefaults_ProvideProductionRulers()
        {
            var scale = PrototypeTuning.LevelDesignScale;

            Assert.That(scale.CellSize, Is.EqualTo(1f));
            Assert.That(scale.PlayerHeight, Is.EqualTo(1.55f));
            Assert.That(scale.PlayerWidth, Is.EqualTo(0.85f));
            Assert.That(scale.StandardJumpHeight, Is.EqualTo(2.4f));
            Assert.That(scale.StandardJumpDistance, Is.EqualTo(4.5f));
            Assert.That(scale.MaximumSafeDrop, Is.EqualTo(6f));
            Assert.That(scale.BaselineMomentumGap, Is.EqualTo(8f));
        }

        [Test]
        public void Components_StartFromUnifiedDefaults()
        {
            var playerObject = new GameObject("PlayerDefaults");
            var portalObject = new GameObject("PortalDefaults");
            var cameraObject = new GameObject("CameraDefaults");

            try
            {
                playerObject.AddComponent<Rigidbody2D>();
                var player = playerObject.AddComponent<PlayerController>();
                portalObject.AddComponent<BoxCollider2D>();
                var portal = portalObject.AddComponent<Portal>();
                var follow = cameraObject.AddComponent<SimpleCameraFollow>();

                Assert.That(player.Physics.MoveSpeed, Is.EqualTo(PrototypeTuning.PlayerPhysics.MoveSpeed));
                Assert.That(portal.Momentum.MaxDownwardExitSpeed, Is.EqualTo(PrototypeTuning.PortalMomentum.MaxDownwardExitSpeed));
                Assert.That(follow.View.OrthographicSize, Is.EqualTo(PrototypeTuning.CameraView.OrthographicSize));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(portalObject);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
