using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class CameraController : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private PlayerModelManager playerModelManager;

        [SerializeField] private float distanceFromTarget = 5f;
        [SerializeField] private float mouseSensitivity = 10f;

        public float DefaultFOV { get; private set; }

        private float verticalRotation;
        private float horizontalRotation;

        private void Awake()
        {
            DefaultFOV = Camera.main.fieldOfView;
        }

        private void Update()
        {
            
        }

        private void LateUpdate()
        {
            if (playerModelManager == null) return;

            ReadCameraInputs();

            HandleFirstPersonCamera();
            HandleThirdPersonCamera();
        }

        private void ReadCameraInputs()
        {
            if (PlayerUI.Instance != null)
            {
                if (PlayerUI.Instance.IsMenuOpen) return;
            }
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            verticalRotation -= mouseY * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            horizontalRotation += mouseX * mouseSensitivity;
        }

        private void HandleFirstPersonCamera()
        {
            if (!IsFirstPersonCameraActive()) return;

            if (playerModelManager.CurrentModelObject.gameObject.layer != LayerMask.NameToLayer("HideFromLocal"))
            {
                playerModelManager.CurrentModelObject.HideFromLocal();
            }

            transform.position = playerModelManager.CurrentModelObject.HeadTransform.position;
            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }

        private bool IsFirstPersonCameraActive()
        {
            return !playerMovement.IsRagdolled && !playerMovement.IsDancing;
        }

        private void HandleThirdPersonCamera()
        {
            if (IsFirstPersonCameraActive()) return;

            if (playerModelManager.CurrentModelObject.gameObject.layer == LayerMask.NameToLayer("HideFromLocal"))
            {
                playerModelManager.CurrentModelObject.ShowToAll();
            }

            Quaternion cameraRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);

            Vector3 offset = cameraRotation * (distanceFromTarget * Vector3.back);
            transform.position = playerModelManager.CurrentModelObject.HeadTransform.position + offset;
            transform.rotation = cameraRotation;
        }

        public void AssignPlayerTarget(PlayerModelManager modelManager)
        {
            playerModelManager = modelManager;
            playerMovement = modelManager.GetComponent<PlayerMovement>();
        }

        public void ChangeFOV(float targetFOV)
        {
            Camera.main.DOKill();
            Camera.main.DOFieldOfView(targetFOV, 0.5f).SetEase(Ease.OutQuad);
        }

        public void ChangeMouseSensitivity(float newSensitivity)
        {
            mouseSensitivity = newSensitivity;
        }
    }
}
