using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode dropDownKey = KeyCode.S;
        [SerializeField] private bool allowDropDown = true;

        private Collider2D _platformCollider;
        private Collider2D _playerCollider;
        private bool _isIgnoring;

        private void Awake()
        {
            _platformCollider = GetComponent<Collider2D>();
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerCollider = player.GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (!allowDropDown || _playerCollider == null) return;

            bool holdingDown = Input.GetKey(dropDownKey);

            if (holdingDown && !_isIgnoring)
            {
                Physics2D.IgnoreCollision(_platformCollider, _playerCollider, true);
                _isIgnoring = true;
            }
            else if (!holdingDown && _isIgnoring && !_platformCollider.IsTouching(_playerCollider))
            {
                Physics2D.IgnoreCollision(_platformCollider, _playerCollider, false);
                _isIgnoring = false;
            }
        }

    }
}
