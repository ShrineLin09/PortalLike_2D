using UnityEngine;

namespace SidePortal.Portals
{
    public sealed class PortalPlacementValidator : MonoBehaviour
    {
        [SerializeField] private LayerMask portalSurfaceMask;
        [SerializeField] private LayerMask placementBlockingMask;
        [SerializeField] private LayerMask portalOverlapMask;
        [SerializeField] private Vector2 portalSize = new Vector2(0.18f, 2.1f);
        [SerializeField] private Vector2 exitClearanceSize = new Vector2(0.85f, 1.8f);
        [SerializeField] private float maxPlaceDistance = 16f;
        [SerializeField] private float surfaceOffset = 0.08f;
        [SerializeField, Range(0f, 1f)] private float minOpposingNormalDot = 0.85f;
        [SerializeField] private bool drawDebug;

        public PortalPlacementResult LastResult { get; private set; } =
            PortalPlacementResult.Failed(PortalPlacementFailure.NoSurfaceHit, "No placement attempted.");

        public PortalPlacementResult TryFindPlacement(Vector2 origin, Vector2 aimDirection)
        {
            var aim = aimDirection.sqrMagnitude < 0.01f ? Vector2.right : aimDirection.normalized;
            var hit = Physics2D.Raycast(origin, aim, maxPlaceDistance, portalSurfaceMask);

            if (hit.collider == null)
            {
                return Store(PortalPlacementResult.Failed(PortalPlacementFailure.NoSurfaceHit, "No portal surface hit."));
            }

            if (Vector2.Dot(hit.normal, -aim) < minOpposingNormalDot)
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    hit.normal,
                    hit.point,
                    PortalPlacementFailure.SurfaceFacingWrongWay,
                    "Surface normal is not aligned with the four-way aim direction."));
            }

            var normal = CardinalNormal(hit.normal);
            var position = hit.point + normal * surfaceOffset;
            var angle = NormalToAngle(normal);

            if (HasBlockingOverlap(position, portalSize, angle, placementBlockingMask, hit.collider))
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.BlockedPortalSpace,
                    "Portal body would overlap blocking geometry."));
            }

            if (Physics2D.OverlapBox(position, portalSize, angle, portalOverlapMask) != null)
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.OverlappingPortal,
                    "Portal would overlap another portal."));
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
                    "Exit clearance is blocked."));
            }

            return Store(new PortalPlacementResult(true, position, normal, hit.point, PortalPlacementFailure.None, "Placement accepted."));
        }

        private PortalPlacementResult Store(PortalPlacementResult result)
        {
            LastResult = result;
            return result;
        }

        private static Vector2 CardinalNormal(Vector2 normal)
        {
            return Mathf.Abs(normal.x) >= Mathf.Abs(normal.y)
                ? new Vector2(Mathf.Sign(normal.x), 0f)
                : new Vector2(0f, Mathf.Sign(normal.y));
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
