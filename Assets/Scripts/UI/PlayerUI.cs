using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Samson.PlayerModelManager;

namespace Samson
{
    public class PlayerUI : MonoBehaviour
    {
        public static PlayerUI Instance { get; private set; }

        private NetworkRunner networkRunner;
        private GameObject playerObject;
        
        private CameraController firstPersonCamera;
        private PlayerMovement playerMovement;
        private PlayerModelManager playerModelManager;

        [SerializeField] private GameObject crossHairObject;

        [Header("Ping")]
        [SerializeField] private TMP_Text pingText;
        private float pingUpdateTimer = 0f;
        private const float pingUpdateInterval = 1f; // seconds

        [Header("Menu")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TMP_Text sensitivityText;
        [SerializeField] private TMP_Dropdown modelDropdown;
        public bool IsMenuOpen { get; private set; } = false;

        public void Init(NetworkRunner networkRunner, GameObject playerObject)
        {
            if(Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            this.networkRunner = networkRunner;
            this.playerObject = playerObject;
            firstPersonCamera = Camera.main.GetComponent<CameraController>();
            playerModelManager = playerObject.GetComponent<PlayerModelManager>();
            playerMovement = playerObject.GetComponent<PlayerMovement>();

            OpenMenu(false);

            SetupModelsDropdown();
        }

        private void Update()
        {
            UpdatePingText();
            HandleCrosshairVisibility();

            ReadMenuInput();
            ReadMouseSensitivitySlider();
        }

        private void UpdatePingText()
        {
            pingUpdateTimer += Time.deltaTime;

            if (pingUpdateTimer >= pingUpdateInterval)
            {
                pingUpdateTimer = 0f;
                double pingMs = networkRunner.GetPlayerRtt(PlayerRef.None) * 1000f;
                pingText.text = $"{pingMs:F0} ms";
            }
        }

        private void HandleCrosshairVisibility()
        {
            crossHairObject.SetActive(!playerMovement.IsDancing);
        }

        private void ReadMenuInput()
        {
            if(!Input.GetKeyDown(KeyCode.Escape)) return;

            OpenMenu(!IsMenuOpen);
        }

        private void OpenMenu(bool isOpened)
        {
            IsMenuOpen = isOpened;
            if (isOpened)
            {
                LockCursor(false);
                menuPanel.SetActive(true);
                gameplayPanel.SetActive(false);
            }
            else
            {
                LockCursor(true);
                menuPanel.SetActive(false);
                gameplayPanel.SetActive(true);
            }
        }

        private void LockCursor(bool isLocked)
        {
            if (isLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void OnCloseButtonClicked()
        {
            OpenMenu(false);
        }

        private void ReadMouseSensitivitySlider()
        {
            float sliderValue = sensitivitySlider.value;
            firstPersonCamera.ChangeMouseSensitivity(sliderValue);

            sensitivityText.text = $"{sliderValue:F2}";
        }

        private void SetupModelsDropdown()
        {
            modelDropdown.ClearOptions();

            var options = Enum.GetNames(typeof(PlayerModelManager.Model)).ToList();
            modelDropdown.AddOptions(options);

            modelDropdown.onValueChanged.AddListener(OnModelSelected);
        }

        private void OnModelSelected(int option)
        {
            Model selectedModel = (Model)option;
            playerModelManager.ChangeModel(selectedModel);
        }
    }
}
