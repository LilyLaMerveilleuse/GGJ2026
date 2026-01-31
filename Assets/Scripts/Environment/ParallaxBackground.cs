using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Environment
{
    /// <summary>
    /// Effet de parallax pour les backgrounds.
    /// Attacher ce script à chaque layer de background.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffectX = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffectY = 0.5f;

        [Header("Options")]
        [SerializeField] private bool infiniteHorizontal = true;
        [SerializeField] private bool infiniteVertical = false;

        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;
        private float _spriteWidth;
        private float _spriteHeight;
        private Vector3 _startPosition;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
            _lastCameraPosition = _cameraTransform.position;
            _startPosition = transform.position;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _spriteWidth = spriteRenderer.bounds.size.x;
                _spriteHeight = spriteRenderer.bounds.size.y;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;

            // Appliquer l'effet parallax
            float moveX = deltaMovement.x * parallaxEffectX;
            float moveY = deltaMovement.y * parallaxEffectY;
            transform.position += new Vector3(moveX, moveY, 0);

            _lastCameraPosition = _cameraTransform.position;

            // Répétition infinie horizontale
            if (infiniteHorizontal && _spriteWidth > 0)
            {
                float distanceX = _cameraTransform.position.x * (1 - parallaxEffectX);
                float offsetX = (_cameraTransform.position.x - _startPosition.x) * parallaxEffectX;

                if (distanceX > _startPosition.x + _spriteWidth)
                {
                    _startPosition.x += _spriteWidth;
                }
                else if (distanceX < _startPosition.x - _spriteWidth)
                {
                    _startPosition.x -= _spriteWidth;
                }

                transform.position = new Vector3(_startPosition.x + offsetX, transform.position.y, transform.position.z);
            }

            // Répétition infinie verticale
            if (infiniteVertical && _spriteHeight > 0)
            {
                float distanceY = _cameraTransform.position.y * (1 - parallaxEffectY);
                float offsetY = (_cameraTransform.position.y - _startPosition.y) * parallaxEffectY;

                if (distanceY > _startPosition.y + _spriteHeight)
                {
                    _startPosition.y += _spriteHeight;
                }
                else if (distanceY < _startPosition.y - _spriteHeight)
                {
                    _startPosition.y -= _spriteHeight;
                }

                transform.position = new Vector3(transform.position.x, _startPosition.y + offsetY, transform.position.z);
            }
        }
    }
}
