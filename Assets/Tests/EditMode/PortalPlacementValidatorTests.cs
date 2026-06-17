using System.Reflection;
using NUnit.Framework;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class PortalPlacementValidatorTests
    {
        private const int SurfaceLayer = 6;
        private const int BlockingLayer = 7;
        private const int PortalLayer = 8;

        private GameObject validatorObject;
        private PortalPlacementValidator validator;

        [SetUp]
        public void SetUp()
        {
            validatorObject = new GameObject("validator");
            validator = validatorObject.AddComponent<PortalPlacementValidator>();

            SetMask("portalSurfaceMask", 1 << SurfaceLayer);
            SetMask("placementBlockingMask", (1 << SurfaceLayer) | (1 << BlockingLayer));
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
            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Failure, Is.EqualTo(PortalPlacementFailure.NoSurfaceHit));
        }

        [Test]
        public void TryFindPlacement_Accepts_ClearAxisAlignedWall()
        {
            CreateBox("surface", new Vector2(4f, 0f), new Vector2(0.5f, 4f), SurfaceLayer);

            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Normal, Is.EqualTo(Vector2.left));
        }

        [Test]
        public void TryFindPlacement_Fails_WhenPortalOverlapsExistingPortal()
        {
            CreateBox("surface", new Vector2(4f, 0f), new Vector2(0.5f, 4f), SurfaceLayer);
            CreateBox("existing portal", new Vector2(3.67f, 0f), new Vector2(0.18f, 2.1f), PortalLayer);

            var result = validator.TryFindPlacement(Vector2.zero, Vector2.right);

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

        private void SetMask(string fieldName, int value)
        {
            var field = typeof(PortalPlacementValidator).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(validator, (LayerMask)value);
        }
    }
}
