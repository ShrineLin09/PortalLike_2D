using System.Reflection;
using NUnit.Framework;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class PortalPlacementValidatorTests
    {
        private const int AnchorLayer = 6;
        private const int BlockingLayer = 7;
        private const int PortalLayer = 8;

        private GameObject validatorObject;
        private PortalPlacementValidator validator;

        [SetUp]
        public void SetUp()
        {
            validatorObject = new GameObject("validator");
            validator = validatorObject.AddComponent<PortalPlacementValidator>();

            Physics2D.queriesHitTriggers = true;
            SetMask("portalAnchorMask", 1 << AnchorLayer);
            SetMask("placementBlockingMask", 1 << BlockingLayer);
            SetMask("portalOverlapMask", 1 << PortalLayer);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(validatorObject);
            foreach (var obj in Object.FindObjectsOfType<GameObject>())
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void TryFindPlacement_Fails_WhenRayMissesSurface()
        {
            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right, true);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Failure, Is.EqualTo(PortalPlacementFailure.NoValidAnchorHit));
        }

        [Test]
        public void TryFindPlacement_Accepts_ClearAnchor()
        {
            CreateAnchor("anchor", new Vector2(4f, 0f), Vector2.left, true, true);

            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right, true);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Normal, Is.EqualTo(Vector2.left));
            Assert.That(result.AnchorName, Is.EqualTo("anchor"));
        }

        [Test]
        public void TryFindPlacement_Fails_WhenPortalTypeIsNotAllowed()
        {
            CreateAnchor("yellow only", new Vector2(4f, 0f), Vector2.left, false, true);

            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right, true);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Failure, Is.EqualTo(PortalPlacementFailure.PortalTypeNotAllowed));
        }

        [Test]
        public void TryFindPlacement_Fails_WhenPortalOverlapsExistingPortal()
        {
            CreateAnchor("anchor", new Vector2(4f, 0f), Vector2.left, true, true);
            CreateBox("existing portal", new Vector2(3.92f, 0f), new Vector2(0.18f, 2f), PortalLayer);

            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right, true);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Failure, Is.EqualTo(PortalPlacementFailure.OverlappingPortal));
        }

        private static void CreateBox(string name, Vector2 position, Vector2 size, int layer)
        {
            var obj = new GameObject(name);
            obj.layer = layer;
            obj.transform.position = position;
            var collider = obj.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void CreateAnchor(string name, Vector2 position, Vector2 normal, bool allowPrimary, bool allowSecondary)
        {
            var obj = new GameObject(name);
            obj.layer = AnchorLayer;
            obj.transform.position = position;
            var collider = obj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.45f, 2f);
            var anchor = obj.AddComponent<PortalAnchor>();
            anchor.Configure(normal, allowPrimary, allowSecondary);
        }

        private void SetMask(string fieldName, int value)
        {
            var field = typeof(PortalPlacementValidator).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(validator, (LayerMask)value);
        }
    }
}
