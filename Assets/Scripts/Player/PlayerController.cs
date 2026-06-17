using SidePortal.Core;
using UnityEngine;

namespace SidePortal.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpImpulse = 13f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.08f);
        [SerializeField] private LayerMask groundMask;

        private Rigidbody2D body;
        private Vector2 aimDirection = AimDirection.Default;
        private bool jumpQueued;

        public Vector2 AimDirection => aimDirection;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            aimDirection = AimDirection.FromInput(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), aimDirection);

            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                jumpQueued = true;
            }
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();

            var velocity = body.velocity;
            velocity.x = Input.GetAxisRaw("Horizontal") * moveSpeed;

            if (jumpQueued && IsGrounded)
            {
                velocity.y = jumpImpulse;
            }

            body.velocity = velocity;
            jumpQueued = false;
        }

        private bool CheckGrounded()
        {
            if (groundCheck == null)
            {
                return false;
            }

            return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask) != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
