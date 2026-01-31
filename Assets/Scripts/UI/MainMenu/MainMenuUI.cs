using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    /// <summary>
    /// Panel principal du menu (Play, Options, Quit).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        public event Action OnPlayClicked;
        public event Action OnOptionsClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());

            // Options désactivé pour l'instant
            if (optionsButton != null)
            {
                optionsButton.interactable = false;
                optionsButton.onClick.AddListener(() => OnOptionsClicked?.Invoke());
            }

            quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        /// <summary>
        /// Affiche le panel principal.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Cache le panel principal.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
