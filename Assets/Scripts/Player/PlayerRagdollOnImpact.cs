using Fusion;
using System.Collections;
using UnityEngine;

namespace Samson
{
    public class PlayerRagdollOnImpact : NetworkBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;

        [SerializeField] private float ragdollDuration = 3f;
        [SerializeField] private float impactThreshold = 3f;

        private void Update()
        {
            transform.localPosition = Vector3.zero;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (!HasStateAuthority) return;
            if (playerMovement.IsRagdolled) return;

            Rigidbody hitRigidbody = collision.attachedRigidbody;
            if(hitRigidbody == null) return;

            float impactForce = hitRigidbody.velocity.magnitude * hitRigidbody.mass;

            Debug.Log($"Hit by {collision.gameObject.name} with force: {impactForce}");

            if (impactForce >= impactThreshold)
            {
                TriggerRagdollRpc();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void TriggerRagdollRpc()
        {
            if (playerMovement.IsRagdolled) return;

            StartCoroutine(HandleRagdoll());
        }

        private IEnumerator HandleRagdoll()
        {
            playerMovement.RagdollPlayer(true);
            yield return new WaitForSeconds(ragdollDuration);
            playerMovement.RagdollPlayer(false);
        }
    }
}
