using SidePortal.Configuration;
using SidePortal.Debugging;
using SidePortal.Player;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Level
{
    public sealed class VerticalSliceRuntimeBootstrap : MonoBehaviour
    {
        private const int PortalAnchorLayer = 6;
        private const int SolidLayer = 7;
        private const int PortalLayer = 8;
        private const int PlayerLayer = 9;

        [SerializeField] private bool buildOnAwake = true;

        private Sprite blockSprite;
        private Sprite playerSprite;
        private Sprite primaryPortalSprite;
        private Sprite secondaryPortalSprite;
        private Sprite exitSprite;
        private Sprite anchorSprite;

        private void Awake()
        {
            if (buildOnAwake)
            {
                Build();
            }
        }

        public void Build()
        {
            Physics2D.queriesHitTriggers = true;
            CreateSprites();

            var levelManager = new GameObject("LevelManager");
            levelManager.AddComponent<LevelManager>();
            var completeOverlay = levelManager.AddComponent<LevelCompleteOverlay>();

            var primaryPortalPrefab = CreatePortalTemplate("PortalPrimaryTemplate", primaryPortalSprite, true);
            var secondaryPortalPrefab = CreatePortalTemplate("PortalSecondaryTemplate", secondaryPortalSprite, false);

            var player = CreatePlayer(primaryPortalPrefab, secondaryPortalPrefab);
            CreateCamera(player.transform);
            CreateDebugOverlay(player);

            CreateBlock("StartFloor", new Vector2(-5f, -2.5f), new Vector2(7f, 1f));
            CreateBlock("StartBackWall", new Vector2(-8.5f, 0f), new Vector2(0.8f, 5f));
            CreateBlock("ExitFloor", new Vector2(11f, -2.5f), new Vector2(10f, 1f));
            CreateBlock("ExitBackWall", new Vector2(16.5f, 0f), new Vector2(0.8f, 5f));
            CreateBlock("Ceiling", new Vector2(4f, 5f), new Vector2(24f, 1f));
            CreateBlock("HighMomentumDrop", new Vector2(-6f, 2.6f), new Vector2(2.5f, 0.6f));
            CreateAnchor("StartAnchor", new Vector2(-8.02f, -0.55f), Vector2.right, true, true);
            CreateAnchor("ExitAnchor", new Vector2(16.02f, -0.55f), Vector2.left, true, true);
            CreateAnchor("CeilingAnchor", new Vector2(-6f, 2.22f), Vector2.down, true, true);
            CreateExit(completeOverlay);
        }

        private void CreateSprites()
        {
            blockSprite = CreateSprite(new Color32(80, 92, 108, 255));
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
            var portalObject = new GameObject(name);
            portalObject.SetActive(false);
            portalObject.layer = PortalLayer;
            portalObject.transform.localScale = PrototypeTuning.PortalScale;

            var renderer = portalObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;

            var collider = portalObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portal = portalObject.AddComponent<Portal>();
            portal.Configure(primary, null, Vector2.zero, Vector2.right);
            portal.ConfigureMomentum(PrototypeTuning.PortalMomentum);
            return portal;
        }

        private PlayerController CreatePlayer(Portal primaryPortalPrefab, Portal secondaryPortalPrefab)
        {
            var scale = PrototypeTuning.LevelDesignScale;
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.layer = PlayerLayer;
            playerObject.transform.position = new Vector3(-6f, -1.25f, 0f);
            playerObject.transform.localScale = new Vector3(scale.PlayerWidth, scale.PlayerHeight, 1f);

            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = playerSprite;
            renderer.sortingOrder = 6;

            var body = playerObject.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(playerObject.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.55f, 0f);

            var fireOrigin = new GameObject("FireOrigin");
            fireOrigin.transform.SetParent(playerObject.transform);
            fireOrigin.transform.localPosition = new Vector3(0.42f, 0.08f, 0f);

            var controller = playerObject.AddComponent<PlayerController>();
            controller.ConfigurePhysics(PrototypeTuning.PlayerPhysics);
            controller.ConfigureGroundCheck(groundCheck.transform, (LayerMask)(1 << SolidLayer));

            var validator = playerObject.AddComponent<PortalPlacementValidator>();
            validator.ConfigureMasks(
                (LayerMask)(1 << PortalAnchorLayer),
                (LayerMask)(1 << SolidLayer),
                (LayerMask)(1 << PortalLayer));

            var gun = playerObject.AddComponent<PortalGun>();
            gun.Configure(fireOrigin.transform, controller, primaryPortalPrefab, secondaryPortalPrefab);

            return controller;
        }

        private void CreateCamera(Transform target)
        {
            var cameraView = PrototypeTuning.CameraView;
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(1f, 1f, cameraView.FollowOffset.z);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = cameraView.OrthographicSize;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);

            var follow = cameraObject.AddComponent<SimpleCameraFollow>();
            follow.SetTarget(target);
            follow.Configure(cameraView, new Vector2(-8f, -1.5f), new Vector2(16f, 5f));
        }

        private static void CreateDebugOverlay(PlayerController player)
        {
            var debugObject = new GameObject("PortalDebugOverlay");
            var overlay = debugObject.AddComponent<PortalDebugOverlay>();
            overlay.Configure(player, player.GetComponent<PortalGun>());
        }

        private void CreateBlock(string name, Vector2 position, Vector2 scale)
        {
            var block = new GameObject(name);
            block.layer = SolidLayer;
            block.transform.position = position;
            block.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = blockSprite;

            var collider = block.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }

        private void CreateAnchor(string name, Vector2 position, Vector2 normal, bool allowPrimary, bool allowSecondary)
        {
            var anchor = new GameObject(name);
            anchor.layer = PortalAnchorLayer;
            anchor.transform.position = position;
            anchor.transform.localScale = Mathf.Abs(normal.x) > 0.5f
                ? PrototypeTuning.PortalAnchorScale(true, 1f)
                : PrototypeTuning.PortalAnchorScale(false, 1f);

            var renderer = anchor.AddComponent<SpriteRenderer>();
            renderer.sprite = anchorSprite;
            renderer.sortingOrder = 3;

            var collider = anchor.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portalAnchor = anchor.AddComponent<PortalAnchor>();
            portalAnchor.Configure(normal, allowPrimary, allowSecondary);
        }

        private void CreateExit(LevelCompleteOverlay completeOverlay)
        {
            var exit = new GameObject("LevelExit");
            exit.layer = SolidLayer;
            exit.transform.position = new Vector3(15.8f, -0.8f, 0f);
            exit.transform.localScale = new Vector3(1.2f, 2.4f, 1f);

            var renderer = exit.AddComponent<SpriteRenderer>();
            renderer.sprite = exitSprite;
            renderer.sortingOrder = 4;

            var collider = exit.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var levelExit = exit.AddComponent<LevelExit>();
            levelExit.Configure(false, completeOverlay);
        }
    }
}
