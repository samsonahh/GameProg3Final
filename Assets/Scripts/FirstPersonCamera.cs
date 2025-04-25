using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class FirstPersonCamera : MonoBehaviour
    {
        public Transform Target { get; set; }
        [SerializeField] private float mouseSensitivity = 10f;

        private float verticalRotation;
        private float horizontalRotation;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Plus))
            {
                mouseSensitivity += 0.25f;
            }

            if(Input.GetKeyDown(KeyCode.Minus))
            {
                mouseSensitivity -= 0.25f;
            }

            mouseSensitivity = Mathf.Clamp(mouseSensitivity, 1f, 100f);
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            transform.position = Target.position;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            verticalRotation -= mouseY * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            horizontalRotation += mouseX * mouseSensitivity;

            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }
    }
}
