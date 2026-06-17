using System.IO;
using SidePortal.GridLevel;
using UnityEditor;
using UnityEngine;

namespace SidePortal.EditorTools
{
    public sealed class GridLevelEditorWindow : EditorWindow
    {
        private enum ToolMode
        {
            PlaceSolid,
            PlaceObstacle,
            DeleteBlock,
            SetPlayerSpawn,
            SetExit,
            TogglePortalEdge
        }

        private static readonly Vector2Int[] SizePresets =
        {
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
            new Vector2Int(1, 2),
            new Vector2Int(2, 2)
        };

        private GridLevelData levelData;
        private ToolMode toolMode;
        private int sizePresetIndex;
        private GridEdge selectedEdge = GridEdge.Right;
        private bool allowPrimary = true;
        private bool allowSecondary = true;
        private Vector2 scroll;
        private string status = "Select or create a GridLevelData asset.";

        [MenuItem("SidePortal/Grid Level Editor")]
        public static void Open()
        {
            GetWindow<GridLevelEditorWindow>("Grid Level Editor");
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(8f);

            if (levelData == null)
            {
                EditorGUILayout.HelpBox(status, MessageType.Info);
                return;
            }

            DrawTools();
            EditorGUILayout.Space(8f);
            DrawGrid();
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(status, MessageType.None);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                levelData = (GridLevelData)EditorGUILayout.ObjectField(levelData, typeof(GridLevelData), false, GUILayout.MinWidth(240f));
                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                {
                    CreateLevelAsset();
                }

                if (levelData != null && GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                {
                    EditorGUIUtility.PingObject(levelData);
                }
            }
        }

        private void DrawTools()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                toolMode = (ToolMode)EditorGUILayout.EnumPopup("Tool", toolMode);
                sizePresetIndex = EditorGUILayout.Popup("Block Size", sizePresetIndex, new[] { "1x1", "2x1", "1x2", "2x2" });
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                selectedEdge = (GridEdge)EditorGUILayout.EnumPopup("Portal Edge", selectedEdge);
                allowPrimary = EditorGUILayout.ToggleLeft("Blue", allowPrimary, GUILayout.Width(70f));
                allowSecondary = EditorGUILayout.ToggleLeft("Yellow", allowSecondary, GUILayout.Width(80f));
            }

            if (GUILayout.Button("Remove Invalid Portal Edges"))
            {
                Record("Remove invalid portal edges");
                levelData.RemoveInvalidPortalEdges();
                Save();
            }
        }

        private void DrawGrid()
        {
            var cellSize = 28f;
            var gridWidth = levelData.Width * cellSize;
            var gridHeight = levelData.Height * cellSize;

            scroll = EditorGUILayout.BeginScrollView(scroll);
            var rect = GUILayoutUtility.GetRect(gridWidth + 1f, gridHeight + 1f);
            EditorGUI.DrawRect(rect, new Color(0.09f, 0.09f, 0.1f));

            DrawCells(rect, cellSize);
            DrawBlocks(rect, cellSize);
            DrawPortalEdges(rect, cellSize);
            DrawMarkers(rect, cellSize);
            HandleGridInput(rect, cellSize);

            EditorGUILayout.EndScrollView();
        }

        private void DrawCells(Rect rect, float cellSize)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.12f);
            for (var x = 0; x <= levelData.Width; x++)
            {
                var px = rect.x + x * cellSize;
                Handles.DrawLine(new Vector3(px, rect.y), new Vector3(px, rect.y + levelData.Height * cellSize));
            }

            for (var y = 0; y <= levelData.Height; y++)
            {
                var py = rect.y + y * cellSize;
                Handles.DrawLine(new Vector3(rect.x, py), new Vector3(rect.x + levelData.Width * cellSize, py));
            }
            Handles.EndGUI();
        }

        private void DrawBlocks(Rect rect, float cellSize)
        {
            foreach (var block in levelData.Blocks)
            {
                var blockRect = BlockRect(rect, cellSize, block.Origin, block.Size);
                var color = block.Type == GridBlockType.Obstacle
                    ? new Color(0.5f, 0.18f, 0.18f, 0.95f)
                    : new Color(0.28f, 0.34f, 0.42f, 0.95f);
                EditorGUI.DrawRect(blockRect, color);
                GUI.Label(blockRect, $"{block.Id}\n{block.Size.x}x{block.Size.y}", EditorStyles.whiteMiniLabel);
            }
        }

        private void DrawPortalEdges(Rect rect, float cellSize)
        {
            Handles.BeginGUI();
            Handles.color = new Color(0.2f, 0.8f, 1f, 0.85f);
            foreach (var edge in levelData.PortalEdges)
            {
                var block = levelData.FindBlock(edge.BlockId);
                if (block == null)
                {
                    continue;
                }

                var startEnd = EdgeLine(rect, cellSize, block.Origin, block.Size, edge.Edge);
                Handles.DrawAAPolyLine(5f, startEnd.Item1, startEnd.Item2);
            }
            Handles.EndGUI();
        }

        private void DrawMarkers(Rect rect, float cellSize)
        {
            DrawCellMarker(rect, cellSize, levelData.PlayerSpawn, new Color(0.95f, 0.78f, 0.25f, 0.9f), "P");
            DrawCellMarker(rect, cellSize, levelData.ExitCell, new Color(0.3f, 0.9f, 0.45f, 0.9f), "E");
        }

        private void DrawCellMarker(Rect rect, float cellSize, Vector2Int cell, Color color, string label)
        {
            if (!IsInBounds(cell))
            {
                return;
            }

            var cellRect = CellRect(rect, cellSize, cell);
            EditorGUI.DrawRect(cellRect, color);
            GUI.Label(cellRect, label, EditorStyles.boldLabel);
        }

        private void HandleGridInput(Rect rect, float cellSize)
        {
            var current = Event.current;
            if (current.type != EventType.MouseDown || current.button != 0 || !rect.Contains(current.mousePosition))
            {
                return;
            }

            var cell = MouseToCell(rect, cellSize, current.mousePosition);
            ApplyTool(cell);
            current.Use();
        }

        private void ApplyTool(Vector2Int cell)
        {
            switch (toolMode)
            {
                case ToolMode.PlaceSolid:
                    PlaceBlock(cell, GridBlockType.Solid);
                    break;
                case ToolMode.PlaceObstacle:
                    PlaceBlock(cell, GridBlockType.Obstacle);
                    break;
                case ToolMode.DeleteBlock:
                    DeleteBlock(cell);
                    break;
                case ToolMode.SetPlayerSpawn:
                    SetPlayerSpawn(cell);
                    break;
                case ToolMode.SetExit:
                    SetExit(cell);
                    break;
                case ToolMode.TogglePortalEdge:
                    TogglePortalEdge(cell);
                    break;
            }
        }

        private void PlaceBlock(Vector2Int cell, GridBlockType type)
        {
            var size = SizePresets[Mathf.Clamp(sizePresetIndex, 0, SizePresets.Length - 1)];
            if (!GridLevelRules.CanPlaceBlock(levelData, cell, size))
            {
                status = $"Cannot place {size.x}x{size.y} block at {cell}; out of bounds or occupied.";
                return;
            }

            Record("Place grid block");
            levelData.AddBlock(cell, size, type);
            levelData.RemoveInvalidPortalEdges();
            Save();
            status = $"Placed {type} block at {cell}.";
        }

        private void DeleteBlock(Vector2Int cell)
        {
            var block = GridLevelRules.FindBlockAt(levelData, cell);
            if (block == null)
            {
                status = $"No block at {cell}.";
                return;
            }

            Record("Delete grid block");
            levelData.RemoveBlock(block.Id);
            levelData.RemoveInvalidPortalEdges();
            Save();
            status = $"Deleted block {block.Id}.";
        }

        private void SetPlayerSpawn(Vector2Int cell)
        {
            if (!IsInBounds(cell))
            {
                status = $"Spawn cell {cell} is out of bounds.";
                return;
            }

            Record("Set player spawn");
            levelData.SetPlayerSpawn(cell);
            Save();
            status = $"Player spawn set to {cell}.";
        }

        private void SetExit(Vector2Int cell)
        {
            if (!IsInBounds(cell))
            {
                status = $"Exit cell {cell} is out of bounds.";
                return;
            }

            Record("Set exit cell");
            levelData.SetExitCell(cell);
            Save();
            status = $"Exit set to {cell}.";
        }

        private void TogglePortalEdge(Vector2Int cell)
        {
            var block = GridLevelRules.FindBlockAt(levelData, cell);
            if (block == null)
            {
                status = $"No block at {cell}; portal edges belong to blocks.";
                return;
            }

            if (!GridLevelRules.IsEdgeExposed(levelData, block, selectedEdge))
            {
                status = $"Block {block.Id} {selectedEdge} edge touches another block; portal marker rejected.";
                return;
            }

            Record("Toggle portal edge");
            if (HasPortalEdge(block.Id, selectedEdge))
            {
                levelData.RemovePortalEdge(block.Id, selectedEdge);
                status = $"Removed portal edge {selectedEdge} from block {block.Id}.";
            }
            else
            {
                levelData.AddOrReplacePortalEdge(block.Id, selectedEdge, allowPrimary, allowSecondary);
                status = $"Added portal edge {selectedEdge} to block {block.Id}.";
            }

            Save();
        }

        private bool HasPortalEdge(int blockId, GridEdge edge)
        {
            foreach (var portalEdge in levelData.PortalEdges)
            {
                if (portalEdge.Matches(blockId, edge))
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateLevelAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Grid Level Data",
                "NewGridLevel",
                "asset",
                "Choose a location for the grid level asset.",
                "Assets/Data/Levels");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            EnsureFolder(Path.GetDirectoryName(path)?.Replace("\\", "/"));
            var asset = CreateInstance<GridLevelData>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            levelData = asset;
            status = $"Created {path}.";
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var name = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private void Record(string action)
        {
            Undo.RecordObject(levelData, action);
        }

        private void Save()
        {
            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private bool IsInBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < levelData.Width && cell.y < levelData.Height;
        }

        private static Vector2Int MouseToCell(Rect rect, float cellSize, Vector2 mouse)
        {
            var x = Mathf.FloorToInt((mouse.x - rect.x) / cellSize);
            var yFromTop = Mathf.FloorToInt((mouse.y - rect.y) / cellSize);
            return new Vector2Int(x, Mathf.Max(0, Mathf.FloorToInt((rect.height - 1f) / cellSize) - yFromTop));
        }

        private Rect CellRect(Rect rect, float cellSize, Vector2Int cell)
        {
            return new Rect(rect.x + cell.x * cellSize, rect.y + (levelData.Height - cell.y - 1) * cellSize, cellSize, cellSize);
        }

        private Rect BlockRect(Rect rect, float cellSize, Vector2Int origin, Vector2Int size)
        {
            return new Rect(
                rect.x + origin.x * cellSize,
                rect.y + (levelData.Height - origin.y - size.y) * cellSize,
                size.x * cellSize,
                size.y * cellSize);
        }

        private Rect BlockRect(Rect rect, float cellSize, Vector2Int origin, int width, int height)
        {
            return BlockRect(rect, cellSize, origin, new Vector2Int(width, height));
        }

        private System.Tuple<Vector3, Vector3> EdgeLine(Rect rect, float cellSize, Vector2Int origin, Vector2Int size, GridEdge edge)
        {
            var blockRect = BlockRect(rect, cellSize, origin, size.x, size.y);
            return edge switch
            {
                GridEdge.Left => System.Tuple.Create(
                    new Vector3(blockRect.xMin, blockRect.yMin),
                    new Vector3(blockRect.xMin, blockRect.yMax)),
                GridEdge.Right => System.Tuple.Create(
                    new Vector3(blockRect.xMax, blockRect.yMin),
                    new Vector3(blockRect.xMax, blockRect.yMax)),
                GridEdge.Up => System.Tuple.Create(
                    new Vector3(blockRect.xMin, blockRect.yMin),
                    new Vector3(blockRect.xMax, blockRect.yMin)),
                GridEdge.Down => System.Tuple.Create(
                    new Vector3(blockRect.xMin, blockRect.yMax),
                    new Vector3(blockRect.xMax, blockRect.yMax)),
                _ => System.Tuple.Create(Vector3.zero, Vector3.zero)
            };
        }
    }
}
