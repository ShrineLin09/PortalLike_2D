using SidePortal.Player;
using SidePortal.Portals;
using UnityEngine;

namespace SidePortal.Debugging
{
    public sealed class PortalDebugOverlay : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PortalGun portalGun;
        [SerializeField] private bool visible = true;

        public void Configure(PlayerController targetPlayer, PortalGun targetPortalGun)
        {
            player = targetPlayer;
            portalGun = targetPortalGun;
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, 440f, 150f), GUI.skin.box);
            GUILayout.Label("Portal Prototype Debug");

            if (player != null)
            {
                GUILayout.Label($"Grounded: {player.IsGrounded}");
            }

            if (portalGun != null)
            {
                var result = portalGun.LastPlacementResult;
                GUILayout.Label($"Mouse aim: {portalGun.CurrentMouseAimDirection}");
                GUILayout.Label($"Last placement: {(result.Success ? "Accepted" : result.Failure.ToString())}");
                GUILayout.Label($"Hit: {result.HitPoint} Normal: {result.Normal} Anchor: {result.AnchorName}");
                GUILayout.Label(result.Message);
            }

            GUILayout.Label("Controls: A/D move, Space jump, LMB blue, RMB yellow, R restart");
            GUILayout.EndArea();
        }
    }
}
