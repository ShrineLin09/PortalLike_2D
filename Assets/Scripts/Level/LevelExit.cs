using UnityEngine;

namespace SidePortal.Level
{
    public sealed class LevelExit : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag) && LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
    }
}
