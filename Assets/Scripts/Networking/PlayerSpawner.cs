using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;

        public void PlayerJoined(PlayerRef player)
        {
            if (player == Runner.LocalPlayer)
            {
                NetworkObject playerSpawned = Runner.Spawn(playerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
                Runner.SetPlayerObject(player, playerSpawned);
                Debug.Log($"Spawned player prefab for {player}. Player spawned is null: {playerSpawned == null}");
            }
        }
    }
}
