using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bundles.SimplePlatformer2D.Scripts.UI
{
    /// <summary>
    /// Handles screen fade in/out transitions.
    /// Requires a UI Image covering the entire screen.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ScreenFader : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private bool startFadedOut;

        private Image fadeImage;
        private bool isFading;

        public bool IsFading => isFading;

        private void Awake()
        {
            fadeImage = GetComponent<Image>();
            fadeImage.color = fadeColor;

            if (startFadedOut)
            {
                SetAlpha(1f);
            }
            else
            {
                SetAlpha(0f);
            }
        }

        private void Start()
        {
            // Auto fade in if started faded out
            if (startFadedOut)
            {
                FadeIn();
            }
        }

        /// <summary>
        /// Fade to black (or fade color).
        /// </summary>
        public Coroutine FadeOut()
        {
            return StartCoroutine(FadeToAlpha(1f));
        }

        /// <summary>
        /// Fade from black to transparent.
        /// </summary>
        public Coroutine FadeIn()
        {
            return StartCoroutine(FadeToAlpha(0f));
        }

        /// <summary>
        /// Fade to a specific alpha value.
        /// </summary>
        public Coroutine FadeTo(float targetAlpha)
        {
            return StartCoroutine(FadeToAlpha(targetAlpha));
        }

        private IEnumerator FadeToAlpha(float targetAlpha)
        {
            isFading = true;

            float startAlpha = fadeImage.color.a;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
                yield return null;
            }

            SetAlpha(targetAlpha);
            isFading = false;
        }

        private void SetAlpha(float alpha)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
