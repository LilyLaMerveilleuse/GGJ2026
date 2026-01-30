using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float deceleration = 50f;
        [SerializeField] private bool useAcceleration = true;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 14f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float jumpBufferTime = 0.1f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D _rb;
        private int _jumpsRemaining;
        private float _jumpBufferCounter;
        private float _coyoteTimeCounter;
        private bool _isGrounded;
        private bool _wasGrounded;
        private float _horizontalInput;
        private bool _jumpInputPressed;
        private bool _jumpInputHeld;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving => Mathf.Abs(_rb.velocity.x) > 0.1f;
        public bool IsFalling => _rb.velocity.y < 0;
        public Vector2 Velocity => _rb.velocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.freezeRotation = true;
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
            _horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
                _jumpInputPressed = true;

            _jumpInputHeld = Input.GetButton("Jump");
        }

        private void CheckGround()
        {
            _wasGrounded = _isGrounded;

            // Ne pas considérer grounded si on monte (évite reset des sauts en traversant one-way platforms)
            if (_rb.velocity.y > 0.1f)
            {
                _isGrounded = false;
                return;
            }

            if (groundCheck != null)
            {
                _isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
            }
            else
            {
                _isGrounded = Physics2D.OverlapBox(
                    (Vector2)transform.position + Vector2.down * 0.5f,
                    groundCheckSize, 0f, groundLayer);
            }

            if (_isGrounded && !_wasGrounded)
                OnLand();
        }

        private void OnLand()
        {
            _jumpsRemaining = maxJumps;
        }

        private void HandleTimers()
        {
            if (_isGrounded)
                _coyoteTimeCounter = coyoteTime;
            else
                _coyoteTimeCounter -= Time.deltaTime;
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
            return _coyoteTimeCounter > 0f || _jumpsRemaining > 0;
        }

        private void Jump()
        {
            if (_coyoteTimeCounter > 0f)
            {
                _jumpsRemaining = maxJumps - 1;
            }
            else
            {
                _jumpsRemaining--;
            }

            _coyoteTimeCounter = 0f;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        }

        private void Move()
        {
            float targetSpeed = _horizontalInput * moveSpeed;

            if (useAcceleration)
            {
                float accelRate = Mathf.Abs(_horizontalInput) > 0.01f ? acceleration : deceleration;
                float speedDiff = targetSpeed - _rb.velocity.x;
                float movement = speedDiff * accelRate * Time.fixedDeltaTime;
                _rb.velocity = new Vector2(_rb.velocity.x + movement, _rb.velocity.y);
            }
            else
            {
                _rb.velocity = new Vector2(targetSpeed, _rb.velocity.y);
            }
        }

        private void ApplyJumpPhysics()
        {
            if (_rb.velocity.y < 0)
            {
                _rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime);
            }
            else if (_rb.velocity.y > 0 && !_jumpInputHeld)
            {
                _rb.velocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 checkPos = groundCheck != null
                ? groundCheck.position
                : transform.position + Vector3.down * 0.5f;
            Gizmos.DrawWireCube(checkPos, groundCheckSize);
        }
    }
}
