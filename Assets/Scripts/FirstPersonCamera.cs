using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform Target { get; set; }
        public float MouseSensitivity = 10f;

        private float verticalRotation;
        private float horizontalRotation;

        private void LateUpdate()
        {
            if (Target == null) return;

            transform.position = Target.position;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            verticalRotation -= mouseY * MouseSensitivity * Time.deltaTime;
            verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);

            horizontalRotation += mouseX * MouseSensitivity * Time.deltaTime;

            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }
    }
}
