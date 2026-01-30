using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Attach to any object that should damage the player on contact.
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damage = 1;
        [SerializeField] private bool oneShot;

        public int Damage => damage;
        public bool OneShot => oneShot;
    }
}
