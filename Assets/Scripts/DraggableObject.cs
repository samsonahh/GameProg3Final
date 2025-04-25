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

        private class DragData
        {
            public Vector3 TargetPosition { get; set; }
            public Transform DragTransform { get; set; }
            public float Force { get; set; }
            public float Damping { get; set; }
            public DragData(Vector3 targetPosition, Transform dragTransform, float force, float damping)
            {
                TargetPosition = targetPosition;
                DragTransform = dragTransform;
                Force = force;
                Damping = damping;
            }
        }

        private Dictionary<PlayerRef, DragData> dragForces { get; set; } = new();

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

            foreach(var dragForce in new Dictionary<PlayerRef, DragData>(dragForces))
            {
                PlayerRef player = dragForce.Key;
                Vector3 targetPosition = dragForce.Value.TargetPosition;
                Transform dragTransform = dragForce.Value.DragTransform;
                float force = dragForce.Value.Force;
                float damp = dragForce.Value.Damping;

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
                Transform existingTransform = dragForces[player].DragTransform;
                if(existingTransform != null)
                {
                    Destroy(existingTransform.gameObject);
                }

                dragForces[player] = new DragData(targetPosition, dragTransform, dragForce, dragDamping);
            }
            else
            {
                dragForces.Add(player, new DragData(targetPosition, dragTransform, dragForce, dragDamping));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void UpdateDragSourceRpc(PlayerRef player, Vector3 targetPosition)
        {
            if (!dragForces.ContainsKey(player))
            {
                return;
            }

            dragForces[player] = new DragData(targetPosition, dragForces[player].DragTransform, dragForces[player].Force, dragForces[player].Damping);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RemoveDragSourceRpc(PlayerRef player)
        {
            if(dragForces.ContainsKey(player))
            {
                Transform existingTransform = dragForces[player].DragTransform;
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
