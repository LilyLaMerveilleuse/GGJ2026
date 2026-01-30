using UnityEngine;
using UnityEngine.Events;

namespace Bundles.SimplePlatformer2D.Scripts.Interaction
{
    /// <summary>
    /// Base class for all interactable objects in the game.
    /// Shows a prompt when player is nearby and can trigger events on interaction.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private string interactionPrompt = "Appuyer sur E";
        [SerializeField] private Vector2 promptOffset = new Vector2(0f, 1.5f);

        [Header("Events")]
        [SerializeField] private UnityEvent onInteract;

        public string InteractionPrompt => interactionPrompt;
        public Vector2 PromptOffset => promptOffset;

        /// <summary>
        /// Called when the player interacts with this object.
        /// Override in derived classes to add custom behavior.
        /// </summary>
        public virtual void Interact()
        {
            onInteract?.Invoke();
        }

        /// <summary>
        /// Called when the player enters the interaction zone.
        /// </summary>
        public virtual void OnPlayerEnter()
        {
        }

        /// <summary>
        /// Called when the player exits the interaction zone.
        /// </summary>
        public virtual void OnPlayerExit()
        {
        }

        private void Reset()
        {
            // Ensure there's a trigger collider
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
    }
}
