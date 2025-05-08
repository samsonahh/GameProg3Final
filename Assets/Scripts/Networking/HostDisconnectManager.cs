using Fusion;
using UnityEngine;

namespace Samson
{
    public class HostDisconnectManager : NetworkBehaviour
    {
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Only non-host clients should care if the master client left
            if (player != PlayerRef.MasterClient)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.OpenURL(Application.absoluteURL);
#endif
            }
        }
    }
}
