using System;
using Constants;
using Masks;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        public event Action OnJump;
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 18f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float fallMultiplier = 5f;
        [SerializeField] private float riseMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 4f;
        [SerializeField] private float fastFallMultiplier = 8f;

        [Header("Glide")]
        [SerializeField] private float glideSpeed = 2f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckWidth = 0.8f;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private float _jumpBufferCounter;
        private float _coyoteTimeCounter;
        private bool _isGrounded;
        private bool _wasGrounded;
        private float _horizontalInput;
        private bool _jumpInputPressed;
        private bool _jumpInputHeld;
        private bool _fastFallHeld;
        private int _airJumpsRemaining;
        private bool _isGliding;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving => Mathf.Abs(_rb.velocity.x) > GameConstants.Physics.VelocityThreshold;
        public bool IsFalling => _rb.velocity.y < 0;
        public bool IsGliding => _isGliding;
        public Vector2 Velocity => _rb.velocity;
        public bool ControlsEnabled { get; set; } = true;

        public static PlayerController2D Instance => _instance;
        private static PlayerController2D _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _airJumpsRemaining = 0;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            GatherInput();
            CheckGround();
            HandleTimers();
            HandleJumpBuffer();
        }

        private void FixedUpdate()
        {
            Move();
            ApplyJumpPhysics();
        }

        private void GatherInput()
        {
            if (!ControlsEnabled)
            {
                _horizontalInput = 0f;
                _jumpInputPressed = false;
                _jumpInputHeld = false;
                _fastFallHeld = false;
                return;
            }

            _horizontalInput = Input.GetAxisRaw(GameConstants.Input.Horizontal);

            if (Input.GetButtonDown(GameConstants.Input.Jump))
                _jumpInputPressed = true;

            _jumpInputHeld = Input.GetButton(GameConstants.Input.Jump);
            _fastFallHeld = Input.GetAxisRaw(GameConstants.Input.Vertical) < -GameConstants.Input.AxisThreshold;
        }

        private void CheckGround()
        {
            _wasGrounded = _isGrounded;

            // Ne pas considérer grounded si on monte (évite reset des sauts en traversant one-way platforms)
            if (_rb.velocity.y > GameConstants.Physics.VelocityThreshold)
            {
                _isGrounded = false;
                return;
            }

            Vector2 origin = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position + Vector2.down * 0.5f;

            _isGrounded = Physics2D.BoxCast(origin, new Vector2(groundCheckWidth, GameConstants.Physics.GroundCheckBoxHeight), 0f, Vector2.down, groundCheckDistance, groundLayer);

            if (_isGrounded && !_wasGrounded)
                OnLand();
        }

        private void OnLand()
        {
            // Réinitialiser les sauts en l'air
            _airJumpsRemaining = GetMaxAirJumps();
        }

        private int GetMaxAirJumps()
        {
            int airJumps = 0;

            // Le masque DoubleJump donne 1 saut en l'air
            if (MaskInventory.Instance != null && MaskInventory.Instance.HasMask(MaskType.DoubleJump))
            {
                airJumps += 1;
            }

            return airJumps;
        }

        private void HandleTimers()
        {
            if (_isGrounded)
            {
                _coyoteTimeCounter = coyoteTime;
                // Réinitialiser les sauts en l'air tant qu'on est au sol
                _airJumpsRemaining = GetMaxAirJumps();
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        private void HandleJumpBuffer()
        {
            if (_jumpInputPressed)
            {
                _jumpBufferCounter = jumpBufferTime;
                _jumpInputPressed = false;
            }
            else
            {
                _jumpBufferCounter -= Time.deltaTime;
            }

            if (_jumpBufferCounter > 0f && CanJump())
            {
                Jump();
                _jumpBufferCounter = 0f;
            }
        }

        private bool CanJump()
        {
            // Saut normal (au sol ou coyote time)
            if (_coyoteTimeCounter > 0f)
            {
                return true;
            }

            // Saut en l'air (double saut)
            if (_airJumpsRemaining > 0)
            {
                return true;
            }

            return false;
        }

        private void Jump()
        {
            // Déterminer si c'est un saut au sol ou en l'air
            bool isGroundJump = _coyoteTimeCounter > 0f;

            if (isGroundJump)
            {
                _coyoteTimeCounter = 0f;
            }
            else
            {
                // Saut en l'air - consommer un saut
                _airJumpsRemaining--;
            }

            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            OnJump?.Invoke();
        }

        private void Move()
        {
            _rb.velocity = new Vector2(_horizontalInput * moveSpeed, _rb.velocity.y);
        }

        private void ApplyJumpPhysics()
        {
            // Vérifier si on peut planer
            bool canGlide = MaskInventory.Instance != null &&
                            MaskInventory.Instance.HasMask(MaskType.Glide) &&
                            !_isGrounded &&
                            _jumpInputHeld &&
                            !_fastFallHeld;

            if (_rb.velocity.y < 0)
            {
                // Plané : limiter la vitesse de chute
                if (canGlide)
                {
                    _isGliding = true;
                    if (_rb.velocity.y < -glideSpeed)
                    {
                        _rb.velocity = new Vector2(_rb.velocity.x, -glideSpeed);
                    }
                }
                else
                {
                    _isGliding = false;
                    // Fast fall si le joueur maintient bas
                    float multiplier = _fastFallHeld ? fastFallMultiplier : fallMultiplier;
                    _rb.velocity += Vector2.up * (Physics2D.gravity.y * (multiplier - 1) * Time.fixedDeltaTime);
                }
            }
            else
            {
                _isGliding = false;

                if (_rb.velocity.y > 0 && _fastFallHeld)
                {
                    // Annule la montée si le joueur maintient bas
                    _rb.velocity += Vector2.up * (Physics2D.gravity.y * (fastFallMultiplier - 1) * Time.fixedDeltaTime);
                }
                else if (_rb.velocity.y > 0 && !_jumpInputHeld)
                {
                    _rb.velocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
                }
                else if (_rb.velocity.y > 0 && _jumpInputHeld)
                {
                    _rb.velocity += Vector2.up * (Physics2D.gravity.y * (riseMultiplier - 1) * Time.fixedDeltaTime);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 checkPos = groundCheck != null
                ? groundCheck.position
                : transform.position + Vector3.down * 0.5f;
            Gizmos.DrawWireCube(checkPos + Vector3.down * groundCheckDistance * 0.5f, new Vector3(groundCheckWidth, groundCheckDistance, 0f));
        }
    }
}
