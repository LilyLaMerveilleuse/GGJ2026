using System.Collections;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Environment
{
    /// <summary>
    /// Mur secret qui se révèle quand le joueur passe à travers.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SecretWall : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Settings")]
        [SerializeField] private float revealSpeed = 3f;
        [SerializeField] private float hiddenAlpha = 0.2f;
        [SerializeField] private bool stayRevealed = false;

        private float _targetAlpha = 1f;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // S'assurer que le collider est un trigger
            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Reveal();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            if (!stayRevealed)
            {
                Hide();
            }
        }

        /// <summary>
        /// Révèle le secret (rend le mur transparent).
        /// </summary>
        public void Reveal()
        {
            _targetAlpha = hiddenAlpha;
            StartFade();
        }

        /// <summary>
        /// Cache le secret (rend le mur opaque).
        /// </summary>
        public void Hide()
        {
            _targetAlpha = 1f;
            StartFade();
        }

        private void StartFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeCoroutine());
        }

        private IEnumerator FadeCoroutine()
        {
            Color color = spriteRenderer.color;

            while (!Mathf.Approximately(color.a, _targetAlpha))
            {
                color.a = Mathf.MoveTowards(color.a, _targetAlpha, revealSpeed * Time.deltaTime);
                spriteRenderer.color = color;
                yield return null;
            }

            color.a = _targetAlpha;
            spriteRenderer.color = color;
            _fadeCoroutine = null;
        }
    }
}
