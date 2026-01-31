using Constants;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Gère la touche Escape pour retourner au menu principal.
    /// À placer sur un GameObject persistant (comme le joueur).
    /// </summary>
    public class PauseHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Ne pas réagir si on est déjà au menu principal
                if (SceneManager.GetActiveScene().name == GameConstants.Scenes.MainMenu)
                {
                    return;
                }

                ReturnToMenu();
            }
        }

        private void ReturnToMenu()
        {
            // Sauvegarder la position du joueur avant de quitter
            if (GameState.Instance != null && GameState.Instance.CurrentSave != null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    GameState.Instance.UpdatePlayerPosition(player.transform.position);
                }

                GameState.Instance.ReturnToMainMenu();
            }
            else
            {
                // Pas de partie en cours, juste charger le menu
                SceneManager.LoadScene(GameConstants.Scenes.MainMenu);
            }
        }
    }
}
