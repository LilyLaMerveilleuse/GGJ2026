using UnityEngine;
using TMPro;

namespace Bundles.SimplePlatformer2D.Scripts.Interaction
{
    /// <summary>
    /// Handles player interaction with Interactable objects.
    /// Attach to the Player GameObject.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Header("UI References")]
        [SerializeField] private GameObject interactionPromptUI;
        [SerializeField] private TextMeshProUGUI interactionPromptText;

        private Interactable currentInteractable;
        private bool canInteract = true;

        private void Update()
        {
            // Don't process input if dialogue is active
            if (Dialogue.DialogueManager.Instance != null &&
                Dialogue.DialogueManager.Instance.IsDialogueActive)
            {
                return;
            }

            if (currentInteractable != null && canInteract && Input.GetKeyDown(interactionKey))
            {
                currentInteractable.Interact();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponent<Interactable>();
            if (interactable != null)
            {
                SetCurrentInteractable(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponent<Interactable>();
            if (interactable != null && interactable == currentInteractable)
            {
                ClearCurrentInteractable();
            }
        }

        private void SetCurrentInteractable(Interactable interactable)
        {
            currentInteractable = interactable;
            currentInteractable.OnPlayerEnter();

            ShowInteractionPrompt(interactable);
        }

        private void ClearCurrentInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnPlayerExit();
            }

            currentInteractable = null;
            HideInteractionPrompt();
        }

        private void ShowInteractionPrompt(Interactable interactable)
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);

                if (interactionPromptText != null)
                {
                    interactionPromptText.text = interactable.InteractionPrompt;
                }

                // Position the prompt above the interactable
                UpdatePromptPosition(interactable);
            }
        }

        private void HideInteractionPrompt()
        {
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (currentInteractable != null && interactionPromptUI != null && interactionPromptUI.activeSelf)
            {
                UpdatePromptPosition(currentInteractable);
            }
        }

        private void UpdatePromptPosition(Interactable interactable)
        {
            if (interactionPromptUI == null) return;

            // Convert world position to screen position
            Vector3 worldPos = interactable.transform.position + (Vector3)interactable.PromptOffset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            interactionPromptUI.transform.position = screenPos;
        }

        /// <summary>
        /// Enable or disable interaction ability.
        /// </summary>
        public void SetCanInteract(bool value)
        {
            canInteract = value;
        }
    }
}
