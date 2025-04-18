﻿using Fusion;
using System;
using UnityEngine;

namespace Samson
{
    public class Player : NetworkBehaviour
    {
        private Vector3 _velocity;
        private bool _jumpPressed;

        private CharacterController _controller;

        public float PlayerSpeed = 2f;

        public float JumpForce = 5f;
        public float GravityValue = -9.81f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                _jumpPressed = true;
            }
        }

        public override void FixedUpdateNetwork()
        {
            // FixedUpdateNetwork is only executed on the StateAuthority

            if (_controller.isGrounded)
            {
                _velocity = new Vector3(0, -1, 0);
            }

            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

            _velocity.y += GravityValue * Runner.DeltaTime;
            if (_jumpPressed && _controller.isGrounded)
            {
                _velocity.y += JumpForce;
            }
            _controller.Move(move + _velocity * Runner.DeltaTime);

            if (move != Vector3.zero)
            {
                gameObject.transform.forward = move;
            }

            _jumpPressed = false;
        }
    }
}
