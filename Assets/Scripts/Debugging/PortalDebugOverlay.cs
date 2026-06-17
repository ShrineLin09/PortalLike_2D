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

            GUILayout.BeginArea(new Rect(12f, 12f, 520f, 260f), GUI.skin.box);
            GUILayout.Label("传送门原型调试");

            if (player != null)
            {
                GUILayout.Label($"是否落地：{(player.IsGrounded ? "是" : "否")}");
                if (player.TryGetComponent<Rigidbody2D>(out var body))
                {
                    GUILayout.Label($"当前速度：{body.velocity} 速度大小：{body.velocity.magnitude:0.00}");
                }
            }

            if (portalGun != null)
            {
                var result = portalGun.LastPlacementResult;
                GUILayout.Label($"鼠标瞄准方向：{portalGun.CurrentMouseAimDirection}");
                GUILayout.Label($"上次放置结果：{(result.Success ? "已接受" : FailureName(result.Failure))}");
                GUILayout.Label($"命中点：{result.HitPoint} 法线：{result.Normal} 锚点：{result.AnchorName}");
                GUILayout.Label(result.Message);
            }

            var teleport = Portal.LastTeleportDebug;
            GUILayout.Label($"最近入门速度：{teleport.EntryVelocity} 大小：{teleport.EntryVelocity.magnitude:0.00}");
            GUILayout.Label($"最近出门速度：{teleport.ExitVelocity} 大小：{teleport.ExitVelocity.magnitude:0.00}");
            GUILayout.Label($"入门法线：{teleport.EntryNormal} 出口法线：{teleport.ExitNormal}");
            GUILayout.Label($"速度截断：{(teleport.WasClamped ? ClampName(teleport.ClampType) : "否")}");

            GUILayout.Label("操作：A/D 移动，空格跳跃，左键蓝门，右键黄门，R 重开");
            GUILayout.EndArea();
        }

        private static string FailureName(PortalPlacementFailure failure)
        {
            return failure switch
            {
                PortalPlacementFailure.NoValidAnchorHit => "没有命中有效传送门锚点",
                PortalPlacementFailure.AnchorDisabled => "锚点已禁用",
                PortalPlacementFailure.PortalTypeNotAllowed => "锚点不允许该门类型",
                PortalPlacementFailure.BlockedPortalSpace => "传送门空间被阻挡",
                PortalPlacementFailure.OverlappingPortal => "与已有传送门重叠",
                PortalPlacementFailure.BlockedExitClearance => "出口空间被阻挡",
                _ => failure.ToString()
            };
        }

        private static string ClampName(VelocityClampType clampType)
        {
            return clampType switch
            {
                VelocityClampType.MaxExitSpeed => "最大出门速度",
                VelocityClampType.MaxDownwardExitSpeed => "最大向下出门速度",
                VelocityClampType.MaxExitSpeedAndDownwardExitSpeed => "最大出门速度 + 最大向下出门速度",
                _ => "否"
            };
        }
    }
}
