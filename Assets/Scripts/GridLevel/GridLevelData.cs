using System.Collections.Generic;
using UnityEngine;

namespace SidePortal.GridLevel
{
    [CreateAssetMenu(menuName = "SidePortal/Grid Level Data", fileName = "GridLevelData")]
    public sealed class GridLevelData : ScriptableObject
    {
        [SerializeField] private int width = 24;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2Int playerSpawn = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int exitCell = new Vector2Int(20, 2);
        [SerializeField] private List<GridBlockInstance> blocks = new List<GridBlockInstance>();
        [SerializeField] private List<PortalEdge> portalEdges = new List<PortalEdge>();
        [SerializeField] private int nextBlockId = 1;

        public int Width => width;
        public int Height => height;
        public float CellSize => Mathf.Max(0.1f, cellSize);
        public Vector2Int PlayerSpawn => playerSpawn;
        public Vector2Int ExitCell => exitCell;
        public IReadOnlyList<GridBlockInstance> Blocks => blocks;
        public IReadOnlyList<PortalEdge> PortalEdges => portalEdges;

        public int AddBlock(Vector2Int origin, Vector2Int size, GridBlockType type, string shapeId = "rect")
        {
            var id = nextBlockId++;
            blocks.Add(new GridBlockInstance(id, origin, size, type, shapeId));
            return id;
        }

        public bool RemoveBlock(int blockId)
        {
            var removed = blocks.RemoveAll(block => block.Id == blockId) > 0;
            if (removed)
            {
                portalEdges.RemoveAll(edge => edge.BlockId == blockId);
            }

            return removed;
        }

        public void SetPlayerSpawn(Vector2Int cell)
        {
            playerSpawn = cell;
        }

        public void SetExitCell(Vector2Int cell)
        {
            exitCell = cell;
        }

        public void AddOrReplacePortalEdge(int blockId, GridEdge edge, bool allowPrimary = true, bool allowSecondary = true)
        {
            portalEdges.RemoveAll(existing => existing.Matches(blockId, edge));
            portalEdges.Add(new PortalEdge(blockId, edge, allowPrimary, allowSecondary));
        }

        public bool RemovePortalEdge(int blockId, GridEdge edge)
        {
            return portalEdges.RemoveAll(existing => existing.Matches(blockId, edge)) > 0;
        }

        public GridBlockInstance FindBlock(int blockId)
        {
            return blocks.Find(block => block.Id == blockId);
        }

        public void RemoveInvalidPortalEdges()
        {
            GridLevelRules.RemoveInvalidPortalEdges(this, portalEdges);
        }

        public void ClearAll()
        {
            blocks.Clear();
            portalEdges.Clear();
            nextBlockId = 1;
        }

        public void ConfigureSize(int newWidth, int newHeight, float newCellSize)
        {
            width = Mathf.Max(1, newWidth);
            height = Mathf.Max(1, newHeight);
            cellSize = Mathf.Max(0.1f, newCellSize);
        }
    }
}
