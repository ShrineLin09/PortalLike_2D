using UnityEngine;

namespace SidePortal.Portals
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Portal : MonoBehaviour
    {
        [SerializeField] private bool primary;
        [SerializeField] private Portal linkedPortal;
        [SerializeField] private float exitOffset = 0.85f;
        [SerializeField] private float teleportCooldown = 0.18f;
        [SerializeField] private float minExitSpeed = 3f;

        private float lastTeleportTime = -999f;

        public bool IsPrimary => primary;
        public Portal LinkedPortal
        {
            get => linkedPortal;
            set => linkedPortal = value;
        }

        public Vector2 ExitNormal => transform.right;

        public void Configure(bool isPrimary, Portal link, Vector2 position, Vector2 normal)
        {
            primary = isPrimary;
            linkedPortal = link;
            transform.position = position;
            transform.right = normal;
        }

        private void Reset()
        {
            var trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (linkedPortal == null || Time.time < lastTeleportTime + teleportCooldown)
            {
                return;
            }

            if (!other.TryGetComponent<Rigidbody2D>(out var body))
            {
                return;
            }

            Teleport(body);
        }

        private void Teleport(Rigidbody2D body)
        {
            var exitPosition = TeleportResolver.ExitPosition(linkedPortal.transform.position, linkedPortal.ExitNormal, exitOffset);
            var exitVelocity = TeleportResolver.RemapVelocity(body.velocity, ExitNormal, linkedPortal.ExitNormal, minExitSpeed);

            body.position = exitPosition;
            body.velocity = exitVelocity;

            lastTeleportTime = Time.time;
            linkedPortal.lastTeleportTime = Time.time;
        }
    }
}
