using System.Collections;
using Bundles.SimplePlatformer2D.Scripts.UI;
using SaveSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Bundles.SimplePlatformer2D.Scripts;

namespace Bundles.SimplePlatformer2D.Scripts.Player
{
    /// <summary>
    /// Manages player health, damage, and death.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invincibilityDuration = 1.5f;

        [Header("References")]
        [SerializeField] private ScreenFader screenFader;

        [Header("Events")]
        public UnityEvent<int> onHealthChanged;
        public UnityEvent onDamaged;
        public UnityEvent onDeath;

        private int currentHealth;
        private bool isInvincible;
        private bool isDead;
        private bool isRespawning;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsInvincible => isInvincible;
        public bool IsDead => isDead;

        private void Start()
        {
            currentHealth = maxHealth;
            onHealthChanged?.Invoke(currentHealth);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(HandleSceneLoaded());
        }

        private IEnumerator HandleSceneLoaded()
        {
            yield return null; // Wait one frame for SceneEntryPoint/SpawnPoint.OnEnable

            if (isRespawning)
            {
                // After death: teleport to spawn point
                TeleportToSpawnPoint();
                ResetHealth();
                isRespawning = false;

                // Brief invincibility after respawn
                StartCoroutine(InvincibilityCoroutine());

                // Fade in
                if (screenFader != null)
                {
                    screenFader.FadeIn();
                }
            }
            else if (GameState.Instance != null && GameState.Instance.ShouldLoadPositionFromSave)
            {
                // Loading from save: teleport to saved position
                TeleportToSavedPosition();
                GameState.Instance.ShouldLoadPositionFromSave = false;
            }
            else
            {
                // Normal scene entry: teleport to entry point
                TeleportToEntryPoint();
            }
        }

        private void TeleportToSpawnPoint()
        {
            if (SpawnPoint.Current != null)
            {
                transform.position = SpawnPoint.Current.transform.position;
            }
        }

        private void TeleportToEntryPoint()
        {
            if (SceneEntryPoint.Default != null)
            {
                transform.position = SceneEntryPoint.Default.transform.position;
            }
        }

        private void TeleportToSavedPosition()
        {
            var savedPos = GameState.Instance.GetSavedPosition();
            // If saved position is zero (new game), use entry point instead
            if (savedPos == Vector3.zero)
            {
                TeleportToEntryPoint();
            }
            else
            {
                transform.position = savedPos;
            }
        }

        /// <summary>
        /// Apply damage to the player.
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        /// <param name="oneShot">If true, kills instantly regardless of health</param>
        public void TakeDamage(int damage = 1, bool oneShot = false)
        {
            if (isDead) return;
            if (isInvincible && !oneShot) return;

            if (oneShot)
            {
                currentHealth = 0;
            }
            else
            {
                currentHealth -= damage;
            }

            currentHealth = Mathf.Max(0, currentHealth);
            onHealthChanged?.Invoke(currentHealth);
            onDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            onHealthChanged?.Invoke(currentHealth);
        }

        /// <summary>
        /// Reset health to max.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            isInvincible = false;
            onHealthChanged?.Invoke(currentHealth);

            // Réactiver les contrôles
            if (PlayerController2D.Instance != null)
            {
                PlayerController2D.Instance.ControlsEnabled = true;
            }
        }

        private void Die()
        {
            isDead = true;

            // Désactiver les contrôles
            if (PlayerController2D.Instance != null)
            {
                PlayerController2D.Instance.ControlsEnabled = false;
            }

            onDeath?.Invoke();
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Fade out
            if (screenFader != null)
            {
                yield return screenFader.FadeOut();
            }

            // Mark as respawning so OnSceneLoaded will teleport to spawn
            isRespawning = true;

            // Reload current scene (respawn handled by OnSceneLoaded)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                TakeDamage(damageable.Damage, damageable.OneShot);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var damageable = other.collider.GetComponent<Damageable>();
            if (damageable != null)
            {
                TakeDamage(damageable.Damage, damageable.OneShot);
            }
        }
    }
}
