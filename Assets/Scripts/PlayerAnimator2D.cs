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
        [SerializeField] private string glideState = "Glide";

        [Header("Seuils de détection")]
        [SerializeField] private float apexThreshold = 10f;
        [SerializeField] private float jumpStartDuration = 0.1f;
        [SerializeField] private float jumpRiseDuration = 0.5f;
        [SerializeField] private float landingDuration = 0.1f;

        private PlayerController2D _controller;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private bool _facingRight = false;
        private bool _wasMoving = false;
        private bool _wasGrounded = true;
        private JumpPhase _currentJumpPhase = JumpPhase.Grounded;
        private float _jumpStartTimer = 0f;
        private float _jumpRiseTimer = 0f;
        private float _landingTimer = 0f;
        private bool _wasGliding = false;

        private void Awake()
        {
            _controller = GetComponent<PlayerController2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnJump += HandleJump;
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnJump -= HandleJump;
            }
        }

        private void HandleJump()
        {
            // Relancer l'animation de saut (pour le double saut notamment)
            _currentJumpPhase = JumpPhase.Start;
            _jumpStartTimer = jumpStartDuration;
            _wasGrounded = false; // Évite que UpdateJumpPhase ne détecte une fausse transition
            PlayJumpAnimation(JumpPhase.Start);
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
            bool isGliding = _controller.IsGliding;

            // Gestion du glide (priorité sur les phases de saut)
            if (isGliding && !_wasGliding)
            {
                _animator.Play(glideState, 0, 0f);
            }
            else if (!isGliding && _wasGliding)
            {
                // Sortie du glide → reprendre l'animation de chute
                _animator.Play(jumpFallState, 0, 0f);
            }
            _wasGliding = isGliding;

            // Gestion des phases de saut (seulement si on ne glide pas)
            if (!isGliding)
            {
                UpdateJumpPhase(isGrounded, velocityY);
            }

            // Animation au sol
            if (_currentJumpPhase == JumpPhase.Grounded && !isGliding)
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
                        _jumpRiseTimer = jumpRiseDuration;
                    }
                    else
                    {
                        newPhase = JumpPhase.Fall;
                    }
                }
                // Note: on n'interrompt pas JumpStart même si isGrounded,
                // car on peut encore être détecté au sol juste après le saut
            }
            // Gestion du timer de montée
            else if (_currentJumpPhase == JumpPhase.Rise)
            {
                _jumpRiseTimer -= Time.deltaTime;
                // Timer expiré ou vélocité négative → passer à Apex
                if (_jumpRiseTimer <= 0f || velocityY <= 0)
                {
                    newPhase = JumpPhase.Apex;
                }
                // Si on atterrit pendant le Rise
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
            // En l'air (Apex ou Fall)
            else if (!isGrounded)
            {
                if (velocityY < -apexThreshold)
                {
                    newPhase = JumpPhase.Fall;
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
