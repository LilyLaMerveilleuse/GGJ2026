using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float disableTime = 0.25f;
        [SerializeField] private KeyCode dropDownKey = KeyCode.S;
        [SerializeField] private bool allowDropDown = true;

        private Collider2D _platformCollider;
        private bool _playerOnPlatform;
        private HashSet<Collider2D> _ignoredColliders = new HashSet<Collider2D>();

        private void Awake()
        {
            _platformCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (allowDropDown && _playerOnPlatform && Input.GetKeyDown(dropDownKey))
            {
                StartCoroutine(DisablePlatformTemporarily());
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _playerOnPlatform = true;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                _playerOnPlatform = false;
            }
        }

        private IEnumerator DisablePlatformTemporarily()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) yield break;

            var playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider == null) yield break;

            Physics2D.IgnoreCollision(_platformCollider, playerCollider, true);

            // Attendre le temps minimum
            yield return new WaitForSeconds(disableTime);

            // Attendre que le joueur soit en dessous de la plateforme ou complètement sorti
            while (IsPlayerInsidePlatform(player.transform, playerCollider))
            {
                yield return null;
            }

            Physics2D.IgnoreCollision(_platformCollider, playerCollider, false);
        }

        private bool IsPlayerInsidePlatform(Transform playerTransform, Collider2D playerCollider)
        {
            // Le joueur est "safe" si son haut est en dessous du bas de la plateforme
            float playerTop = playerCollider.bounds.max.y;
            float platformBottom = _platformCollider.bounds.min.y;

            // Toujours ignorer si le joueur chevauche horizontalement ET n'est pas clairement en dessous
            if (playerTop > platformBottom)
            {
                float playerRight = playerCollider.bounds.max.x;
                float playerLeft = playerCollider.bounds.min.x;
                float platformRight = _platformCollider.bounds.max.x;
                float platformLeft = _platformCollider.bounds.min.x;

                // Vérifie le chevauchement horizontal
                bool horizontalOverlap = playerRight > platformLeft && playerLeft < platformRight;
                return horizontalOverlap;
            }

            return false;
        }
    }
}
