using Fusion;
using UnityEngine;

namespace Samson
{
    public class PlayerUISpawner : NetworkBehaviour
    {
        [SerializeField] private PlayerUI playerUIPrefab;

        public override void Spawned()
        {
            if (!HasStateAuthority) return;

            Transform mainCanvasTransform = GameObject.FindGameObjectWithTag("MainCanvas").transform;
            PlayerUI playerUI = Instantiate(playerUIPrefab, mainCanvasTransform);
            playerUI.Init(Runner, gameObject);
        }
    }
}
