using SidePortal.Configuration;
using UnityEngine;

namespace SidePortal.Portals
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PortalAnchor : MonoBehaviour
    {
        [SerializeField] private Vector2 normal = Vector2.right;
        [SerializeField] private bool allowPrimary = true;
        [SerializeField] private bool allowSecondary = true;
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private Transform portalMount;

        public Vector2 Normal => normal.sqrMagnitude < 0.01f ? Vector2.right : normal.normalized;
        public bool AllowPrimary => allowPrimary;
        public bool AllowSecondary => allowSecondary;
        public bool IsEnabled => isEnabled;
        public Transform PortalMount => portalMount != null ? portalMount : transform;

        public void Configure(Vector2 anchorNormal, bool primaryAllowed, bool secondaryAllowed, bool enabledAnchor = true)
        {
            normal = anchorNormal.sqrMagnitude < 0.01f ? Vector2.right : anchorNormal.normalized;
            allowPrimary = primaryAllowed;
            allowSecondary = secondaryAllowed;
            isEnabled = enabledAnchor;
            portalMount = transform;
            ApplyDefaultGeometry();
        }

        public bool AllowsPortalType(bool primary)
        {
            return primary ? allowPrimary : allowSecondary;
        }

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
            ApplyDefaultGeometry();
        }

        private void Awake()
        {
            ApplyDefaultGeometry();
        }

        private void OnEnable()
        {
            ApplyDefaultGeometry();
        }

        private void OnValidate()
        {
            ApplyDefaultGeometry();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isEnabled ? new Color(0f, 0.8f, 1f, 0.65f) : new Color(0.5f, 0.5f, 0.5f, 0.35f);
            var verticalSurface = Mathf.Abs(Normal.x) > 0.5f;
            Gizmos.DrawWireCube(PortalMount.position, PrototypeTuning.PortalAnchorScale(verticalSurface, 1f));
            Gizmos.DrawRay(PortalMount.position, Normal);
        }

        private void ApplyDefaultGeometry()
        {
            var verticalSurface = Mathf.Abs(Normal.x) > 0.5f;
            transform.localScale = PrototypeTuning.PortalAnchorScale(verticalSurface, 1f);
            if (TryGetComponent<BoxCollider2D>(out var box))
            {
                box.isTrigger = true;
                box.size = Vector2.one;
            }
        }
    }
}
