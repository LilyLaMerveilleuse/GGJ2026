using UnityEngine;
using UnityEngine.Events;
using Bundles.SimplePlatformer2D.Scripts.Dialogue;

namespace Bundles.SimplePlatformer2D.Scripts.Interaction
{
    /// <summary>
    /// NPC that can display dialogue when interacted with.
    /// </summary>
    public class NPC : Interactable
    {
        [Header("NPC Settings")]
        [SerializeField] private DialogueData dialogue;

        [Header("Post-Dialogue Events")]
        [SerializeField] private UnityEvent onDialogueComplete;

        public override void Interact()
        {
            base.Interact();

            if (dialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(dialogue, OnDialogueFinished);
            }
        }

        private void OnDialogueFinished()
        {
            onDialogueComplete?.Invoke();
        }
    }
}
