using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class PlayerUISpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject playerUIPrefab;

        public override void Spawned()
        {
            if (!HasStateAuthority) return;

            Transform mainCanvasTransform = GameObject.FindGameObjectWithTag("MainCanvas").transform;
            Instantiate(playerUIPrefab, mainCanvasTransform);
        }
    }
}
