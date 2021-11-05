using System;
using UnityEngine;

namespace Controller {
    public class PlayerController : MonoBehaviour {
        public float moveSpeed = 1f;
        public float jumpForce = 1f;

        private Rigidbody2D _rigidbody;
        private Animator _animator;

        private float _horizontal;

        private bool _onGround;
        private bool _facingRight;
        
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        private void FixedUpdate() {
            if (_rigidbody == null) {
                return;
            }

            _horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = DetermineVerticalForce();

            FlipPlayer(_horizontal);

            _rigidbody.velocity = new Vector2(_horizontal * moveSpeed, vertical);
        }

        private void Update() {
            if (_animator == null) {
                return;
            }
            
            _animator.SetFloat(Horizontal, _horizontal);
        }

        private void FlipPlayer(float horizontal) {
            if ((horizontal < 0 && _facingRight) || (horizontal > 0 && !_facingRight)) {
                _facingRight = !_facingRight;
                _rigidbody.transform.Rotate(new Vector3(0, 180, 0));
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
