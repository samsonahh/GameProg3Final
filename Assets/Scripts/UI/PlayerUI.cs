using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Samson
{
    public class PlayerUI : MonoBehaviour
    {
        public static PlayerUI Instance { get; private set; }

        private NetworkRunner networkRunner;

        [Header("Ping")]
        [SerializeField] private TMP_Text pingText;
        private float pingUpdateTimer = 0f;
        private const float pingUpdateInterval = 1f; // seconds

        [Header("Menu")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject gameplayPanel;
        public bool IsMenuOpen { get; private set; } = false;

        public void Init(NetworkRunner networkRunner)
        {
            if(Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            this.networkRunner = networkRunner;

            OpenMenu(false);
        }

        private void Update()
        {
            UpdatePingText();

            ReadMenuInput();
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
    }
}
