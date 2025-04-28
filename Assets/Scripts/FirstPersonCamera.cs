using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class FirstPersonCamera : MonoBehaviour
    {
        private PlayerModelManager playerModelManager;
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
            transform.position = playerModelManager.CurrentModelObject.HeadTransform.position;

            if (PlayerUI.Instance != null)
            {
                if(PlayerUI.Instance.IsMenuOpen) return;
            }
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            verticalRotation -= mouseY * mouseSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            horizontalRotation += mouseX * mouseSensitivity;

            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }

        public void AssignPlayerTarget(PlayerModelManager modelManager)
        {
            playerModelManager = modelManager;
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
