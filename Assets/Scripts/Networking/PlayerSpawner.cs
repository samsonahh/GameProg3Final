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
        [SerializeField] private List<Transform> spawns = new();

        public void PlayerJoined(PlayerRef player)
        {
            if (player == Runner.LocalPlayer)
            {
                NetworkObject playerSpawned = Runner.Spawn(playerPrefab, spawns[UnityEngine.Random.Range(0, spawns.Count)].position, Quaternion.identity, player);
                Runner.SetPlayerObject(player, playerSpawned);

                PlayerMovement playerMovement = playerSpawned.GetComponent<PlayerMovement>();
                playerMovement.SetSpawns(spawns);
                playerMovement.Respawn();
            }
        }
    }
}
