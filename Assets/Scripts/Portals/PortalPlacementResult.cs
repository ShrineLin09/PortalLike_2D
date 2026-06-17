using UnityEngine;

namespace SidePortal.Portals
{
    public enum PortalPlacementFailure
    {
        None,
        NoSurfaceHit,
        SurfaceFacingWrongWay,
        BlockedPortalSpace,
        OverlappingPortal,
        BlockedExitClearance
    }

    public readonly struct PortalPlacementResult
    {
        public PortalPlacementResult(
            bool success,
            Vector2 position,
            Vector2 normal,
            Vector2 hitPoint,
            PortalPlacementFailure failure,
            string message)
        {
            Success = success;
            Position = position;
            Normal = normal;
            HitPoint = hitPoint;
            Failure = failure;
            Message = message;
        }

        public bool Success { get; }
        public Vector2 Position { get; }
        public Vector2 Normal { get; }
        public Vector2 HitPoint { get; }
        public PortalPlacementFailure Failure { get; }
        public string Message { get; }

        public static PortalPlacementResult Failed(PortalPlacementFailure failure, string message)
        {
            return new PortalPlacementResult(false, Vector2.zero, Vector2.zero, Vector2.zero, failure, message);
        }
    }
}
