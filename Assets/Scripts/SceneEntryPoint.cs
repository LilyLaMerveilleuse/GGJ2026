using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Marks the entry point of a scene.
    /// The player will be teleported here when entering the scene (not after death).
    /// </summary>
    public class SceneEntryPoint : MonoBehaviour
    {
        private static SceneEntryPoint _current;

        public static SceneEntryPoint Current => _current;

        private void OnEnable()
        {
            _current = this;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
        }
    }
}
