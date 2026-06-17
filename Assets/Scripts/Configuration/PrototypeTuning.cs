using System;
using UnityEngine;

namespace SidePortal.Configuration
{
    [Serializable]
    public struct PlayerPhysicsConfig
    {
        public float MoveSpeed;
        public float JumpImpulse;
        public float RisingGravityScale;
        public float FallingGravityScale;
        public float MaxFallSpeed;
        public float JumpBufferTime;
        public float CoyoteTime;
        public float JumpCutMultiplier;
        public Vector2 GroundCheckSize;

        public static PlayerPhysicsConfig Default => new PlayerPhysicsConfig
        {
            MoveSpeed = 8f,
            JumpImpulse = 11.5f,
            RisingGravityScale = 2.8f,
            FallingGravityScale = 5.2f,
            MaxFallSpeed = 18f,
            JumpBufferTime = 0.12f,
            CoyoteTime = 0.1f,
            JumpCutMultiplier = 0.45f,
            GroundCheckSize = new Vector2(0.7f, 0.08f)
        };
    }

    [Serializable]
    public struct PortalMomentumConfig
    {
        public float ExitOffset;
        public float TeleportCooldown;
        public float MinExitSpeed;
        public float ExitClearancePadding;
        public float MaxExitSpeed;
        public float MaxDownwardExitSpeed;

        public static PortalMomentumConfig Default => new PortalMomentumConfig
        {
            ExitOffset = 1.1f,
            TeleportCooldown = 0.22f,
            MinExitSpeed = 4f,
            ExitClearancePadding = 0.2f,
            MaxExitSpeed = 36f,
            MaxDownwardExitSpeed = 36f
        };
    }

    [Serializable]
    public struct CameraViewConfig
    {
        public float OrthographicSize;
        public float FollowSmoothTime;
        public Vector3 FollowOffset;
        public int ReferenceWidth;
        public int ReferenceHeight;

        public static CameraViewConfig Default => new CameraViewConfig
        {
            OrthographicSize = 4.15f,
            FollowSmoothTime = 0.08f,
            FollowOffset = new Vector3(0f, 1.5f, -10f),
            ReferenceWidth = 1280,
            ReferenceHeight = 720
        };
    }

    [Serializable]
    public struct LevelDesignScale
    {
        public float CellSize;
        public float PlayerHeight;
        public float PlayerWidth;
        public float StandardJumpHeight;
        public float StandardJumpDistance;
        public float MaximumSafeDrop;
        public float BaselineMomentumGap;

        public static LevelDesignScale Default => new LevelDesignScale
        {
            CellSize = 1f,
            PlayerHeight = 1.55f,
            PlayerWidth = 0.85f,
            StandardJumpHeight = 2.4f,
            StandardJumpDistance = 4.5f,
            MaximumSafeDrop = 6f,
            BaselineMomentumGap = 8f
        };
    }

    public static class PrototypeTuning
    {
        public static PlayerPhysicsConfig PlayerPhysics => PlayerPhysicsConfig.Default;
        public static PortalMomentumConfig PortalMomentum => PortalMomentumConfig.Default;
        public static CameraViewConfig CameraView => CameraViewConfig.Default;
        public static LevelDesignScale LevelDesignScale => LevelDesignScale.Default;

        public static PlayerPhysicsConfig EnsurePlayerPhysics(PlayerPhysicsConfig config)
        {
            var defaults = PlayerPhysicsConfig.Default;
            if (config.MoveSpeed <= 0f) config.MoveSpeed = defaults.MoveSpeed;
            if (config.JumpImpulse <= 0f) config.JumpImpulse = defaults.JumpImpulse;
            if (config.RisingGravityScale <= 0f) config.RisingGravityScale = defaults.RisingGravityScale;
            if (config.FallingGravityScale <= 0f) config.FallingGravityScale = defaults.FallingGravityScale;
            if (config.MaxFallSpeed <= 0f) config.MaxFallSpeed = defaults.MaxFallSpeed;
            if (config.JumpBufferTime <= 0f) config.JumpBufferTime = defaults.JumpBufferTime;
            if (config.CoyoteTime <= 0f) config.CoyoteTime = defaults.CoyoteTime;
            if (config.JumpCutMultiplier <= 0f) config.JumpCutMultiplier = defaults.JumpCutMultiplier;
            if (config.GroundCheckSize == Vector2.zero) config.GroundCheckSize = defaults.GroundCheckSize;
            return config;
        }

        public static PortalMomentumConfig EnsurePortalMomentum(PortalMomentumConfig config)
        {
            var defaults = PortalMomentumConfig.Default;
            if (config.ExitOffset <= 0f) config.ExitOffset = defaults.ExitOffset;
            if (config.TeleportCooldown <= 0f) config.TeleportCooldown = defaults.TeleportCooldown;
            if (config.MinExitSpeed <= 0f) config.MinExitSpeed = defaults.MinExitSpeed;
            if (config.ExitClearancePadding <= 0f) config.ExitClearancePadding = defaults.ExitClearancePadding;
            if (config.MaxExitSpeed <= 0f) config.MaxExitSpeed = defaults.MaxExitSpeed;
            if (config.MaxDownwardExitSpeed <= 0f) config.MaxDownwardExitSpeed = defaults.MaxDownwardExitSpeed;
            return config;
        }

        public static CameraViewConfig EnsureCameraView(CameraViewConfig config)
        {
            var defaults = CameraViewConfig.Default;
            if (config.OrthographicSize <= 0f) config.OrthographicSize = defaults.OrthographicSize;
            if (config.FollowSmoothTime <= 0f) config.FollowSmoothTime = defaults.FollowSmoothTime;
            if (config.FollowOffset == Vector3.zero) config.FollowOffset = defaults.FollowOffset;
            if (config.ReferenceWidth <= 0) config.ReferenceWidth = defaults.ReferenceWidth;
            if (config.ReferenceHeight <= 0) config.ReferenceHeight = defaults.ReferenceHeight;
            return config;
        }
    }
}
