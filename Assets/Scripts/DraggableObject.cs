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

        private float originalAngularDrag;

        private Dictionary<PlayerRef, (Vector3, Transform, float, float)> dragForces { get; set; } = new();

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();

            originalAngularDrag = rigidBody.angularDrag;
        }

        private void FixedUpdate()
        {
            ApplyNetworkedDragForces();
        }

        private void ApplyNetworkedDragForces()
        {
            if (!HasStateAuthority) return;

            foreach(var dragForce in new Dictionary<PlayerRef, (Vector3, Transform, float, float)>(dragForces))
            {
                PlayerRef player = dragForce.Key;
                Vector3 targetPosition = dragForce.Value.Item1;
                Transform dragTransform = dragForce.Value.Item2;
                float force = dragForce.Value.Item3;
                float damp = dragForce.Value.Item4;

                if (dragTransform == null)
                {
                    dragForces.Remove(player);
                    continue;
                }

                Vector3 dragDirection = targetPosition - dragTransform.position;

                rigidBody.AddForceAtPosition(dragDirection * force, dragTransform.position, ForceMode.Acceleration);

                Vector3 pointVelocity = rigidBody.GetPointVelocity(dragTransform.position);
                rigidBody.AddForceAtPosition(-pointVelocity * damp, dragTransform.position, ForceMode.Acceleration);

            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void AddDragSourceRpc(PlayerRef player, Vector3 targetPosition, Vector3 dragStartPosition, float dragForce, float dragDamping, float angularDrag)
        {
            rigidBody.useGravity = false;
            rigidBody.angularDrag = angularDrag;

            Transform dragTransform = new GameObject("DragTransform").transform;
            dragTransform.position = dragStartPosition;
            dragTransform.SetParent(transform);

            if (dragForces.ContainsKey(player))
            {
                Transform existingTransform = dragForces[player].Item2;
                if(existingTransform != null)
                {
                    Destroy(existingTransform.gameObject);
                }

                dragForces[player] = (targetPosition, dragTransform, dragForce, dragDamping);
            }
            else
            {
                dragForces.Add(player, (targetPosition, dragTransform, dragForce, dragDamping));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void UpdateDragSourceRpc(PlayerRef player, Vector3 targetPosition)
        {
            if (!dragForces.ContainsKey(player))
            {
                return;
            }

            dragForces[player] = (targetPosition, dragForces[player].Item2, dragForces[player].Item3, dragForces[player].Item4);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RemoveDragSourceRpc(PlayerRef player)
        {
            if(dragForces.ContainsKey(player))
            {
                Transform existingTransform = dragForces[player].Item2;
                if(existingTransform != null)
                {
                    Destroy(existingTransform.gameObject);
                }
                dragForces.Remove(player);
            }

            if(dragForces.Count == 0)
            {
                rigidBody.useGravity = true;
                ResetAngularDrag();
            }
        }

        private void ResetAngularDrag()
        {
            rigidBody.angularDrag = originalAngularDrag;
        }
    }
}
