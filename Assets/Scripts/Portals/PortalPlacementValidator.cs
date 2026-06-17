using UnityEngine;

namespace SidePortal.Portals
{
    public sealed class PortalPlacementValidator : MonoBehaviour
    {
        [SerializeField] private LayerMask portalAnchorMask;
        [SerializeField] private LayerMask placementBlockingMask;
        [SerializeField] private LayerMask portalOverlapMask;
        [SerializeField] private Vector2 portalSize = new Vector2(0.18f, 2.1f);
        [SerializeField] private Vector2 exitClearanceSize = new Vector2(0.85f, 1.8f);
        [SerializeField] private float maxPlaceDistance = 16f;
        [SerializeField] private float surfaceOffset = 0.08f;
        [SerializeField] private bool drawDebug;

        public PortalPlacementResult LastResult { get; private set; } =
            PortalPlacementResult.Failed(PortalPlacementFailure.NoValidAnchorHit, "No placement attempted.");

        public void ConfigureMasks(LayerMask anchorMask, LayerMask blockingMask, LayerMask overlapMask)
        {
            portalAnchorMask = anchorMask;
            placementBlockingMask = blockingMask;
            portalOverlapMask = overlapMask;
        }

        public void SetExternalFailure(string message)
        {
            LastResult = PortalPlacementResult.Failed(PortalPlacementFailure.NoValidAnchorHit, message);
        }

        public PortalPlacementResult TryFindPlacement(Vector2 origin, Vector2 aimDirection, bool primary)
        {
            var aim = aimDirection.sqrMagnitude < 0.01f ? Vector2.right : aimDirection.normalized;
            var hit = Physics2D.Raycast(origin, aim, maxPlaceDistance, portalAnchorMask);

            if (hit.collider == null)
            {
                return Store(PortalPlacementResult.Failed(PortalPlacementFailure.NoValidAnchorHit, "No valid portal anchor hit."));
            }

            if (!hit.collider.TryGetComponent<PortalAnchor>(out var anchor))
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    Vector2.zero,
                    hit.point,
                    PortalPlacementFailure.NoValidAnchorHit,
                    "Hit collider has no PortalAnchor."));
            }

            if (!anchor.IsEnabled)
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    anchor.Normal,
                    hit.point,
                    PortalPlacementFailure.AnchorDisabled,
                    "Portal anchor is disabled.",
                    anchor.name));
            }

            if (!anchor.AllowsPortalType(primary))
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    anchor.Normal,
                    hit.point,
                    PortalPlacementFailure.PortalTypeNotAllowed,
                    primary ? "Anchor does not allow blue portals." : "Anchor does not allow yellow portals.",
                    anchor.name));
            }

            var normal = anchor.Normal;
            var position = (Vector2)anchor.PortalMount.position + normal * surfaceOffset;
            var angle = NormalToAngle(normal);

            if (HasBlockingOverlap(position, portalSize, angle, placementBlockingMask, null))
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.BlockedPortalSpace,
                    "Portal body would overlap blocking geometry.",
                    anchor.name));
            }

            if (Physics2D.OverlapBox(position, portalSize, angle, portalOverlapMask) != null)
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.OverlappingPortal,
                    "Portal would overlap another portal.",
                    anchor.name));
            }

            var exitPosition = position + normal * (exitClearanceSize.x * 0.5f + surfaceOffset);
            if (HasBlockingOverlap(exitPosition, exitClearanceSize, angle, placementBlockingMask, null))
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.BlockedExitClearance,
                    "Exit clearance is blocked.",
                    anchor.name));
            }

            return Store(new PortalPlacementResult(true, position, normal, hit.point, PortalPlacementFailure.None, "Placement accepted.", anchor.name));
        }

        private PortalPlacementResult Store(PortalPlacementResult result)
        {
            LastResult = result;
            return result;
        }

        private static float NormalToAngle(Vector2 normal)
        {
            return Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        }

        private static bool HasBlockingOverlap(
            Vector2 position,
            Vector2 size,
            float angle,
            LayerMask mask,
            Collider2D ignoredCollider)
        {
            var hits = Physics2D.OverlapBoxAll(position, size, angle, mask);
            foreach (var overlap in hits)
            {
                if (overlap != null && overlap != ignoredCollider)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            if (!drawDebug || !LastResult.Success)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(LastResult.Position, portalSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(LastResult.Position, LastResult.Normal);
        }
    }
}
