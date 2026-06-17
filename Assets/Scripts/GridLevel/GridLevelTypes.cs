using System;
using UnityEngine;

namespace SidePortal.GridLevel
{
    public enum GridBlockType
    {
        Solid,
        Obstacle
    }

    public enum GridEdge
    {
        Left,
        Right,
        Up,
        Down
    }

    [Serializable]
    public sealed class GridBlockInstance
    {
        [SerializeField] private int id;
        [SerializeField] private Vector2Int origin;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private GridBlockType type;
        [SerializeField] private string shapeId = "rect";

        public int Id => id;
        public Vector2Int Origin => origin;
        public Vector2Int Size => size;
        public GridBlockType Type => type;
        public string ShapeId => shapeId;

        public GridBlockInstance(int id, Vector2Int origin, Vector2Int size, GridBlockType type, string shapeId = "rect")
        {
            this.id = id;
            this.origin = origin;
            this.size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
            this.type = type;
            this.shapeId = string.IsNullOrWhiteSpace(shapeId) ? "rect" : shapeId;
        }

        public bool ContainsCell(Vector2Int cell)
        {
            return cell.x >= origin.x
                && cell.y >= origin.y
                && cell.x < origin.x + size.x
                && cell.y < origin.y + size.y;
        }

        public RectInt Bounds => new RectInt(origin, size);
    }

    [Serializable]
    public sealed class PortalEdge
    {
        [SerializeField] private int blockId;
        [SerializeField] private GridEdge edge;
        [SerializeField] private bool allowPrimary = true;
        [SerializeField] private bool allowSecondary = true;

        public int BlockId => blockId;
        public GridEdge Edge => edge;
        public bool AllowPrimary => allowPrimary;
        public bool AllowSecondary => allowSecondary;

        public PortalEdge(int blockId, GridEdge edge, bool allowPrimary = true, bool allowSecondary = true)
        {
            this.blockId = blockId;
            this.edge = edge;
            this.allowPrimary = allowPrimary;
            this.allowSecondary = allowSecondary;
        }

        public bool Matches(int otherBlockId, GridEdge otherEdge)
        {
            return blockId == otherBlockId && edge == otherEdge;
        }
    }
}
