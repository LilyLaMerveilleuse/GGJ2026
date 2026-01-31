using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts.Player
{
    /// <summary>
    /// Active des Trail Renderers sur les mains quand le joueur plane.
    /// </summary>
    public class GlideTrailEffect : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private TrailRenderer[] trails;

        [Header("Settings")]
        [SerializeField] private float fadeInSpeed = 10f;
        [SerializeField] private float fadeOutSpeed = 5f;

        private bool _wasGliding = false;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<PlayerController2D>();
            }

            // Désactiver les trails au démarrage
            SetTrailsEmitting(false);
        }

        private void Update()
        {
            if (controller == null) return;

            bool isGliding = controller.IsGliding;

            if (isGliding != _wasGliding)
            {
                SetTrailsEmitting(isGliding);
                _wasGliding = isGliding;
            }
        }

        private void SetTrailsEmitting(bool emitting)
        {
            foreach (var trail in trails)
            {
                if (trail != null)
                {
                    trail.emitting = emitting;
                }
            }
        }
    }
}
