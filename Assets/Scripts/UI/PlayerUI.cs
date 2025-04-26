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
            if (networkRunner == null) return;

            double pingMs = networkRunner.GetPlayerRtt(PlayerRef.None) * 1000f;
            pingText.text = $"{pingMs:F0} ms";
        }
    }
}
