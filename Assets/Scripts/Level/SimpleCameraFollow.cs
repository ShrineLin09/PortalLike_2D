using SidePortal.Configuration;
using UnityEngine;

namespace SidePortal.Level
{
    public sealed class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private CameraViewConfig view = CameraViewConfig.Default;
        [SerializeField] private Vector2 minBounds = new Vector2(-8f, -2f);
        [SerializeField] private Vector2 maxBounds = new Vector2(18f, 7f);

        private Vector3 velocity;

        public CameraViewConfig View => view;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void Configure(float followSmoothTime, Vector2 minimumBounds, Vector2 maximumBounds)
        {
            var defaultView = CameraViewConfig.Default;
            defaultView.FollowSmoothTime = followSmoothTime;
            Configure(defaultView, minimumBounds, maximumBounds);
        }

        public void Configure(CameraViewConfig viewConfig, Vector2 minimumBounds, Vector2 maximumBounds)
        {
            view = PrototypeTuning.EnsureCameraView(viewConfig);
            minBounds = minimumBounds;
            maxBounds = maximumBounds;
        }

        private void Awake()
        {
            view = PrototypeTuning.EnsureCameraView(view);
        }

        private void OnValidate()
        {
            view = PrototypeTuning.EnsureCameraView(view);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + view.FollowOffset;
            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, view.FollowSmoothTime);
        }
    }
}
