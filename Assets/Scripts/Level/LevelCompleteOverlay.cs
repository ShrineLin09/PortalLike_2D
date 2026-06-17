using UnityEngine;

namespace SidePortal.Level
{
    public sealed class LevelCompleteOverlay : MonoBehaviour
    {
        private bool completed;

        public void ShowComplete()
        {
            completed = true;
        }

        private void OnGUI()
        {
            if (!completed)
            {
                return;
            }

            var rect = new Rect(Screen.width * 0.5f - 180f, 32f, 360f, 72f);
            GUI.Box(rect, "第一关完成\n下一关尚未制作，按 R 重新开始。");
        }
    }
}
