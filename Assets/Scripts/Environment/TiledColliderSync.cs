using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Environment
{
    /// <summary>
    /// Synchronise la taille du BoxCollider2D avec le SpriteRenderer en mode Tiled.
    /// Permet d'ajuster le collider pour exclure des parties du sprite (ex: herbe).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [ExecuteInEditMode]
    public class TiledColliderSync : MonoBehaviour
    {
        [Header("Ajustements du collider")]
        [SerializeField] private float topPadding = 0.2f;
        [SerializeField] private float bottomPadding = 0f;
        [SerializeField] private float leftPadding = 0f;
        [SerializeField] private float rightPadding = 0f;

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private Vector2 _lastSize;
        private float _lastTopPadding;
        private float _lastBottomPadding;
        private float _lastLeftPadding;
        private float _lastRightPadding;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            SyncCollider();
        }

        private void Update()
        {
            if (_spriteRenderer == null) return;

            // Synchroniser si la taille ou les paddings ont changé
            bool sizeChanged = _spriteRenderer.size != _lastSize;
            bool paddingChanged = topPadding != _lastTopPadding ||
                                  bottomPadding != _lastBottomPadding ||
                                  leftPadding != _lastLeftPadding ||
                                  rightPadding != _lastRightPadding;

            if (sizeChanged || paddingChanged)
            {
                SyncCollider();
            }
        }

        private void SyncCollider()
        {
            if (_spriteRenderer == null || _boxCollider == null) return;

            Vector2 spriteSize = _spriteRenderer.size;

            // Calculer la nouvelle taille
            float width = spriteSize.x - leftPadding - rightPadding;
            float height = spriteSize.y - topPadding - bottomPadding;
            _boxCollider.size = new Vector2(width, height);

            // Calculer l'offset pour centrer le collider correctement
            float offsetX = (leftPadding - rightPadding) / 2f;
            float offsetY = (bottomPadding - topPadding) / 2f;
            _boxCollider.offset = new Vector2(offsetX, offsetY);

            // Sauvegarder les valeurs
            _lastSize = spriteSize;
            _lastTopPadding = topPadding;
            _lastBottomPadding = bottomPadding;
            _lastLeftPadding = leftPadding;
            _lastRightPadding = rightPadding;
        }
    }
}
