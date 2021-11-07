using System;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Controller {
    public class PlayerController : MonoBehaviour {
        public float moveSpeed = 1f;
        public float jumpForce = 1f;

        public float attackSpeed = 1f;

        public TileData selectedTile;

        private Rigidbody2D _rigidbody;
        private Animator _animator;

        private float _horizontal;
        private float _interactionRange = 10f;
        
        private bool _onGround;
        private bool _facingRight;
        private bool _hit;
        private bool _place;

        private float _attackCountdownTime;
        private float _placeCountdownTime;
        
        private static readonly int Horizontal = Animator.StringToHash("Horizontal");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private static readonly int AttackSpeedMultiplier = Animator.StringToHash("attackSpeedMultiplier");

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();

            _animator.SetFloat(AttackSpeedMultiplier, attackSpeed);
            
        }

        private void FixedUpdate() {
            HandleMovement();
        }
        
        private void Update() {
            HandleTimers();
            
            HandleInput();
            HandleAnimation();


            Vector2 mousePosition = World.Instance.mousePosition;

            int xPos = (int) mousePosition.x;
            int yPos = (int) mousePosition.y;
            if (_hit && World.Instance.GetDistanceToMouse(transform.position) <= _interactionRange) {
                if (_place) {
                    PlaceTile(xPos, yPos);
                }
                else {
                    DestroyTile(xPos, yPos);
                }
            }
        }
        
        private void HandleTimers() {
            _attackCountdownTime -= Time.deltaTime;
            _placeCountdownTime -= Time.deltaTime;
        }

        private void DestroyTile(int xPos, int yPos) {
            if (_attackCountdownTime <= 0f) {
                Tile tile = World.Instance.TileAtPosition(xPos, yPos);
                if (tile != null && !tile.data.inBackground) {
                    World.Instance.RemoveTile(tile);
                }

                // Debug.Log($"Removing tile at: {xPos},{yPos}");

                _attackCountdownTime = 1 / attackSpeed;
            }
        }

        private void PlaceTile(int xPos, int yPos) {
            if (_placeCountdownTime <= 0f) {
                World.Instance.PlaceTile(selectedTile, xPos, yPos, selectedTile.isSolid); //    Only replace with solid tiles

                // Debug.Log("Placing tile: " + selectedTile);

                _placeCountdownTime = 1 / attackSpeed;
            }
        }

        private void HandleInput() {
            _hit = Input.GetMouseButton(0);

            if (Input.GetMouseButtonDown(1)) {
                _place = !_place;

                World.Instance.SetPlace(_place);
            }
        }

        private void HandleMovement() {
            if (_rigidbody == null) {
                return;
            }
            
            _horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = DetermineVerticalForce();

            FlipPlayer(_horizontal);

            _rigidbody.velocity = new Vector2(_horizontal * moveSpeed, vertical);
        }

        private void HandleAnimation() {
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
