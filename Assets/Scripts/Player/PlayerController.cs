using UnityEngine;

namespace SidePortal.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpImpulse = 11.5f;
        [SerializeField] private float risingGravityScale = 2.8f;
        [SerializeField] private float fallingGravityScale = 5.2f;
        [SerializeField] private float maxFallSpeed = 18f;
        [SerializeField] private float jumpBufferTime = 0.12f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpCutMultiplier = 0.45f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.7f, 0.08f);
        [SerializeField] private LayerMask groundMask;

        private Rigidbody2D body;
        private float jumpBufferCounter;
        private float coyoteCounter;
        private bool jumpCutQueued;

        public bool IsGrounded { get; private set; }

        public void ConfigureGroundCheck(Transform check, LayerMask mask)
        {
            groundCheck = check;
            groundMask = mask;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space))
            {
                jumpCutQueued = true;
            }
        }

        private void FixedUpdate()
        {
            IsGrounded = CheckGrounded();
            coyoteCounter = IsGrounded ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;

            var velocity = body.velocity;
            velocity.x = Input.GetAxisRaw("Horizontal") * moveSpeed;

            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                velocity.y = jumpImpulse;
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            if (jumpCutQueued && velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
            }

            body.gravityScale = velocity.y > 0f ? risingGravityScale : fallingGravityScale;
            if (velocity.y < -maxFallSpeed)
            {
                velocity.y = -maxFallSpeed;
            }

            body.velocity = velocity;
            jumpCutQueued = false;
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
