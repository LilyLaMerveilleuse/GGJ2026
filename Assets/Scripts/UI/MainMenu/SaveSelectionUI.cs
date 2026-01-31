using System;
using SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    /// <summary>
    /// Panel de sélection des sauvegardes.
    /// </summary>
    public class SaveSelectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SaveSlotUI[] saveSlots;
        [SerializeField] private Button backButton;
        [SerializeField] private ConfirmationPopupUI confirmationPopup;

        public event Action OnBackRequested;
        public event Action<int> OnNewGameRequested;
        public event Action<int> OnLoadGameRequested;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
            }

            Debug.Log($"[SaveSelectionUI] Awake - saveSlots count: {(saveSlots != null ? saveSlots.Length : 0)}");

            if (saveSlots == null || saveSlots.Length == 0)
            {
                Debug.LogError("[SaveSelectionUI] saveSlots array is empty! Assign slots in inspector.");
                return;
            }

            for (int i = 0; i < saveSlots.Length; i++)
            {
                var slot = saveSlots[i];
                if (slot == null)
                {
                    Debug.LogError($"[SaveSelectionUI] saveSlots[{i}] is null!");
                    continue;
                }
                Debug.Log($"[SaveSelectionUI] Subscribing to slot {i}");
                slot.Initialize(i);
                slot.OnSlotClicked += HandleSlotClicked;
                slot.OnDeleteClicked += HandleDeleteClicked;
            }
        }

        private void OnDestroy()
        {
            foreach (var slot in saveSlots)
            {
                slot.OnSlotClicked -= HandleSlotClicked;
                slot.OnDeleteClicked -= HandleDeleteClicked;
            }
        }

        /// <summary>
        /// Affiche le panel de sélection.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAllSlots();
        }

        /// <summary>
        /// Cache le panel de sélection.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Rafraîchit l'affichage de tous les slots.
        /// </summary>
        public void RefreshAllSlots()
        {
            foreach (var slot in saveSlots)
            {
                slot.Refresh();
            }
        }

        private void HandleSlotClicked(int slotIndex)
        {
            Debug.Log($"[SaveSelectionUI] Slot {slotIndex} clicked");

            if (SaveManager.Instance == null)
            {
                Debug.LogError("[SaveSelectionUI] SaveManager.Instance is null!");
                return;
            }

            if (SaveManager.Instance.HasSave(slotIndex))
            {
                // Slot occupé - charger la partie directement
                Debug.Log($"[SaveSelectionUI] Loading existing save from slot {slotIndex}");
                OnLoadGameRequested?.Invoke(slotIndex);
            }
            else
            {
                // Slot vide - demander confirmation pour nouvelle partie
                Debug.Log($"[SaveSelectionUI] Slot {slotIndex} is empty, showing confirmation popup");

                if (confirmationPopup == null)
                {
                    Debug.LogError("[SaveSelectionUI] confirmationPopup is not assigned!");
                    return;
                }

                confirmationPopup.Show(
                    "Commencer une nouvelle partie ?",
                    () => OnNewGameRequested?.Invoke(slotIndex)
                );
            }
        }

        private void HandleDeleteClicked(int slotIndex)
        {
            if (confirmationPopup == null)
            {
                Debug.LogError("[SaveSelectionUI] confirmationPopup is not assigned!");
                return;
            }

            confirmationPopup.Show(
                "Supprimer cette sauvegarde ?",
                () =>
                {
                    SaveManager.Instance.DeleteSave(slotIndex);
                    RefreshAllSlots();
                }
            );
        }
    }
}
