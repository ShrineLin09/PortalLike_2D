using System.IO;
using SidePortal.Debugging;
using SidePortal.Level;
using SidePortal.Player;
using SidePortal.Portals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SidePortal.EditorTools
{
    public static class VerticalSliceContentBuilder
    {
        private const int PortalAnchorLayer = 6;
        private const int SolidLayer = 7;
        private const int PortalLayer = 8;
        private const int PlayerLayer = 9;

        private const string ScenePath = "Assets/Scenes/Level_01_Tutorial.unity";
        private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
        private const string PrimaryPortalPrefabPath = "Assets/Prefabs/PortalPrimary.prefab";
        private const string SecondaryPortalPrefabPath = "Assets/Prefabs/PortalSecondary.prefab";
        private const string LevelExitPrefabPath = "Assets/Prefabs/LevelExit.prefab";
        private const string BlockPrefabPath = "Assets/Prefabs/Greybox/GreyboxBlock.prefab";

        [MenuItem("SidePortal/生成旧版可玩竖切")]
        public static void BuildPlayableVerticalSlice()
        {
            CreateFolders();
            var blockSprite = CreateSpriteAsset("Assets/Art/Greybox/block.png", new Color32(80, 92, 108, 255));
            var playerSprite = CreateSpriteAsset("Assets/Art/Greybox/player.png", new Color32(245, 204, 92, 255));
            var primarySprite = CreateSpriteAsset("Assets/Art/Greybox/portal_primary.png", new Color32(66, 180, 255, 255));
            var secondarySprite = CreateSpriteAsset("Assets/Art/Greybox/portal_secondary.png", new Color32(255, 142, 45, 255));
            var exitSprite = CreateSpriteAsset("Assets/Art/Greybox/exit.png", new Color32(104, 225, 119, 255));
            var anchorSprite = CreateSpriteAsset("Assets/Art/Greybox/portal_anchor.png", new Color32(160, 220, 255, 115));

            var primaryPortal = CreatePortalPrefab(PrimaryPortalPrefabPath, primarySprite, true);
            var secondaryPortal = CreatePortalPrefab(SecondaryPortalPrefabPath, secondarySprite, false);
            var blockPrefab = CreateBlockPrefab(blockSprite);
            var exitPrefab = CreateExitPrefab(exitSprite);
            var playerPrefab = CreatePlayerPrefab(playerSprite, primaryPortal, secondaryPortal);

            CreateTutorialScene(playerPrefab, blockPrefab, exitPrefab, anchorSprite);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateFolders()
        {
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Greybox");
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Greybox");
            EnsureFolder("Assets/Editor");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static Sprite CreateSpriteAsset(string path, Color32 color)
        {
            if (!File.Exists(path))
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, color);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Portal CreatePortalPrefab(string path, Sprite sprite, bool primary)
        {
            var root = new GameObject(primary ? "PortalPrimary" : "PortalSecondary");
            root.layer = PortalLayer;
            root.transform.localScale = new Vector3(0.18f, 2.1f, 1f);

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 10;

            var collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var portal = root.AddComponent<Portal>();
            SetPrivate(portal, "primary", primary);
            SetPrivate(portal, "exitOffset", 1.1f);
            SetPrivate(portal, "teleportCooldown", 0.22f);
            SetPrivate(portal, "minExitSpeed", 4f);
            SetPrivate(portal, "exitClearancePadding", 0.2f);
            SetPrivate(portal, "maxExitSpeed", 16f);
            SetPrivate(portal, "maxDownwardExitSpeed", 10f);

            var prefab = SavePrefab(root, path).GetComponent<Portal>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateBlockPrefab(Sprite sprite)
        {
            var root = new GameObject("GreyboxBlock");
            root.layer = SolidLayer;
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            var prefab = SavePrefab(root, BlockPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateExitPrefab(Sprite sprite)
        {
            var root = new GameObject("LevelExit");
            root.layer = SolidLayer;
            root.transform.localScale = new Vector3(1.2f, 2.4f, 1f);

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 4;

            var collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            root.AddComponent<LevelExit>();
            var prefab = SavePrefab(root, LevelExitPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(Sprite sprite, Portal primaryPortal, Portal secondaryPortal)
        {
            var root = new GameObject("Player");
            root.tag = "Player";
            root.layer = PlayerLayer;
            root.transform.localScale = new Vector3(0.85f, 1.55f, 1f);

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 6;

            var body = root.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(root.transform);
            groundCheck.transform.localPosition = new Vector3(0f, -0.55f, 0f);

            var fireOrigin = new GameObject("FireOrigin");
            fireOrigin.transform.SetParent(root.transform);
            fireOrigin.transform.localPosition = new Vector3(0.42f, 0.08f, 0f);

            var controller = root.AddComponent<PlayerController>();
            SetPrivate(controller, "groundCheck", groundCheck.transform);
            SetPrivate(controller, "groundMask", (LayerMask)(1 << SolidLayer));

            var validator = root.AddComponent<PortalPlacementValidator>();
            SetPrivate(validator, "portalAnchorMask", (LayerMask)(1 << PortalAnchorLayer));
            SetPrivate(validator, "placementBlockingMask", (LayerMask)(1 << SolidLayer));
            SetPrivate(validator, "portalOverlapMask", (LayerMask)(1 << PortalLayer));
            SetPrivate(validator, "drawDebug", true);

            var gun = root.AddComponent<PortalGun>();
            SetPrivate(gun, "fireOrigin", fireOrigin.transform);
            SetPrivate(gun, "player", controller);
            SetPrivate(gun, "primaryPortalPrefab", primaryPortal);
            SetPrivate(gun, "secondaryPortalPrefab", secondaryPortal);

            var prefab = SavePrefab(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateTutorialScene(GameObject playerPrefab, GameObject blockPrefab, GameObject exitPrefab, Sprite anchorSprite)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level_01_Tutorial";

            var levelManagerObject = new GameObject("LevelManager");
            levelManagerObject.AddComponent<LevelManager>();
            var completeOverlay = levelManagerObject.AddComponent<LevelCompleteOverlay>();

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = new Vector3(-6f, -1.25f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.15f;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            cameraObject.transform.position = new Vector3(1f, 1f, -10f);
            var follow = cameraObject.AddComponent<SimpleCameraFollow>();
            follow.SetTarget(player.transform);
            follow.Configure(0.08f, new Vector2(-8f, -1.5f), new Vector2(16f, 5f));

            var debugObject = new GameObject("PortalDebugOverlay");
            var overlay = debugObject.AddComponent<PortalDebugOverlay>();
            SetPrivate(overlay, "player", player.GetComponent<PlayerController>());
            SetPrivate(overlay, "portalGun", player.GetComponent<PortalGun>());

            var exit = (GameObject)PrefabUtility.InstantiatePrefab(exitPrefab);
            exit.transform.position = new Vector3(15.8f, -0.8f, 0f);
            var trigger = exit.GetComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            var exitTrigger = exit.GetComponent<LevelExit>();
            SetPrivate(exitTrigger, "loadNextLevel", false);
            SetPrivate(exitTrigger, "completeOverlay", completeOverlay);

            CreateBlock(blockPrefab, "StartFloor", new Vector2(-5f, -2.5f), new Vector2(7f, 1f));
            CreateBlock(blockPrefab, "StartBackWall", new Vector2(-8.5f, 0f), new Vector2(0.8f, 5f));
            CreateBlock(blockPrefab, "ExitFloor", new Vector2(11f, -2.5f), new Vector2(10f, 1f));
            CreateBlock(blockPrefab, "ExitBackWall", new Vector2(16.5f, 0f), new Vector2(0.8f, 5f));
            CreateBlock(blockPrefab, "Ceiling", new Vector2(4f, 5f), new Vector2(24f, 1f));
            CreateBlock(blockPrefab, "HighMomentumDrop", new Vector2(-6f, 2.6f), new Vector2(2.5f, 0.6f));
            CreateAnchor("StartAnchor", anchorSprite, new Vector2(-8.02f, -0.55f), Vector2.right);
            CreateAnchor("ExitAnchor", anchorSprite, new Vector2(16.02f, -0.55f), Vector2.left);
            CreateAnchor("CeilingAnchor", anchorSprite, new Vector2(-6f, 2.22f), Vector2.down);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }

        private static void CreateBlock(GameObject prefab, string name, Vector2 position, Vector2 scale)
        {
            var block = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        }

        private static void CreateAnchor(string name, Sprite sprite, Vector2 position, Vector2 normal)
        {
            var anchorObject = new GameObject(name);
            anchorObject.layer = PortalAnchorLayer;
            anchorObject.transform.position = position;
            anchorObject.transform.localScale = new Vector3(0.55f, 1.7f, 1f);

            var renderer = anchorObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;

            var collider = anchorObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var anchor = anchorObject.AddComponent<PortalAnchor>();
            anchor.Configure(normal, true, true);
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            return PrefabUtility.SaveAsPrefabAsset(root, path);
        }

        private static void SetPrivate(Object target, string fieldName, object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                throw new System.MissingFieldException($"Missing serialized field {fieldName} on {target.name}.");
            }

            switch (value)
            {
                case bool boolValue:
                    property.boolValue = boolValue;
                    break;
                case float floatValue:
                    property.floatValue = floatValue;
                    break;
                case int intValue:
                    property.intValue = intValue;
                    break;
                case LayerMask layerMask:
                    property.intValue = layerMask.value;
                    break;
                case Object objectValue:
                    property.objectReferenceValue = objectValue;
                    break;
                default:
                    throw new System.NotSupportedException($"Unsupported serialized value type {value.GetType()}.");
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

}
