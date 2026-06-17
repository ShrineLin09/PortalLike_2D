using UnityEngine;

namespace SidePortal.Portals
{
    public enum VelocityClampType
    {
        None,
        MaxExitSpeed,
        MaxDownwardExitSpeed,
        MaxExitSpeedAndDownwardExitSpeed
    }

    public readonly struct VelocityClampResult
    {
        public VelocityClampResult(Vector2 velocity, VelocityClampType clampType)
        {
            Velocity = velocity;
            ClampType = clampType;
        }

        public Vector2 Velocity { get; }
        public VelocityClampType ClampType { get; }
        public bool WasClamped => ClampType != VelocityClampType.None;
    }

    public static class TeleportResolver
    {
        public static Vector2 RemapVelocity(Vector2 velocity, Vector2 entryExitNormal, Vector2 exitNormal, float minExitSpeed)
        {
            var entryForward = -entryExitNormal.normalized;
            var entryTangent = Perpendicular(entryExitNormal).normalized;
            var exitForward = exitNormal.normalized;
            var exitTangent = Perpendicular(exitNormal).normalized;

            var forwardSpeed = Vector2.Dot(velocity, entryForward);
            var tangentSpeed = Vector2.Dot(velocity, entryTangent);
            var remapped = exitForward * forwardSpeed + exitTangent * tangentSpeed;

            if (remapped.magnitude < minExitSpeed)
            {
                remapped = exitForward * minExitSpeed;
            }

            return remapped;
        }

        public static Vector2 RemapPlayerVelocity(Vector2 velocity, Vector2 entryExitNormal, Vector2 exitNormal)
        {
            return RemapVelocity(velocity, entryExitNormal, exitNormal, 0f);
        }

        public static Vector2 ExitPosition(Vector2 exitPortalPosition, Vector2 exitNormal, float exitOffset)
        {
            return exitPortalPosition + exitNormal.normalized * exitOffset;
        }

        public static Vector2 ClampExitVelocity(Vector2 velocity, float maxExitSpeed, float maxDownwardExitSpeed)
        {
            return ClampExitVelocityDetailed(velocity, maxExitSpeed, maxDownwardExitSpeed).Velocity;
        }

        public static VelocityClampResult ClampExitVelocityDetailed(Vector2 velocity, float maxExitSpeed, float maxDownwardExitSpeed)
        {
            var clampedByMaxSpeed = false;
            var clampedByDownwardSpeed = false;

            if (maxExitSpeed > 0f && velocity.magnitude > maxExitSpeed)
            {
                velocity = velocity.normalized * maxExitSpeed;
                clampedByMaxSpeed = true;
            }

            if (maxDownwardExitSpeed > 0f && velocity.y < -maxDownwardExitSpeed)
            {
                velocity.y = -maxDownwardExitSpeed;
                clampedByDownwardSpeed = true;
            }

            var clampType = (clampedByMaxSpeed, clampedByDownwardSpeed) switch
            {
                (true, true) => VelocityClampType.MaxExitSpeedAndDownwardExitSpeed,
                (true, false) => VelocityClampType.MaxExitSpeed,
                (false, true) => VelocityClampType.MaxDownwardExitSpeed,
                _ => VelocityClampType.None
            };

            return new VelocityClampResult(velocity, clampType);
        }

        private static Vector2 Perpendicular(Vector2 value)
        {
            return new Vector2(-value.y, value.x);
        }
    }
}
