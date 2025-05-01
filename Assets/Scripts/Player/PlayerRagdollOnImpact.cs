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
            if (!Runner.IsSharedModeMasterClient)
            {
                Debug.LogWarning($"Player {Runner.LocalPlayer} is not the local player, ignoring collision.");
                return;
            }
            if (playerMovement.IsRagdolled) return;

            Rigidbody hitBody = collision.attachedRigidbody;
            if (hitBody == null)
            {
                Debug.Log($"Hit object {collision.gameObject.name} has no rigidbody, ignoring.");
                return;
            }

            DraggableObject draggableObject = hitBody.GetComponent<DraggableObject>();
            if(draggableObject == null)
            {
                Debug.Log($"Hit object {collision.gameObject.name} is not draggable object");
                return;
            }

            float impactForce = hitBody.velocity.magnitude * hitBody.mass;

            Debug.Log($"Hit by {collision.gameObject.name} with force: {impactForce}");

            if (impactForce >= impactThreshold)
            {
                TriggerRagdollRpc();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void TriggerRagdollRpc()
        {
            if (playerMovement.IsRagdolled)
            {
                Debug.LogWarning($"{playerMovement.gameObject.name} Ragdoll already active, ignoring RPC trigger.");
                return;
            }

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
