using UnityEngine;

namespace SidePortal.Core
{
    public static class AimDirection
    {
        public static readonly Vector2 Default = Vector2.right;

        public static Vector2 FromInput(float horizontal, float vertical, Vector2 fallback)
        {
            var input = new Vector2(horizontal, vertical);
            if (input.sqrMagnitude < 0.01f)
            {
                return ToCardinal(fallback.sqrMagnitude < 0.01f ? Default : fallback);
            }

            return ToCardinal(input);
        }

        public static Vector2 ToCardinal(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
            {
                return Default;
            }

            return Mathf.Abs(direction.x) >= Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));
        }
    }
}
