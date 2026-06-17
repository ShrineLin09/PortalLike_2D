using UnityEngine;

namespace SidePortal.Level
{
    public sealed class LevelExit : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool loadNextLevel = true;
        [SerializeField] private LevelCompleteOverlay completeOverlay;

        public void Configure(bool shouldLoadNextLevel, LevelCompleteOverlay overlay)
        {
            loadNextLevel = shouldLoadNextLevel;
            completeOverlay = overlay;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (completeOverlay != null)
            {
                completeOverlay.ShowComplete();
            }

            if (loadNextLevel && LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
    }
}
