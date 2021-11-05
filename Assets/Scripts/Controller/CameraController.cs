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

        private Vector3 _velocity = Vector3.zero;
        
        private void LateUpdate() {
            if (target == null) {
                return;
            }

            Vector3 desiredPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, smoothRate);
        }
    }
}

