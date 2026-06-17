using SidePortal.Core;
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
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0))
            {
                Fire(true);
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1))
            {
                Fire(false);
            }
        }

        public bool Fire(bool primary)
        {
            var origin = fireOrigin != null ? (Vector2)fireOrigin.position : (Vector2)transform.position;
            var aim = player != null ? player.AimDirection : AimDirection.Default;
            var result = placementValidator.TryFindPlacement(origin, aim);

            if (!result.Success)
            {
                return false;
            }

            PlacePortal(primary, result.Position, result.Normal);
            return true;
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
            return portal;
        }
    }
}
