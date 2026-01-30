using UnityEngine;
using TMPro;

namespace Bundles.SimplePlatformer2D.Scripts.Dialogue
{
    /// <summary>
    /// Handles the dialogue UI display.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private GameObject continueIndicator;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            dialoguePanel.SetActive(true);
        }

        public void Hide()
        {
            dialoguePanel.SetActive(false);
        }

        public void SetText(string text)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }
        }

        public void SetSpeakerName(string name)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = name;
                speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(name));
            }
        }

        public void ShowContinueIndicator(bool show)
        {
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(show);
            }
        }
    }
}
