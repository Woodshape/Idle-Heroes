using System;
using UnityEngine;

namespace Controller {
    public class PlayerController : MonoBehaviour {
        public float moveSpeed = 1f;
        public float jumpForce = 1f;

        public float attackSpeed = 1f;

        private Rigidbody2D _rigidbody;
        private Animator _animator;

        private float _horizontal;
        
        private bool _onGround;
        private bool _facingRight;
        private bool _hit;
        
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int AttackSpeedMultiplier = Animator.StringToHash("attackSpeedMultiplier");

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            
            _animator.SetFloat(AttackSpeedMultiplier, attackSpeed);
        }

        private void FixedUpdate() {
            if (_rigidbody == null) {
                return;
            }

            _horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = DetermineVerticalForce();

            FlipPlayer(_horizontal);

            _rigidbody.velocity = new Vector2(_horizontal * moveSpeed, vertical);

            _hit = Input.GetMouseButton(0);
        }

        private void Update() {
            if (_animator == null) {
                return;
            }
            
            _animator.SetFloat(Horizontal, _horizontal);
            
            float animatorAttackSpeed = _animator.GetFloat(AttackSpeedMultiplier);
            //  If animation speed and attack speed differ by more than .1, update the animation speed to the attack speed
            if (Math.Abs(animatorAttackSpeed - attackSpeed) >= .1f) {
                _animator.SetFloat(AttackSpeedMultiplier, attackSpeed);
            }
            
            _animator.SetBool(Hit, _hit);
        }

        private void FlipPlayer(float horizontal) {
            if ((horizontal < 0 && _facingRight) || (horizontal > 0 && !_facingRight)) {
                _facingRight = !_facingRight;
                // _rigidbody.transform.Rotate(new Vector3(0, 180, 0));
                _rigidbody.transform.localScale = new Vector3(_facingRight ? -1 : 1, 1, 0);
            }
        }

        private float DetermineVerticalForce() {
            float jump = Input.GetAxisRaw("Jump");

            float force = _rigidbody.velocity.y;

            if (_onGround && jump > 0.5f) {
                force = jumpForce;
            }

            return force;
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (other.CompareTag("Ground")) {
                _onGround = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Ground")) {
                _onGround = false;
            }
        }
    }
}
