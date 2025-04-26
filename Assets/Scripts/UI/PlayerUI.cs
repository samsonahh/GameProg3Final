using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Samson
{
    public class PlayerUI : MonoBehaviour
    {
        private NetworkRunner networkRunner;

        [SerializeField] private TMP_Text pingText;
        private float pingUpdateTimer = 0f;
        private const float pingUpdateInterval = 1f; // seconds

        public void Init(NetworkRunner networkRunner)
        {
            this.networkRunner = networkRunner;
        }

        private void Update()
        {
            UpdatePingText();
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
    }
}
