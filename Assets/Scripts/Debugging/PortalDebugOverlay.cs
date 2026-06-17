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
                GUILayout.Label($"Aim: {player.AimDirection}");
                GUILayout.Label($"Grounded: {player.IsGrounded}");
            }

            if (portalGun != null)
            {
                var result = portalGun.LastPlacementResult;
                GUILayout.Label($"Last placement: {(result.Success ? "Accepted" : result.Failure.ToString())}");
                GUILayout.Label($"Hit: {result.HitPoint} Normal: {result.Normal}");
                GUILayout.Label(result.Message);
            }

            GUILayout.Label("Controls: A/D move, Space jump, arrows/WASD aim, Q/E portals, R restart");
            GUILayout.EndArea();
        }
    }
}
