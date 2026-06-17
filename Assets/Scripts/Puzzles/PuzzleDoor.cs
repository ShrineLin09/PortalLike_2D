using UnityEngine;

namespace SidePortal.Puzzles
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PuzzleDoor : MonoBehaviour
    {
        [SerializeField] private bool openOnStart;
        [SerializeField] private SpriteRenderer visual;

        private Collider2D doorCollider;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            doorCollider = GetComponent<Collider2D>();
            SetOpen(openOnStart);
        }

        public void Open()
        {
            SetOpen(true);
        }

        public void Close()
        {
            SetOpen(false);
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (doorCollider != null)
            {
                doorCollider.enabled = !open;
            }

            if (visual != null)
            {
                visual.enabled = !open;
            }
        }
    }
}
