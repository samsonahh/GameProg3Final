using Fusion;
using System;
using UnityEngine;

namespace Samson
{
    public class PlayerMovement : NetworkBehaviour
    {
        private CharacterController controller;

        [SerializeField] private GameObject modelObject;

        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        public Camera Camera { get; private set; }

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 2f;
        [SerializeField] private float rotationSpeed = 10f;
        [Networked]
        public float NetworkedSpeedModifier { get; private set; } = 1f;
        public float MoveSpeed => baseSpeed * NetworkedSpeedModifier;

        [Header("Sprint")]
        [SerializeField] private float sprintModifier = 1.5f;
        private bool sprintPressed;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 2f;
        private float yVelocity;
        private bool jumpPressed;
        private bool isJumping;
        public Action OnGrounded = delegate { };
        public Action OnJump = delegate { };
        public Action OnFalling = delegate { };

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Camera = Camera.main;
                Camera.GetComponent<FirstPersonCamera>().Target = cameraTarget;
                Cursor.lockState = CursorLockMode.Locked;

                SetLayerRecursively(modelObject, LayerMask.NameToLayer("HideFromLocal"));
            }
        }

        void Update()
        {
            ReadInputs();
        }

        public override void FixedUpdateNetwork()
        {
            // FixedUpdateNetwork is only executed on the StateAuthority

            if (controller.isGrounded)
            {
                yVelocity = -1;
                isJumping = false;
                OnGrounded.Invoke();
            }

            Vector3 moveInput = GetMoveInput();
            HandleSpeedModifier(moveInput, sprintPressed);

            Vector3 moveAmount = GetMoveAmount(moveInput);

            yVelocity += Physics.gravity.y * Runner.DeltaTime;
            HandleNetworkJump();

            controller.Move(moveAmount + yVelocity * Runner.DeltaTime * Vector3.up);

            HandleNetworkRotation(moveAmount);
        }

        private void ReadInputs()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpPressed = true;
            }

            sprintPressed = Input.GetKey(KeyCode.LeftShift);
        }

        private Vector3 GetMoveInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector3(horizontal, 0, vertical);
        }

        private Vector3 GetMoveAmount(Vector3 moveInput)
        {
            Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.eulerAngles.y, 0);
            
            return cameraRotationY * moveInput.normalized * Runner.DeltaTime * MoveSpeed;
        }

        private void HandleNetworkJump()
        {
            if (jumpPressed && controller.isGrounded)
            {
                yVelocity += Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
                isJumping = true;
                OnJump.Invoke();
            }

            if(!isJumping && !controller.isGrounded)
            {
                OnFalling.Invoke();
            }

            jumpPressed = false;
        }

        private void HandleNetworkRotation(Vector3 moveAmount)
        {
            if (moveAmount != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveAmount, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
            }
        }

        private void HandleSpeedModifier(Vector3 moveInput, bool sprintPressed)
        {
            if (!HasStateAuthority) return;

            if (moveInput == Vector3.zero)
            {
                NetworkedSpeedModifier = 0f;
                return;
            }

            NetworkedSpeedModifier = sprintPressed ? sprintModifier : 1f;
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
