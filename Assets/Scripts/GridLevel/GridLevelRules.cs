using System.Collections.Generic;
using UnityEngine;

namespace SidePortal.GridLevel
{
    public static class GridLevelRules
    {
        public static bool CanPlaceBlock(GridLevelData data, Vector2Int origin, Vector2Int size)
        {
            var normalizedSize = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
            if (origin.x < 0 || origin.y < 0 || origin.x + normalizedSize.x > data.Width || origin.y + normalizedSize.y > data.Height)
            {
                return false;
            }

            var candidate = new RectInt(origin, normalizedSize);
            foreach (var block in data.Blocks)
            {
                if (Overlaps(candidate, block.Bounds))
                {
                    return false;
                }
            }

            return true;
        }

        public static GridBlockInstance FindBlockAt(GridLevelData data, Vector2Int cell)
        {
            foreach (var block in data.Blocks)
            {
                if (block.ContainsCell(cell))
                {
                    return block;
                }
            }

            return null;
        }

        public static bool IsEdgeExposed(GridLevelData data, GridBlockInstance block, GridEdge edge)
        {
            if (block == null)
            {
                return false;
            }

            foreach (var other in data.Blocks)
            {
                if (other.Id == block.Id)
                {
                    continue;
                }

                if (TouchesEdge(block.Bounds, other.Bounds, edge))
                {
                    return false;
                }
            }

            return true;
        }

        public static void RemoveInvalidPortalEdges(GridLevelData data, List<PortalEdge> portalEdges)
        {
            portalEdges.RemoveAll(edge =>
            {
                var block = data.FindBlock(edge.BlockId);
                return block == null || !IsEdgeExposed(data, block, edge.Edge);
            });
        }

        public static Vector2 EdgeWorldPosition(GridBlockInstance block, GridEdge edge, float cellSize)
        {
            var center = (Vector2)block.Origin + (Vector2)block.Size * 0.5f;
            switch (edge)
            {
                case GridEdge.Left:
                    center.x = block.Origin.x;
                    break;
                case GridEdge.Right:
                    center.x = block.Origin.x + block.Size.x;
                    break;
                case GridEdge.Up:
                    center.y = block.Origin.y + block.Size.y;
                    break;
                case GridEdge.Down:
                    center.y = block.Origin.y;
                    break;
            }

            return center * cellSize;
        }

        public static Vector2 EdgeNormal(GridEdge edge)
        {
            return edge switch
            {
                GridEdge.Left => Vector2.left,
                GridEdge.Right => Vector2.right,
                GridEdge.Up => Vector2.up,
                GridEdge.Down => Vector2.down,
                _ => Vector2.right
            };
        }

        private static bool Overlaps(RectInt a, RectInt b)
        {
            return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
        }

        private static bool TouchesEdge(RectInt block, RectInt other, GridEdge edge)
        {
            switch (edge)
            {
                case GridEdge.Left:
                    return block.xMin == other.xMax && RangesOverlap(block.yMin, block.yMax, other.yMin, other.yMax);
                case GridEdge.Right:
                    return block.xMax == other.xMin && RangesOverlap(block.yMin, block.yMax, other.yMin, other.yMax);
                case GridEdge.Up:
                    return block.yMax == other.yMin && RangesOverlap(block.xMin, block.xMax, other.xMin, other.xMax);
                case GridEdge.Down:
                    return block.yMin == other.yMax && RangesOverlap(block.xMin, block.xMax, other.xMin, other.xMax);
                default:
                    return false;
            }
        }

        private static bool RangesOverlap(int minA, int maxA, int minB, int maxB)
        {
            return minA < maxB && maxA > minB;
        }
    }
}
