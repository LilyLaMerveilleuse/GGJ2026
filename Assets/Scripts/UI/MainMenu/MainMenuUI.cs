using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    /// <summary>
    /// Panel principal du menu (Play, Quit).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        public event Action OnPlayClicked;
        public event Action OnQuitClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
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
