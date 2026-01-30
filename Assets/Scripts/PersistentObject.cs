using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Makes this GameObject persist between scene loads.
    /// Ensures only one instance exists (singleton pattern).
    /// Use on Canvas, EventSystem, or any object that should survive scene changes.
    /// </summary>
    public class PersistentObject : MonoBehaviour
    {
        [SerializeField] private string uniqueId;

        private static readonly System.Collections.Generic.Dictionary<string, PersistentObject> _instances
            = new System.Collections.Generic.Dictionary<string, PersistentObject>();

        private void Awake()
        {
            // Use GameObject name if no unique ID specified
            string id = string.IsNullOrEmpty(uniqueId) ? gameObject.name : uniqueId;

            if (_instances.TryGetValue(id, out var existingInstance) && existingInstance != null && existingInstance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instances[id] = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            string id = string.IsNullOrEmpty(uniqueId) ? gameObject.name : uniqueId;
            if (_instances.TryGetValue(id, out var instance) && instance == this)
            {
                _instances.Remove(id);
            }
        }
    }
}
