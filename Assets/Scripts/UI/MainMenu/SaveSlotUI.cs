using System;
using SaveSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.MainMenu
{
    /// <summary>
    /// Composant UI pour un slot de sauvegarde individuel.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button slotButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TextMeshProUGUI slotNumberText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private GameObject emptyStateObject;
        [SerializeField] private GameObject filledStateObject;

        public int SlotIndex { get; private set; }
        public bool HasSave { get; private set; }

        public event Action<int> OnSlotClicked;
        public event Action<int> OnDeleteClicked;

        private void Awake()
        {
            if (slotButton == null)
            {
                // Si pas de bouton assigné, utiliser le Button sur ce GameObject
                slotButton = GetComponent<Button>();
            }

            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() =>
                {
                    Debug.Log($"[SaveSlotUI] Button clicked for slot {SlotIndex}");
                    OnSlotClicked?.Invoke(SlotIndex);
                });
            }
            else
            {
                Debug.LogError($"[SaveSlotUI] slotButton is not assigned on {gameObject.name}!");
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(SlotIndex));
            }
        }

        /// <summary>
        /// Initialise le slot avec un index.
        /// </summary>
        public void Initialize(int index)
        {
            SlotIndex = index;
            if (slotNumberText != null) slotNumberText.text = $"Slot {index + 1}";
            Refresh();
        }

        /// <summary>
        /// Rafraîchit l'affichage du slot.
        /// </summary>
        public void Refresh()
        {
            var save = SaveManager.Instance?.GetSave(SlotIndex);
            HasSave = save != null;

            if (HasSave)
            {
                ShowFilledState(save);
            }
            else
            {
                ShowEmptyState();
            }
        }

        private void ShowEmptyState()
        {
            if (emptyStateObject != null) emptyStateObject.SetActive(true);
            if (filledStateObject != null) filledStateObject.SetActive(false);
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);

            if (statusText != null) statusText.text = "Vide";
            if (detailsText != null) detailsText.text = "Nouvelle partie";
        }

        private void ShowFilledState(SaveData save)
        {
            if (emptyStateObject != null) emptyStateObject.SetActive(false);
            if (filledStateObject != null) filledStateObject.SetActive(true);
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);

            if (statusText != null) statusText.text = save.currentScene;

            var lastPlayed = save.GetLastPlayedDate();
            var playTime = save.GetFormattedPlayTime();
            if (detailsText != null) detailsText.text = $"{lastPlayed:dd/MM/yyyy}\nTime: {playTime}";
        }
    }
}
