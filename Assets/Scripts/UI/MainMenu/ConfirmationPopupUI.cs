using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.MainMenu
{
    /// <summary>
    /// Popup de confirmation réutilisable (Oui/Non).
    /// </summary>
    public class ConfirmationPopupUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action onConfirm;
        private Action onCancel;

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
            // Ne pas désactiver ici - la popup doit être désactivée dans l'éditeur
        }

        /// <summary>
        /// Affiche la popup avec un message et des callbacks.
        /// </summary>
        public void Show(string message, Action onConfirmCallback, Action onCancelCallback = null)
        {
            messageText.text = message;
            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Cache la popup.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            onConfirm = null;
            onCancel = null;
        }

        private void OnYesClicked()
        {
            var callback = onConfirm;
            Hide();
            callback?.Invoke();
        }

        private void OnNoClicked()
        {
            var callback = onCancel;
            Hide();
            callback?.Invoke();
        }
    }
}
