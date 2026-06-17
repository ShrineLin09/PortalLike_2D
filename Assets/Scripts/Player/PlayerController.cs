using SidePortal.Configuration;
using UnityEngine;

namespace SidePortal.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerPhysicsConfig physics = PlayerPhysicsConfig.Default;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody2D body;
        private float jumpBufferCounter;
        private float coyoteCounter;
        private bool jumpCutQueued;

        public bool IsGrounded { get; private set; }
        public PlayerPhysicsConfig Physics => physics;

        public void ConfigurePhysics(PlayerPhysicsConfig config)
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(config);
        }

        public void ConfigureGroundCheck(Transform check, LayerMask mask)
        {
            groundCheck = check;
            groundMask = mask;
        }

        private void Awake()
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(physics);
            body = GetComponent<Rigidbody2D>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnValidate()
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(physics);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferCounter = physics.JumpBufferTime;
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
            coyoteCounter = IsGrounded ? physics.CoyoteTime : coyoteCounter - Time.fixedDeltaTime;

            var velocity = body.velocity;
            velocity.x = Input.GetAxisRaw("Horizontal") * physics.MoveSpeed;

            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                velocity.y = physics.JumpImpulse;
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            if (jumpCutQueued && velocity.y > 0f)
            {
                velocity.y *= physics.JumpCutMultiplier;
            }

            body.gravityScale = velocity.y > 0f ? physics.RisingGravityScale : physics.FallingGravityScale;
            if (velocity.y < -physics.MaxFallSpeed)
            {
                velocity.y = -physics.MaxFallSpeed;
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

            return Physics2D.OverlapBox(groundCheck.position, physics.GroundCheckSize, 0f, groundMask) != null;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, physics.GroundCheckSize);
        }
    }
}
