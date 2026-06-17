using UnityEngine;

namespace SidePortal.Puzzles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class WeightedBox : MonoBehaviour
    {
        [SerializeField] private float mass = 3f;

        private void Awake()
        {
            var body = GetComponent<Rigidbody2D>();
            body.mass = mass;
            body.freezeRotation = true;
        }
    }
}
