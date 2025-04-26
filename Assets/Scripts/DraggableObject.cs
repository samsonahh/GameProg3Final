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

        private Dictionary<PlayerRef, PlayerDragData> draggers = new();

        [SerializeField] private float dragMaxForce = 25f;
        [SerializeField] private float dragIdleExpireTime = 5f;

        public override void Spawned()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;

            if (draggers.Count <= 0) return;

            foreach(var dragger in new Dictionary<PlayerRef, PlayerDragData>(draggers))
            {
                PlayerDragData dragData = dragger.Value;

                // Check if the dragger is still valid
                if(Runner.SimulationTime - dragData.TimeStamp > dragIdleExpireTime)
                {
                    draggers.Remove(dragger.Key);
                    continue;
                }
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
        public void StartDraggingRpc(PlayerRef player, PlayerDragData dragData)
        {
            if (draggers.ContainsKey(player)) return;

            draggers.Add(player, dragData);
            rigidBody.useGravity = false;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void UpdateDraggingRpc(PlayerRef player, PlayerDragData dragData)
        {
            if(!draggers.ContainsKey(player)) return;

            draggers[player] = dragData;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void StopDraggingRpc(PlayerRef player)
        {
            draggers.Remove(player);

            if (draggers.Count == 0)
            {
                rigidBody.useGravity = true;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void LaunchRpc(PlayerRef player, Vector3 direction, float launchForce)
        {
            StopDraggingRpc(player);
            rigidBody.AddForce(direction.normalized * launchForce, ForceMode.Impulse);
        }
    }
}
