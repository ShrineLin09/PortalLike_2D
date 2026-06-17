using UnityEngine;

namespace SidePortal.GridLevel
{
    [RequireComponent(typeof(GridLevelBuilder))]
    public sealed class GridLevelSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GridLevelData levelData;
        [SerializeField] private bool buildOnAwake = true;

        public void Configure(GridLevelData data, bool shouldBuildOnAwake)
        {
            levelData = data;
            buildOnAwake = shouldBuildOnAwake;
        }

        private void Awake()
        {
            if (!buildOnAwake)
            {
                return;
            }

            var builder = GetComponent<GridLevelBuilder>();
            builder.Build(levelData);
        }
    }
}
