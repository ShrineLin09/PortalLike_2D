using NUnit.Framework;
using SidePortal.GridLevel;
using UnityEngine;

namespace SidePortal.Tests.EditMode
{
    public sealed class GridLevelRulesTests
    {
        private GridLevelData data;

        [SetUp]
        public void SetUp()
        {
            data = ScriptableObject.CreateInstance<GridLevelData>();
            data.ConfigureSize(10, 8, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(data);
        }

        [Test]
        public void CanPlaceBlock_RejectsOverlappingBlocks()
        {
            data.AddBlock(new Vector2Int(1, 1), new Vector2Int(2, 2), GridBlockType.Solid);

            Assert.That(GridLevelRules.CanPlaceBlock(data, new Vector2Int(2, 2), Vector2Int.one), Is.False);
            Assert.That(GridLevelRules.CanPlaceBlock(data, new Vector2Int(3, 1), Vector2Int.one), Is.True);
        }

        [Test]
        public void IsEdgeExposed_ReturnsFalse_WhenAnotherBlockTouchesEdge()
        {
            var leftId = data.AddBlock(new Vector2Int(1, 1), new Vector2Int(2, 2), GridBlockType.Solid);
            data.AddBlock(new Vector2Int(3, 1), new Vector2Int(1, 2), GridBlockType.Solid);

            var block = data.FindBlock(leftId);

            Assert.That(GridLevelRules.IsEdgeExposed(data, block, GridEdge.Right), Is.False);
            Assert.That(GridLevelRules.IsEdgeExposed(data, block, GridEdge.Left), Is.True);
        }

        [Test]
        public void RemoveInvalidPortalEdges_RemovesEdgesThatBecomeCovered()
        {
            var leftId = data.AddBlock(new Vector2Int(1, 1), new Vector2Int(2, 2), GridBlockType.Solid);
            data.AddOrReplacePortalEdge(leftId, GridEdge.Right);
            data.AddBlock(new Vector2Int(3, 1), new Vector2Int(1, 2), GridBlockType.Obstacle);

            data.RemoveInvalidPortalEdges();

            Assert.That(data.PortalEdges, Is.Empty);
        }
    }
}
