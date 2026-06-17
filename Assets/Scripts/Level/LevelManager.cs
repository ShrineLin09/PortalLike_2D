using UnityEngine;
using UnityEngine.SceneManagement;

namespace SidePortal.Level
{
    public sealed class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(restartKey))
            {
                RestartLevel();
            }
        }

        public void RestartLevel()
        {
            var current = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(current);
        }

        public void LoadNextLevel()
        {
            var current = SceneManager.GetActiveScene().buildIndex;
            var next = current + 1;

            if (next < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(next);
            }
            else
            {
                RestartLevel();
            }
        }
    }
}
