using Constants;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    public enum JumpPhase
    {
        Grounded,
        Start,      // Initiation
        Rise,       // Montée
        Apex,       // Apogée
        Fall,       // Chute
        Land        // Atterrissage
    }

    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerAnimator2D : MonoBehaviour
    {
        [Header("Sprite Flip")]
        [SerializeField] private bool flipSprite = true;
        [SerializeField] private bool useScaleFlip = true;

        [Header("Animator Parameters")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string groundedParam = "IsGrounded";
        [SerializeField] private string verticalVelocityParam = "VerticalVelocity";

        [Header("Animation States - Sol")]
        [SerializeField] private string idleState = "Idle";
        [SerializeField] private string walkState = "Walk";

        [Header("Animation States - Saut")]
        [SerializeField] private string jumpStartState = "JumpStart";
        [SerializeField] private string jumpRiseState = "JumpRise";
        [SerializeField] private string jumpApexState = "JumpApex";
        [SerializeField] private string jumpFallState = "JumpFall";
        [SerializeField] private string jumpLandState = "JumpLand";

        [Header("Seuils de détection")]
        [SerializeField] private float apexThreshold = 10f;
        [SerializeField] private float jumpStartDuration = 0.1f;
        [SerializeField] private float landingDuration = 0.1f;

        private PlayerController2D _controller;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private bool _facingRight = false;
        private bool _wasMoving = false;
        private bool _wasGrounded = true;
        private JumpPhase _currentJumpPhase = JumpPhase.Grounded;
        private float _jumpStartTimer = 0f;
        private float _landingTimer = 0f;

        private void Awake()
        {
            _controller = GetComponent<PlayerController2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            HandleFlip();
            UpdateAnimator();
        }

        private void HandleFlip()
        {
            if (!flipSprite) return;

            float horizontal = Input.GetAxisRaw(GameConstants.Input.Horizontal);

            if ((horizontal > 0 && !_facingRight) || (horizontal < 0 && _facingRight))
            {
                Flip();
            }
        }

        private void Flip()
        {
            _facingRight = !_facingRight;

            if (useScaleFlip)
            {
                Vector3 scale = transform.localScale;
                scale.x *= -1;
                transform.localScale = scale;
            }
            else if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = !_facingRight;
            }
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            float horizontalInput = Input.GetAxisRaw(GameConstants.Input.Horizontal);
            bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
            bool isGrounded = _controller.IsGrounded;
            float velocityY = _controller.Velocity.y;

            // Gestion des phases de saut
            UpdateJumpPhase(isGrounded, velocityY);

            // Animation au sol
            if (_currentJumpPhase == JumpPhase.Grounded)
            {
                if (isMoving != _wasMoving)
                {
                    _animator.Play(isMoving ? walkState : idleState, 0, 0f);
                }
            }

            _wasMoving = isMoving;
            _wasGrounded = isGrounded;

            _animator.SetFloat(speedParam, Mathf.Abs(horizontalInput));
            _animator.SetBool(groundedParam, isGrounded);
            _animator.SetFloat(verticalVelocityParam, velocityY);
        }

        private void UpdateJumpPhase(bool isGrounded, float velocityY)
        {
            JumpPhase newPhase = _currentJumpPhase;

            // Gestion du timer d'atterrissage
            if (_currentJumpPhase == JumpPhase.Land)
            {
                _landingTimer -= Time.deltaTime;
                if (_landingTimer <= 0f)
                {
                    newPhase = JumpPhase.Grounded;
                }
            }
            // Gestion du timer de départ de saut
            else if (_currentJumpPhase == JumpPhase.Start)
            {
                _jumpStartTimer -= Time.deltaTime;
                if (_jumpStartTimer <= 0f)
                {
                    // Passer à Rise ou Fall selon la vélocité
                    if (velocityY > 0)
                    {
                        newPhase = JumpPhase.Rise;
                    }
                    else
                    {
                        newPhase = JumpPhase.Fall;
                    }
                }
                // Si on atterrit pendant le JumpStart
                else if (isGrounded)
                {
                    newPhase = JumpPhase.Land;
                    _landingTimer = landingDuration;
                }
            }
            // Vient d'atterrir
            else if (isGrounded && !_wasGrounded)
            {
                newPhase = JumpPhase.Land;
                _landingTimer = landingDuration;
            }
            // Vient de quitter le sol
            else if (!isGrounded && _wasGrounded)
            {
                newPhase = JumpPhase.Start;
                _jumpStartTimer = jumpStartDuration;
            }
            // En l'air (après JumpStart)
            else if (!isGrounded)
            {
                if (velocityY > apexThreshold)
                {
                    newPhase = JumpPhase.Rise;
                }
                else if (velocityY < -apexThreshold)
                {
                    newPhase = JumpPhase.Fall;
                }
                else if (_currentJumpPhase == JumpPhase.Rise)
                {
                    newPhase = JumpPhase.Apex;
                }
            }
            // Au sol (pas en train d'atterrir)
            else if (_currentJumpPhase != JumpPhase.Land)
            {
                newPhase = JumpPhase.Grounded;
            }

            // Changement de phase
            if (newPhase != _currentJumpPhase)
            {
                _currentJumpPhase = newPhase;
                PlayJumpAnimation(newPhase);
            }
        }

        private void PlayJumpAnimation(JumpPhase phase)
        {
            // Retour au sol : jouer Idle ou Walk selon le mouvement
            if (phase == JumpPhase.Grounded)
            {
                float horizontalInput = Input.GetAxisRaw(GameConstants.Input.Horizontal);
                bool isMoving = Mathf.Abs(horizontalInput) > 0.01f;
                _animator.Play(isMoving ? walkState : idleState, 0, 0f);
                return;
            }

            string stateName = phase switch
            {
                JumpPhase.Start => jumpStartState,
                JumpPhase.Rise => jumpRiseState,
                JumpPhase.Apex => jumpApexState,
                JumpPhase.Fall => jumpFallState,
                JumpPhase.Land => jumpLandState,
                _ => null
            };

            if (!string.IsNullOrEmpty(stateName))
            {
                _animator.Play(stateName, 0, 0f);
            }
        }

        public void SetFacingDirection(bool faceRight)
        {
            if (_facingRight != faceRight)
                Flip();
        }
    }
}
