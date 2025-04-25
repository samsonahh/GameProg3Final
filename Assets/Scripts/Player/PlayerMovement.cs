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
        [Networked] public float NetworkedSpeedModifier { get; private set; } = 1f;
        [Networked] public Vector2 NetworkedMoveInput { get; private set; }
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

        [field: Header("Dance")]
        public bool IsDancing { get; private set; }
        private bool dancePressed;
        public Action<bool> OnDance = delegate { };

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
            NetworkedMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector3 moveAmount = GetMoveAmount(moveInput);

            yVelocity += Physics.gravity.y * Runner.DeltaTime;
            HandleNetworkJump();

            controller.Move(moveAmount + yVelocity * Runner.DeltaTime * Vector3.up);

            HandleNetworkRotation();
            HandleNetworkDance();
        }

        private void ReadInputs()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                dancePressed = true;
            }

            if (!IsDancing)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpPressed = true;
                }

                sprintPressed = Input.GetKey(KeyCode.LeftShift);
            }
            else
            {
                sprintPressed = false;
            }
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

        private void HandleNetworkRotation()
        {
            Quaternion targetRotation = Quaternion.Euler(0f, Camera.transform.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
        }

        private void HandleSpeedModifier(Vector3 moveInput, bool sprintPressed)
        {
            if (!HasStateAuthority) return;

            if (IsDancing)
            {
                NetworkedSpeedModifier = 0f;
                return;
            }

            if (moveInput == Vector3.zero)
            {
                NetworkedSpeedModifier = 0f;
                return;
            }

            NetworkedSpeedModifier = sprintPressed ? sprintModifier : 1f;
        }

        private void HandleNetworkDance()
        {
            if (dancePressed && controller.isGrounded)
            {
                IsDancing = !IsDancing;
                OnDance.Invoke(IsDancing);
            }
            dancePressed = false;
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
