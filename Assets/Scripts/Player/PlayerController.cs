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
        private float temporaryMaxFallSpeed;
        private float temporaryMaxFallSpeedUntil;

        public bool IsGrounded { get; private set; }
        public PlayerPhysicsConfig Physics => physics;

        public void ConfigurePhysics(PlayerPhysicsConfig config)
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(config);
            ApplyDefaultScale();
        }

        public void ConfigureGroundCheck(Transform check, LayerMask mask)
        {
            groundCheck = check;
            groundMask = mask;
        }

        public void AllowTemporaryFallSpeed(float maxSpeed, float duration)
        {
            if (maxSpeed <= physics.MaxFallSpeed || duration <= 0f)
            {
                return;
            }

            temporaryMaxFallSpeed = Mathf.Max(temporaryMaxFallSpeed, maxSpeed);
            temporaryMaxFallSpeedUntil = Mathf.Max(temporaryMaxFallSpeedUntil, Time.time + duration);
        }

        private void Awake()
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(physics);
            ApplyDefaultScale();
            body = GetComponent<Rigidbody2D>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            ApplyDefaultScale();
        }

        private void OnValidate()
        {
            physics = PrototypeTuning.EnsurePlayerPhysics(physics);
            ApplyDefaultScale();
        }

        private void Reset()
        {
            physics = PrototypeTuning.PlayerPhysics;
            ApplyDefaultScale();
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
            velocity.x = ResolveHorizontalVelocity(velocity.x, Input.GetAxisRaw("Horizontal"), physics.MoveSpeed);

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
            var fallSpeedLimit = CurrentMaxFallSpeed();
            if (velocity.y < -fallSpeedLimit)
            {
                velocity.y = -fallSpeedLimit;
            }

            body.velocity = velocity;
            jumpCutQueued = false;
        }

        public static float ResolveHorizontalVelocity(float currentVelocityX, float rawInput, float moveSpeed)
        {
            var input = Mathf.Clamp(rawInput, -1f, 1f);
            var targetWalkVelocity = input * moveSpeed;
            var currentSpeed = Mathf.Abs(currentVelocityX);

            if (currentSpeed > moveSpeed)
            {
                if (Mathf.Approximately(input, 0f) || Mathf.Sign(input) == Mathf.Sign(currentVelocityX))
                {
                    return currentVelocityX;
                }
            }

            return targetWalkVelocity;
        }

        private bool CheckGrounded()
        {
            if (groundCheck == null)
            {
                return false;
            }

            return Physics2D.OverlapBox(groundCheck.position, physics.GroundCheckSize, 0f, groundMask) != null;
        }

        private float CurrentMaxFallSpeed()
        {
            if (Time.time <= temporaryMaxFallSpeedUntil)
            {
                return Mathf.Max(physics.MaxFallSpeed, temporaryMaxFallSpeed);
            }

            temporaryMaxFallSpeed = 0f;
            return physics.MaxFallSpeed;
        }

        private void ApplyDefaultScale()
        {
            var scale = PrototypeTuning.LevelDesignScale;
            transform.localScale = new Vector3(scale.PlayerWidth, scale.PlayerHeight, 1f);
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
