using SidePortal.Debugging;
using SidePortal.Level;
using SidePortal.Player;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.GridLevel
{
    public sealed class GridLevelBuilder : MonoBehaviour
    {
        private const int PortalAnchorLayer = 6;
        private const int SolidLayer = 7;
        private const int PortalLayer = 8;
        private const int PlayerLayer = 9;

        [SerializeField] private GridLevelData levelData;
        [SerializeField] private bool buildOnAwake = true;

        private Transform generatedRoot;
        private Sprite solidSprite;
        private Sprite obstacleSprite;
        private Sprite playerSprite;
        private Sprite primaryPortalSprite;
        private Sprite secondaryPortalSprite;
        private Sprite exitSprite;
        private Sprite anchorSprite;

        public GridLevelData LevelData => levelData;

        public void Configure(GridLevelData data, bool shouldBuildOnAwake)
        {
            levelData = data;
            buildOnAwake = shouldBuildOnAwake;
        }

        private void Awake()
        {
            if (buildOnAwake && levelData != null)
            {
                Build(levelData);
            }
        }

        public void Build(GridLevelData data)
        {
            if (data == null)
            {
                return;
            }

            levelData = data;
            Physics2D.queriesHitTriggers = true;
            ClearGeneratedLevel();
            CreateSprites();

            generatedRoot = new GameObject("GeneratedGridLevel").transform;
            generatedRoot.SetParent(transform);

            var levelManager = CreateChild("LevelManager");
            levelManager.AddComponent<LevelManager>();
            var completeOverlay = levelManager.AddComponent<LevelCompleteOverlay>();

            var primaryPortalPrefab = CreatePortalTemplate("PortalPrimaryTemplate", primaryPortalSprite, true);
            var secondaryPortalPrefab = CreatePortalTemplate("PortalSecondaryTemplate", secondaryPortalSprite, false);
            var player = CreatePlayer(data, primaryPortalPrefab, secondaryPortalPrefab);

            CreateCamera(player.transform, data);
            CreateDebugOverlay(player);
            CreateBlocks(data);
            CreatePortalAnchors(data);
            CreateExit(data, completeOverlay);
        }

        public void ClearGeneratedLevel()
        {
            if (generatedRoot == null)
            {
                var existing = transform.Find("GeneratedGridLevel");
                generatedRoot = existing;
            }

            if (generatedRoot == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedRoot.gameObject);
            }
            else
            {
                DestroyImmediate(generatedRoot.gameObject);
            }

            generatedRoot = null;
        }

        private void CreateSprites()
        {
            solidSprite = CreateSprite(new Color32(80, 92, 108, 255));
            obstacleSprite = CreateSprite(new Color32(130, 68, 72, 255));
            playerSprite = CreateSprite(new Color32(245, 204, 92, 255));
            primaryPortalSprite = CreateSprite(new Color32(66, 180, 255, 255));
            secondaryPortalSprite = CreateSprite(new Color32(255, 142, 45, 255));
            exitSprite = CreateSprite(new Color32(104, 225, 119, 255));
            anchorSprite = CreateSprite(new Color32(160, 220, 255, 115));
        }

        private static Sprite CreateSprite(Color32 color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private Portal CreatePortalTemplate(string name, Sprite sprite, bool primary)
        {
            var portalObject = CreateChild(name);
            portalObject.SetActive(false);
            portalObject.layer = PortalLayer;
            portalObject.transform.localScale = new Vector3(0.18f, 2.1f, 1f);

            var renderer = portalObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portal = portalObject.AddComponent<Portal>();
            portal.Configure(primary, null, Vector2.zero, Vector2.right);
            return portal;
        }

        private PlayerController CreatePlayer(GridLevelData data, Portal primaryPortalPrefab, Portal secondaryPortalPrefab)
        {
            var playerObject = CreateChild("Player");
            playerObject.tag = "Player";
            playerObject.layer = PlayerLayer;
            playerObject.transform.position = CellCenter(data.PlayerSpawn, data.CellSize);
            playerObject.transform.localScale = new Vector3(0.85f, 1.55f, 1f);

            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = playerSprite;
            renderer.sortingOrder = 6;

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(playerObject.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.55f, 0f);

            var fireOrigin = new GameObject("FireOrigin");
            fireOrigin.transform.SetParent(playerObject.transform);
            fireOrigin.transform.localPosition = new Vector3(0.42f, 0.08f, 0f);

            var controller = playerObject.AddComponent<PlayerController>();
            controller.ConfigureGroundCheck(groundCheck.transform, (LayerMask)(1 << SolidLayer));

            var validator = playerObject.AddComponent<PortalPlacementValidator>();
            validator.ConfigureMasks((LayerMask)(1 << PortalAnchorLayer), (LayerMask)(1 << SolidLayer), (LayerMask)(1 << PortalLayer));

            var gun = playerObject.AddComponent<PortalGun>();
            gun.Configure(fireOrigin.transform, controller, primaryPortalPrefab, secondaryPortalPrefab);
            return controller;
        }

        private void CreateCamera(Transform target, GridLevelData data)
        {
            var cameraObject = CreateChild("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(data.Width * data.CellSize * 0.5f, data.Height * data.CellSize * 0.45f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.15f;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);

            var follow = cameraObject.AddComponent<SimpleCameraFollow>();
            follow.SetTarget(target);
            follow.Configure(0.08f, new Vector2(0f, 0f), new Vector2(data.Width * data.CellSize, data.Height * data.CellSize));
        }

        private void CreateDebugOverlay(PlayerController player)
        {
            var debugObject = CreateChild("PortalDebugOverlay");
            var overlay = debugObject.AddComponent<PortalDebugOverlay>();
            overlay.Configure(player, player.GetComponent<PortalGun>());
        }

        private void CreateBlocks(GridLevelData data)
        {
            foreach (var block in data.Blocks)
            {
                var blockObject = CreateChild($"Block_{block.Id}_{block.Type}");
                blockObject.layer = SolidLayer;
                blockObject.transform.position = RectCenter(block.Origin, block.Size, data.CellSize);
                blockObject.transform.localScale = new Vector3(block.Size.x * data.CellSize, block.Size.y * data.CellSize, 1f);

                var renderer = blockObject.AddComponent<SpriteRenderer>();
                renderer.sprite = block.Type == GridBlockType.Obstacle ? obstacleSprite : solidSprite;
                renderer.sortingOrder = block.Type == GridBlockType.Obstacle ? 2 : 1;

                var collider = blockObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
            }
        }

        private void CreatePortalAnchors(GridLevelData data)
        {
            foreach (var edge in data.PortalEdges)
            {
                var block = data.FindBlock(edge.BlockId);
                if (block == null || !GridLevelRules.IsEdgeExposed(data, block, edge.Edge))
                {
                    continue;
                }

                var normal = GridLevelRules.EdgeNormal(edge.Edge);
                var anchorObject = CreateChild($"PortalAnchor_{block.Id}_{edge.Edge}");
                anchorObject.layer = PortalAnchorLayer;
                anchorObject.transform.position = GridLevelRules.EdgeWorldPosition(block, edge.Edge, data.CellSize)
                    + normal * data.CellSize * 0.12f;
                anchorObject.transform.localScale = AnchorScale(edge.Edge, data.CellSize);

                var renderer = anchorObject.AddComponent<SpriteRenderer>();
                renderer.sprite = anchorSprite;
                renderer.sortingOrder = 3;

                var collider = anchorObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = Vector2.one;

                var anchor = anchorObject.AddComponent<PortalAnchor>();
                anchor.Configure(normal, edge.AllowPrimary, edge.AllowSecondary);
            }
        }

        private void CreateExit(GridLevelData data, LevelCompleteOverlay completeOverlay)
        {
            var exit = CreateChild("LevelExit");
            exit.layer = SolidLayer;
            exit.transform.position = CellCenter(data.ExitCell, data.CellSize);
            exit.transform.localScale = new Vector3(data.CellSize * 1.2f, data.CellSize * 2.4f, 1f);

            var renderer = exit.AddComponent<SpriteRenderer>();
            renderer.sprite = exitSprite;
            renderer.sortingOrder = 4;

            var collider = exit.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var levelExit = exit.AddComponent<LevelExit>();
            levelExit.Configure(false, completeOverlay);
        }

        private GameObject CreateChild(string name)
        {
            var obj = new GameObject(name);
            if (generatedRoot != null)
            {
                obj.transform.SetParent(generatedRoot);
            }

            return obj;
        }

        private static Vector3 CellCenter(Vector2Int cell, float cellSize)
        {
            return new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
        }

        private static Vector3 RectCenter(Vector2Int origin, Vector2Int size, float cellSize)
        {
            return new Vector3((origin.x + size.x * 0.5f) * cellSize, (origin.y + size.y * 0.5f) * cellSize, 0f);
        }

        private static Vector3 AnchorScale(GridEdge edge, float cellSize)
        {
            return edge == GridEdge.Left || edge == GridEdge.Right
                ? new Vector3(0.45f * cellSize, 1.5f * cellSize, 1f)
                : new Vector3(1.5f * cellSize, 0.45f * cellSize, 1f);
        }
    }
}
