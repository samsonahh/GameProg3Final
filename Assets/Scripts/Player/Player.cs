using Fusion;
using System;
using UnityEngine;

namespace Samson
{
    public class Player : NetworkBehaviour
    {
        public Camera Camera;

        private Vector3 velocity;
        private bool jumpPressed;

        private CharacterController controller;

        public float PlayerSpeed = 2f;

        public float JumpForce = 5f;
        public float GravityValue = -9.81f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Camera = Camera.main;
                Camera.GetComponent<FirstPersonCamera>().Target = transform;
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                jumpPressed = true;
            }
        }

        public override void FixedUpdateNetwork()
        {
            // FixedUpdateNetwork is only executed on the StateAuthority

            if (controller.isGrounded)
            {
                velocity = new Vector3(0, -1, 0);
            }

            Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.eulerAngles.y, 0);
            Vector3 move = cameraRotationY * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * Runner.DeltaTime * PlayerSpeed;

            velocity.y += GravityValue * Runner.DeltaTime;
            if (jumpPressed && controller.isGrounded)
            {
                velocity.y += JumpForce;
            }
            controller.Move(move + velocity * Runner.DeltaTime);

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            jumpPressed = false;
        }
    }
}
