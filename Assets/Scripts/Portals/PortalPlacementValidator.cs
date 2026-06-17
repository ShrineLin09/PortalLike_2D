using SidePortal.Configuration;
using UnityEngine;

namespace SidePortal.Portals
{
    public sealed class PortalPlacementValidator : MonoBehaviour
    {
        [SerializeField] private LayerMask portalAnchorMask;
        [SerializeField] private LayerMask placementBlockingMask;
        [SerializeField] private LayerMask portalOverlapMask;
        [SerializeField] private Vector2 portalSize = PrototypeTuning.PortalPlacementSize;
        [SerializeField] private Vector2 exitClearanceSize = PrototypeTuning.PlayerClearanceSize;
        [SerializeField] private float maxPlaceDistance = 16f;
        [SerializeField] private float surfaceOffset = 0.08f;
        [SerializeField] private bool drawDebug;

        public PortalPlacementResult LastResult { get; private set; } =
            PortalPlacementResult.Failed(PortalPlacementFailure.NoValidAnchorHit, "尚未尝试放置传送门。");

        public void ConfigureMasks(LayerMask anchorMask, LayerMask blockingMask, LayerMask overlapMask)
        {
            portalAnchorMask = anchorMask;
            placementBlockingMask = blockingMask;
            portalOverlapMask = overlapMask;
            ApplyDefaultGeometry();
        }

        private void Awake()
        {
            ApplyDefaultGeometry();
        }

        private void OnValidate()
        {
            ApplyDefaultGeometry();
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
                return Store(PortalPlacementResult.Failed(PortalPlacementFailure.NoValidAnchorHit, "没有命中有效的传送门锚点。"));
            }

            if (!hit.collider.TryGetComponent<PortalAnchor>(out var anchor))
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    Vector2.zero,
                    hit.point,
                    PortalPlacementFailure.NoValidAnchorHit,
                    "命中的碰撞体没有传送门锚点组件。"));
            }

            if (!anchor.IsEnabled)
            {
                return Store(new PortalPlacementResult(
                    false,
                    hit.point,
                    anchor.Normal,
                    hit.point,
                    PortalPlacementFailure.AnchorDisabled,
                    "传送门锚点已禁用。",
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
                    primary ? "该锚点不允许放置蓝门。" : "该锚点不允许放置黄门。",
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
                    "传送门本体会与阻挡物重叠。",
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
                    "传送门会与已有传送门重叠。",
                    anchor.name));
            }

            var exitPosition = position + normal * (exitClearanceSize.x * 0.5f + surfaceOffset);
            if (HasBlockingOverlap(exitPosition, exitClearanceSize, 0f, placementBlockingMask, null))
            {
                return Store(new PortalPlacementResult(
                    false,
                    position,
                    normal,
                    hit.point,
                    PortalPlacementFailure.BlockedExitClearance,
                    "传送门出口空间被阻挡。",
                    anchor.name));
            }

            return Store(new PortalPlacementResult(true, position, normal, hit.point, PortalPlacementFailure.None, "传送门放置成功。", anchor.name));
        }

        private PortalPlacementResult Store(PortalPlacementResult result)
        {
            LastResult = result;
            return result;
        }

        private void ApplyDefaultGeometry()
        {
            portalSize = PrototypeTuning.PortalPlacementSize;
            exitClearanceSize = PrototypeTuning.PlayerClearanceSize;
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
