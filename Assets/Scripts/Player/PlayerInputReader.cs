using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace Samson
{
    public class PlayerInputReader : NetworkBehaviour
    {
        public Vector3 MoveDirection { get; private set; }

        private const KeyCode JUMP_KEY = KeyCode.Space;
        private const KeyCode HOLD_OBJECT_KEY = KeyCode.Mouse0;

        public Action OnJumpInput = delegate { };
        public Action OnHoldObjectInput = delegate { };

        private void Update()
        {
            ReadMovementInput();
            ReadJumpInput();
            ReadHoldObjectInput();
        }   

        private void ReadMovementInput()
        {
            MoveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }

        private void ReadJumpInput()
        {
            if (Input.GetKeyDown(JUMP_KEY))
            {
                OnJumpInput?.Invoke();
            }
        }

        private void ReadHoldObjectInput()
        {
            if (Input.GetKeyDown(HOLD_OBJECT_KEY))
            {
                OnHoldObjectInput?.Invoke();
            }
        }
    }
}
