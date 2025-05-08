using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class PlayerRagdollOnImpact : NetworkBehaviour
    {
        private CapsuleCollider capsuleCollider;

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private RagdollEnabler ragdollEnabler;
        [SerializeField] private NetworkObject networkObject;

        [SerializeField] private float ragdollDuration = 3f;
        [SerializeField] private float impactThreshold = 3f;
        [SerializeField] private float impactForceModifier = 5f;

        private void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Update()
        {
            transform.localPosition = Vector3.zero;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (!Runner.IsSharedModeMasterClient)
            {
                //Debug.LogWarning($"Player {Runner.LocalPlayer} is not the host, ignoring collision.");
                return;
            }
            if (playerMovement.IsRagdolled) return;

            Rigidbody hitBody = collision.attachedRigidbody;
            if (hitBody == null)
            {
                //Debug.Log($"Hit object {collision.gameObject.name} has no rigidbody, ignoring.");
                return;
            }

            DraggableObject draggableObject = hitBody.GetComponent<DraggableObject>();
            if(draggableObject == null)
            {
                //Debug.Log($"Hit object {collision.gameObject.name} is not draggable object");
                return;
            }

            float impactForce = hitBody.velocity.magnitude * hitBody.mass;

            //Debug.Log($"Hit by {collision.gameObject.name} with force: {impactForce}");

            if (impactForce >= impactThreshold)
            {
                TriggerRagdollRpc(hitBody.velocity.magnitude * (capsuleCollider.ClosestPoint(collision.transform.position) - collision.transform.position).normalized);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void TriggerRagdollRpc(Vector3 direction)
        {
            if (playerMovement.IsRagdolled)
            {
                //Debug.LogWarning($"{playerMovement.gameObject.name} Ragdoll already active, ignoring RPC trigger.");
                return;
            }

            StartCoroutine(HandleRagdoll());
            ragdollEnabler.AddForce(impactForceModifier * direction, ForceMode.Impulse);
        }

        private IEnumerator HandleRagdoll()
        {
            playerMovement.RagdollPlayer(true);
            yield return new WaitForSeconds(ragdollDuration);
            playerMovement.RagdollPlayer(false);
        }
    }
}
