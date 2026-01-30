using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 18f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float fallMultiplier = 5f;
        [SerializeField] private float riseMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 4f;

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

        public bool IsGrounded => _isGrounded;
        public bool IsMoving => Mathf.Abs(_rb.velocity.x) > 0.1f;
        public bool IsFalling => _rb.velocity.y < 0;
        public Vector2 Velocity => _rb.velocity;

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

            Vector2 origin = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)transform.position + Vector2.down * 0.5f;

            _isGrounded = Physics2D.BoxCast(origin, new Vector2(groundCheckWidth, 0.02f), 0f, Vector2.down, groundCheckDistance, groundLayer);

            if (_isGrounded && !_wasGrounded)
                OnLand();
        }

        private void OnLand()
        {
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
            return _coyoteTimeCounter > 0f;
        }


        private void Jump()
        {
            _coyoteTimeCounter = 0f;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        }

        private void Move()
        {
            _rb.velocity = new Vector2(_horizontalInput * moveSpeed, _rb.velocity.y);
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
            else if (_rb.velocity.y > 0 && _jumpInputHeld)
            {
                _rb.velocity += Vector2.up * (Physics2D.gravity.y * (riseMultiplier - 1) * Time.fixedDeltaTime);
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
