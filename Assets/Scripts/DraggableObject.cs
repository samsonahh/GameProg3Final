using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    [RequireComponent(typeof(Rigidbody))]
    public class DraggableObject : NetworkBehaviour
    {
        private Rigidbody rigidBody;

        [Networked, Capacity(20)] // Up to 20 draggers
        public NetworkDictionary<PlayerRef, PlayerObjectDragger> Draggers => default;

        [SerializeField] private float dragMaxForce = 25f;
        [SerializeField] private float dragIdleExpireTime = 5f;

        public override void Spawned()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;
            if (Draggers.Count <= 0)
            {
                rigidBody.useGravity = true;
                return;
            }

            foreach(var keyValue in new Dictionary<PlayerRef, PlayerObjectDragger>(Draggers))
            {
                PlayerRef draggerPlayerRef = keyValue.Key;
                PlayerObjectDragger playerObjectDragger = keyValue.Value;

                Debug.Log($"DraggableObject processing dragger {draggerPlayerRef}");
                if(playerObjectDragger == null || !playerObjectDragger.IsDragging)
                {
                    Debug.LogWarning($"Dragger player {draggerPlayerRef} doesn't have object dragger or is not dragging.");
                    Draggers.Remove(draggerPlayerRef);
                    continue;
                }

                PlayerDragData dragData = playerObjectDragger.CurrentDragData;

                // Check if the dragger is still valid
                if(Runner.SimulationTime - dragData.TimeStamp > dragIdleExpireTime)
                {
                    Debug.LogWarning($"Dragger {draggerPlayerRef} player timed out.");
                    Draggers.Remove(draggerPlayerRef);
                    continue;
                }

                Debug.Log($"Applying player {draggerPlayerRef}'s drag with data: {dragData}");
                Vector3 worldDragPoint = transform.TransformPoint(dragData.LocalDragPoint);
                Vector3 forceDirection = dragData.TargetPosition - rigidBody.position;

                Vector3 springForce = forceDirection * dragData.Force;
                Vector3 dampForce = -rigidBody.GetPointVelocity(worldDragPoint) * dragData.Damping;

                Vector3 finalForce = springForce + dampForce;

                if(finalForce.magnitude > dragMaxForce) finalForce = finalForce.normalized * dragMaxForce;
                 
                rigidBody.AddForceAtPosition(finalForce, worldDragPoint, ForceMode.Force);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void StartDraggingRpc(PlayerRef player)
        {
            if (Draggers.ContainsKey(player)) return;

            Debug.Log($"Attempting to add player {player} to draggers list");

            NetworkObject playerObject = Runner.GetPlayerObject(player);
            if (playerObject == null)
            {
                Debug.LogWarning($"Dragger player {player} doesn't exist.");
                return;
            }

            PlayerObjectDragger playerObjectDragger = playerObject.GetComponent<PlayerObjectDragger>();
            if (playerObjectDragger == null)
            {
                Debug.LogWarning($"Dragger player {player} doesn't have object dragger.");
                return;
            }

            Draggers.Add(player, playerObjectDragger);
            rigidBody.useGravity = false;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void LaunchRpc(PlayerRef player, Vector3 direction, float launchForce)
        {
            if(Draggers.ContainsKey(player)) Draggers.Remove(player); // Remove the player from the draggers list
            rigidBody.AddForce(direction.normalized * launchForce, ForceMode.Impulse);
        }
    }
}
