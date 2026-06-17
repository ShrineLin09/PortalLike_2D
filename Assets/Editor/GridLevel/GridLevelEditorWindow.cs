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

        private static readonly string[] ToolLabels =
        {
            "放置道路/实体块",
            "放置障碍块",
            "删除块",
            "设置出生点",
            "设置通关点",
            "切换传送门边缘"
        };

        private static readonly string[] EdgeLabels =
        {
            "左边",
            "右边",
            "上边",
            "下边"
        };

        private GridLevelData levelData;
        private ToolMode toolMode;
        private int sizePresetIndex;
        private GridEdge selectedEdge = GridEdge.Right;
        private bool allowPrimary = true;
        private bool allowSecondary = true;
        private Vector2 scroll;
        private string status = "请选择或新建一个关卡数据资产。";

        [MenuItem("SidePortal/网格关卡编辑器")]
        public static void Open()
        {
            GetWindow<GridLevelEditorWindow>("网格关卡编辑器");
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
                if (GUILayout.Button("新建", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                {
                    CreateLevelAsset();
                }

                if (levelData != null && GUILayout.Button("定位", EditorStyles.toolbarButton, GUILayout.Width(52f)))
                {
                    EditorGUIUtility.PingObject(levelData);
                }
            }
        }

        private void DrawTools()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                toolMode = (ToolMode)EditorGUILayout.Popup("工具", (int)toolMode, ToolLabels);
                sizePresetIndex = EditorGUILayout.Popup("块尺寸", sizePresetIndex, new[] { "1x1", "2x1", "1x2", "2x2" });
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                selectedEdge = (GridEdge)EditorGUILayout.Popup("传送门边", (int)selectedEdge, EdgeLabels);
                allowPrimary = EditorGUILayout.ToggleLeft("允许蓝门", allowPrimary, GUILayout.Width(90f));
                allowSecondary = EditorGUILayout.ToggleLeft("允许黄门", allowSecondary, GUILayout.Width(90f));
            }

            if (GUILayout.Button("移除失效传送门边缘"))
            {
                Record("移除失效传送门边缘");
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
            DrawCellMarker(rect, cellSize, levelData.PlayerSpawn, new Color(0.95f, 0.78f, 0.25f, 0.9f), "生");
            DrawCellMarker(rect, cellSize, levelData.ExitCell, new Color(0.3f, 0.9f, 0.45f, 0.9f), "终");
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
                status = $"无法在 {FormatCell(cell)} 放置 {size.x}x{size.y} 块：超出边界或区域已被占用。";
                return;
            }

            Record("放置关卡块");
            levelData.AddBlock(cell, size, type);
            levelData.RemoveInvalidPortalEdges();
            Save();
            status = $"已在 {FormatCell(cell)} 放置{BlockTypeName(type)}。";
        }

        private void DeleteBlock(Vector2Int cell)
        {
            var block = GridLevelRules.FindBlockAt(levelData, cell);
            if (block == null)
            {
                status = $"{FormatCell(cell)} 没有关卡块。";
                return;
            }

            Record("删除关卡块");
            levelData.RemoveBlock(block.Id);
            levelData.RemoveInvalidPortalEdges();
            Save();
            status = $"已删除 {block.Id} 号关卡块。";
        }

        private void SetPlayerSpawn(Vector2Int cell)
        {
            if (!IsInBounds(cell))
            {
                status = $"出生点 {FormatCell(cell)} 超出关卡边界。";
                return;
            }

            Record("设置出生点");
            levelData.SetPlayerSpawn(cell);
            Save();
            status = $"出生点已设置为 {FormatCell(cell)}。";
        }

        private void SetExit(Vector2Int cell)
        {
            if (!IsInBounds(cell))
            {
                status = $"通关点 {FormatCell(cell)} 超出关卡边界。";
                return;
            }

            Record("设置通关点");
            levelData.SetExitCell(cell);
            Save();
            status = $"通关点已设置为 {FormatCell(cell)}。";
        }

        private void TogglePortalEdge(Vector2Int cell)
        {
            var block = GridLevelRules.FindBlockAt(levelData, cell);
            if (block == null)
            {
                status = $"{FormatCell(cell)} 没有关卡块；传送门边缘必须设置在块上。";
                return;
            }

            if (!GridLevelRules.IsEdgeExposed(levelData, block, selectedEdge))
            {
                status = $"{block.Id} 号块的{EdgeName(selectedEdge)}与其他块紧贴，不能设置传送门标记。";
                return;
            }

            Record("切换传送门边缘");
            if (HasPortalEdge(block.Id, selectedEdge))
            {
                levelData.RemovePortalEdge(block.Id, selectedEdge);
                status = $"已移除 {block.Id} 号块{EdgeName(selectedEdge)}的传送门标记。";
            }
            else
            {
                levelData.AddOrReplacePortalEdge(block.Id, selectedEdge, allowPrimary, allowSecondary);
                status = $"已在 {block.Id} 号块{EdgeName(selectedEdge)}添加传送门标记。";
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
                "创建关卡数据",
                "新关卡数据",
                "asset",
                "选择关卡数据资产的保存位置。",
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
            status = $"已创建：{path}";
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

        private static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x}, {cell.y})";
        }

        private static string BlockTypeName(GridBlockType type)
        {
            return type == GridBlockType.Obstacle ? "障碍块" : "道路/实体块";
        }

        private static string EdgeName(GridEdge edge)
        {
            return EdgeLabels[(int)edge];
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
