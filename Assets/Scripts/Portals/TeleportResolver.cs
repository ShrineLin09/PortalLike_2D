using UnityEngine;

namespace SidePortal.Portals
{
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

        public static Vector2 ExitPosition(Vector2 exitPortalPosition, Vector2 exitNormal, float exitOffset)
        {
            return exitPortalPosition + exitNormal.normalized * exitOffset;
        }

        public static Vector2 ClampExitVelocity(Vector2 velocity, float maxExitSpeed, float maxDownwardExitSpeed)
        {
            if (maxExitSpeed > 0f && velocity.magnitude > maxExitSpeed)
            {
                velocity = velocity.normalized * maxExitSpeed;
            }

            if (maxDownwardExitSpeed > 0f && velocity.y < -maxDownwardExitSpeed)
            {
                velocity.y = -maxDownwardExitSpeed;
            }

            return velocity;
        }

        private static Vector2 Perpendicular(Vector2 value)
        {
            return new Vector2(-value.y, value.x);
        }
    }
}
