using System.Collections.Generic;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Marks the entry point of a scene.
    /// The player will be teleported here when entering the scene (not after death).
    /// </summary>
    public class SceneEntryPoint : MonoBehaviour
    {
        [Header("Identification")]
        [SerializeField] private string entryPointId = "default";

        [Header("Spawn Point")]
        [SerializeField] private SpawnPoint associatedSpawnPoint;

        private static Dictionary<string, SceneEntryPoint> _entryPoints = new Dictionary<string, SceneEntryPoint>();
        private static SceneEntryPoint _default;

        public string EntryPointId => entryPointId;
        public SpawnPoint AssociatedSpawnPoint => associatedSpawnPoint;

        /// <summary>
        /// Retourne l'entry point par défaut de la scène.
        /// </summary>
        public static SceneEntryPoint Default => _default;

        /// <summary>
        /// Retourne l'entry point correspondant à l'ID, ou le défaut si non trouvé.
        /// </summary>
        public static SceneEntryPoint GetById(string id)
        {
            if (string.IsNullOrEmpty(id) || !_entryPoints.TryGetValue(id, out var entryPoint))
            {
                return _default;
            }
            return entryPoint;
        }

        private void OnEnable()
        {
            _entryPoints[entryPointId] = this;

            // Le premier entry point ou celui avec l'id "default" devient le défaut
            if (_default == null || entryPointId == "default")
            {
                _default = this;
            }
        }

        private void OnDisable()
        {
            if (_entryPoints.TryGetValue(entryPointId, out var entry) && entry == this)
            {
                _entryPoints.Remove(entryPointId);
            }

            if (_default == this)
            {
                _default = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);

            // Ligne vers le spawn point associé
            if (associatedSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, associatedSpawnPoint.transform.position);
            }
        }
    }
}
