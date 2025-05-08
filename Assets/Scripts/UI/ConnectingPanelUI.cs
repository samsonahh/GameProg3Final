using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Samson
{
    public class ConnectingPanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text connectingText;
        [SerializeField] private float dotDelay = 0.25f;

        private Coroutine currentCoroutine;

        private void Awake()
        {
            currentCoroutine = StartCoroutine(ConnectingTextAnimationCoroutine());
        }

        public void StopConnectingUI()
        {
            StopCoroutine(currentCoroutine);
            Destroy(gameObject);
        }

        private IEnumerator ConnectingTextAnimationCoroutine()
        {
            while (true)
            {
                connectingText.text = "Connecting";
                yield return new WaitForSeconds(dotDelay);
                connectingText.text = "Connecting.";
                yield return new WaitForSeconds(dotDelay);
                connectingText.text = "Connecting..";
                yield return new WaitForSeconds(dotDelay);
                connectingText.text = "Connecting...";
                yield return new WaitForSeconds(dotDelay);
            }
        }
    }
}
