using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Controller {
    public class CameraController : MonoBehaviour {
        public Transform target;

        public Vector3 offset;

        [PropertyRange(0f, 1f)]
        public float smoothRate;
        
        [PropertyRange(0f, 10f)]
        public float zoomFactor;
        public float minZoom;
        public float maxZoom;
        public float zoomSpeed;

        private Camera _camera;
        private float _zoomTarget;

        private Vector3 _velocity = Vector3.zero;

        private void Awake() {
            _camera = GetComponent<Camera>();
            _zoomTarget = _camera.orthographicSize;
        }

        private void Update() {
            if (target == null || _camera == null) {
                return;
            }

            Vector3 desiredPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothRate);

            Zoom();
        }
        
        private void Zoom() {
            float scrollData = Input.GetAxis("Mouse ScrollWheel");

            _zoomTarget -= scrollData * zoomFactor;
            _zoomTarget = Mathf.Clamp(_zoomTarget, minZoom, maxZoom);

            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _zoomTarget, zoomSpeed * Time.deltaTime);
        }
    }
}

