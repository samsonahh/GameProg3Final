using Fusion;
using UnityEngine;

namespace Samson
{
    public class HostDisconnectManager : SimulationBehaviour
    {
        [SerializeField] private GameObject disconnectUI;

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player} has left. Player is master client: {runner.IsSharedModeMasterClient}");
            if (runner.IsSharedModeMasterClient)
            {
                Runner.Shutdown();
                PlayerUI.Instance.gameObject.SetActive(false);
                PlayerUI.Instance.PlayerObject.SetActive(false);
                disconnectUI.SetActive(true);
            }
        }
    }
}
