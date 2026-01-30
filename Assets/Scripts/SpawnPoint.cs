using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Marks a spawn point in the scene.
    /// The player will respawn here after death.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        private static SpawnPoint _currentSpawnPoint;

        public static SpawnPoint Current => _currentSpawnPoint;

        private void OnEnable()
        {
            _currentSpawnPoint = this;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
        }
    }
}
