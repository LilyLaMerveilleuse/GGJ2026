using Constants;
using UnityEngine;

namespace Bundles.SimplePlatformer2D.Scripts
{
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

        [Header("Animation States (pour forcer les transitions)")]
        [SerializeField] private string idleState = "Idle";
        [SerializeField] private string walkState = "Walk";

        private PlayerController2D _controller;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private bool _facingRight = false;
        private bool _wasMoving = false;

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

            // Force le changement d'animation immédiat lors des transitions idle/walk
            if (isMoving != _wasMoving && _controller.IsGrounded)
            {
                _animator.Play(isMoving ? walkState : idleState, 0, 0f);
            }
            _wasMoving = isMoving;

            _animator.SetFloat(speedParam, Mathf.Abs(horizontalInput));
            _animator.SetBool(groundedParam, _controller.IsGrounded);
            _animator.SetFloat(verticalVelocityParam, _controller.Velocity.y);
        }

        public void SetFacingDirection(bool faceRight)
        {
            if (_facingRight != faceRight)
                Flip();
        }
    }
}
