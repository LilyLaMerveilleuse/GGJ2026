using Bundles.SimplePlatformer2D.Scripts.Dialogue;
using Bundles.SimplePlatformer2D.Scripts.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace Masks
{
    /// <summary>
    /// Interactable qui donne un masque au joueur après un dialogue.
    /// </summary>
    public class MaskPickup : Interactable
    {
        [Header("Mask Settings")]
        [SerializeField] private MaskType maskType = MaskType.DoubleJump;
        [SerializeField] private DialogueData obtainDialogue;
        [SerializeField] private bool destroyAfterPickup = true;

        [Header("Visuals")]
        [SerializeField] private GameObject visualObject;

        [Header("Events")]
        [SerializeField] private UnityEvent onMaskObtained;

        private bool isObtained;

        private void Start()
        {
            // Vérifier si le masque a déjà été obtenu
            if (MaskInventory.Instance != null && MaskInventory.Instance.HasMask(maskType))
            {
                isObtained = true;
                if (destroyAfterPickup)
                {
                    Destroy(gameObject);
                }
                else if (visualObject != null)
                {
                    visualObject.SetActive(false);
                }
            }
        }

        public override void Interact()
        {
            if (isObtained) return;

            if (obtainDialogue != null && DialogueManager.Instance != null)
            {
                // Afficher le dialogue puis donner le masque
                DialogueManager.Instance.StartDialogue(obtainDialogue, OnDialogueComplete);
            }
            else
            {
                // Pas de dialogue, donner directement le masque
                GiveMask();
            }
        }

        private void OnDialogueComplete()
        {
            GiveMask();
        }

        private void GiveMask()
        {
            if (isObtained) return;
            isObtained = true;

            // Donner le masque au joueur
            if (MaskInventory.Instance != null)
            {
                MaskInventory.Instance.ObtainMask(maskType);
            }
            else
            {
                Debug.LogError("[MaskPickup] MaskInventory.Instance is null!");
            }

            onMaskObtained?.Invoke();

            // Désactiver ou détruire l'objet
            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else if (visualObject != null)
            {
                visualObject.SetActive(false);
            }
        }
    }
}
