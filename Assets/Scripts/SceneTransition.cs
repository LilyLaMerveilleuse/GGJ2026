using System.Collections;
using Bundles.SimplePlatformer2D.Scripts.UI;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bundles.SimplePlatformer2D.Scripts
{
    /// <summary>
    /// Trigger qui charge une nouvelle scène quand le joueur y entre.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneTransition : MonoBehaviour
    {
        [Header("Destination")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private string targetEntryPointId = "default";

        [Header("Settings")]
        [SerializeField] private bool saveBeforeTransition = true;

        private ScreenFader screenFader;
        private bool isTransitioning;

        private void Start()
        {
            // Trouver le ScreenFader persistant
            screenFader = FindObjectOfType<ScreenFader>();
        }

        private void Reset()
        {
            // S'assurer que le collider est un trigger
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTransitioning) return;

            // Vérifier que c'est le joueur
            if (!other.CompareTag("Player")) return;

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("[SceneTransition] Target scene name is not set!");
                return;
            }

            StartCoroutine(TransitionCoroutine());
        }

        private IEnumerator TransitionCoroutine()
        {
            isTransitioning = true;

            // Désactiver les contrôles du joueur
            if (PlayerController2D.Instance != null)
            {
                PlayerController2D.Instance.ControlsEnabled = false;
            }

            // Fade out
            if (screenFader != null)
            {
                yield return screenFader.FadeOut();
            }

            // Sauvegarder la position et la partie
            if (saveBeforeTransition && GameState.Instance != null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    GameState.Instance.UpdatePlayerPosition(player.transform.position);
                }
                GameState.Instance.SaveCurrentGame();
            }

            // Stocker l'ID de destination
            if (GameState.Instance != null)
            {
                GameState.Instance.SetTargetEntryPoint(targetEntryPointId);
            }

            // Charger la nouvelle scène
            SceneManager.LoadScene(targetSceneName);
        }

        private void OnDrawGizmos()
        {
            // Afficher le nom de la scène cible dans l'éditeur
            Gizmos.color = Color.cyan;
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }
        }
    }
}
