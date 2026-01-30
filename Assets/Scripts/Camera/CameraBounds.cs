using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.CameraSystem
{
    /// <summary>
    /// Marks a Collider2D as the camera bounds for this scene.
    /// The CameraConfinerLinker will automatically find this and assign it to the Cinemachine Confiner.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CameraBounds : MonoBehaviour
    {
        private static CameraBounds _current;

        public static CameraBounds Current => _current;
        public Collider2D BoundingCollider { get; private set; }

        private void Awake()
        {
            BoundingCollider = GetComponent<Collider2D>();
            BoundingCollider.isTrigger = true;
        }

        private void OnEnable()
        {
            _current = this;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}
