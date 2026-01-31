using SaveSystem;
using UnityEngine;

namespace UI.MainMenu
{
    /// <summary>
    /// Orchestre la navigation entre les écrans du menu principal.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private MainMenuUI mainMenuPanel;
        [SerializeField] private SaveSelectionUI saveSelectionPanel;

        private void Start()
        {
            // S'assurer que les singletons existent
            EnsureSingletonsExist();

            // Initialiser l'état
            ShowMainMenu();

            // Abonnement aux événements
            mainMenuPanel.OnPlayClicked += HandlePlayClicked;
            mainMenuPanel.OnQuitClicked += HandleQuitClicked;

            saveSelectionPanel.OnBackRequested += ShowMainMenu;
            saveSelectionPanel.OnNewGameRequested += HandleNewGame;
            saveSelectionPanel.OnLoadGameRequested += HandleLoadGame;
        }

        private void OnDestroy()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.OnPlayClicked -= HandlePlayClicked;
                mainMenuPanel.OnQuitClicked -= HandleQuitClicked;
            }

            if (saveSelectionPanel != null)
            {
                saveSelectionPanel.OnBackRequested -= ShowMainMenu;
                saveSelectionPanel.OnNewGameRequested -= HandleNewGame;
                saveSelectionPanel.OnLoadGameRequested -= HandleLoadGame;
            }
        }

        private void EnsureSingletonsExist()
        {
            if (SaveManager.Instance == null)
            {
                var saveManagerObj = new GameObject("SaveManager");
                saveManagerObj.AddComponent<SaveManager>();
            }

            if (GameState.Instance == null)
            {
                var gameStateObj = new GameObject("GameState");
                gameStateObj.AddComponent<GameState>();
            }
        }

        private void ShowMainMenu()
        {
            mainMenuPanel.Show();
            saveSelectionPanel.Hide();
        }

        private void ShowSaveSelection()
        {
            mainMenuPanel.Hide();
            saveSelectionPanel.Show();
        }

        private void HandlePlayClicked()
        {
            ShowSaveSelection();
        }

        private void HandleQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleNewGame(int slotIndex)
        {
            GameState.Instance.StartNewGame(slotIndex);
        }

        private void HandleLoadGame(int slotIndex)
        {
            GameState.Instance.LoadGame(slotIndex);
        }
    }
}
