using Fusion;
using UnityEngine;

namespace Samson
{
    public class Health : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(OnHealthChanged))]
        public float NetworkedHealth { get; set; } = 100f;

        private void OnHealthChanged()
        {
            Debug.Log($"Health changed to {NetworkedHealth}");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void DealDamageRpc(float damage)
        {
            Debug.Log($"Received damage: {damage}");
            NetworkedHealth -= damage;
        }
    }
}
