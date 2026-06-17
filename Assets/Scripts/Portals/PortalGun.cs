using SidePortal.Player;
using UnityEngine;

namespace SidePortal.Portals
{
    [RequireComponent(typeof(PortalPlacementValidator))]
    public sealed class PortalGun : MonoBehaviour
    {
        [SerializeField] private Transform fireOrigin;
        [SerializeField] private PlayerController player;
        [SerializeField] private Portal primaryPortalPrefab;
        [SerializeField] private Portal secondaryPortalPrefab;

        private PortalPlacementValidator placementValidator;
        private Portal primaryPortal;
        private Portal secondaryPortal;

        public PortalPlacementResult LastPlacementResult => placementValidator.LastResult;

        public void Configure(Transform origin, PlayerController controller, Portal primaryPrefab, Portal secondaryPrefab)
        {
            fireOrigin = origin;
            player = controller;
            primaryPortalPrefab = primaryPrefab;
            secondaryPortalPrefab = secondaryPrefab;
        }

        private void Awake()
        {
            placementValidator = GetComponent<PortalPlacementValidator>();
            if (player == null)
            {
                player = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fire(true);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Fire(false);
            }
        }

        public bool Fire(bool primary)
        {
            if ((primary && primaryPortalPrefab == null) || (!primary && secondaryPortalPrefab == null))
            {
                placementValidator.SetExternalFailure(primary
                    ? "蓝门预制体未绑定。"
                    : "黄门预制体未绑定。");
                return false;
            }

            var origin = fireOrigin != null ? (Vector2)fireOrigin.position : (Vector2)transform.position;
            var aim = MouseAimDirection(origin);
            var result = placementValidator.TryFindPlacement(origin, aim, primary);

            if (!result.Success)
            {
                return false;
            }

            PlacePortal(primary, result.Position, result.Normal);
            return true;
        }

        public Vector2 CurrentMouseAimDirection
        {
            get
            {
                var origin = fireOrigin != null ? (Vector2)fireOrigin.position : (Vector2)transform.position;
                return MouseAimDirection(origin);
            }
        }

        private static Vector2 MouseAimDirection(Vector2 origin)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return Vector2.right;
            }

            var mouse = Input.mousePosition;
            var mouseWorld = camera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -camera.transform.position.z));
            var direction = (Vector2)mouseWorld - origin;
            return direction.sqrMagnitude < 0.01f ? Vector2.right : direction.normalized;
        }

        private void PlacePortal(bool placePrimary, Vector2 position, Vector2 normal)
        {
            if (placePrimary)
            {
                primaryPortal = CreateOrMove(primaryPortal, primaryPortalPrefab, true, position, normal);
            }
            else
            {
                secondaryPortal = CreateOrMove(secondaryPortal, secondaryPortalPrefab, false, position, normal);
            }

            if (primaryPortal != null && secondaryPortal != null)
            {
                primaryPortal.LinkedPortal = secondaryPortal;
                secondaryPortal.LinkedPortal = primaryPortal;
            }
        }

        private static Portal CreateOrMove(Portal existing, Portal prefab, bool isPrimary, Vector2 position, Vector2 normal)
        {
            var portal = existing != null ? existing : Instantiate(prefab);
            portal.Configure(isPrimary, null, position, normal);
            portal.gameObject.SetActive(true);
            return portal;
        }
    }
}
