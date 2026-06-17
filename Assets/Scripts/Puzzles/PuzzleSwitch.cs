using UnityEngine;
using UnityEngine.Events;

namespace SidePortal.Puzzles
{
    public sealed class PuzzleSwitch : MonoBehaviour
    {
        [SerializeField] private LayerMask activatingLayers;
        [SerializeField] private UnityEvent activated;
        [SerializeField] private UnityEvent deactivated;

        private int activatorCount;

        public bool IsActive => activatorCount > 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsInMask(other.gameObject.layer))
            {
                return;
            }

            activatorCount++;
            if (activatorCount == 1)
            {
                activated.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsInMask(other.gameObject.layer))
            {
                return;
            }

            activatorCount = Mathf.Max(0, activatorCount - 1);
            if (activatorCount == 0)
            {
                deactivated.Invoke();
            }
        }

        private bool IsInMask(int layer)
        {
            return (activatingLayers.value & (1 << layer)) != 0;
        }
    }
}
