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

        public class DragData
        {
            public Vector3 TargetPosition { get; private set; }
            public Transform DragTransform { get; private set; }
            public float Force { get; private set; }
            public float Damping { get; private set; }
            public float Timestamp { get; private set; }
            public DragData(Vector3 targetPosition, Transform dragTransform, float force, float damping, float timestamp)
            {
                TargetPosition = targetPosition;
                DragTransform = dragTransform;
                Force = force;
                Damping = damping;
                Timestamp = timestamp;
            }
        }

        private Dictionary<PlayerRef, DragData> dragForces = new();

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();

            originalAngularDrag = rigidBody.angularDrag;
        }

        public override void FixedUpdateNetwork()
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
                float timeStamp = dragForce.Value.Timestamp;

                if(Runner.SimulationTime - timeStamp > 10f)
                {
                    dragForces.Remove(player);
                    continue;
                }

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

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void AddDragSourceRpc(PlayerRef player, Vector3 targetPosition, Vector3 dragStartPosition, float dragForce, float dragDamping, float angularDrag, float timeStamp)
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

                dragForces[player] = new DragData(targetPosition, dragTransform, dragForce, dragDamping, timeStamp);
            }
            else
            {
                dragForces.Add(player, new DragData(targetPosition, dragTransform, dragForce, dragDamping, timeStamp));
            }
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void UpdateDragSourceRpc(PlayerRef player, Vector3 targetPosition, float timeStamp)
        {
            if (!dragForces.ContainsKey(player))
            {
                return;
            }

            dragForces[player] = new DragData(targetPosition, dragForces[player].DragTransform, dragForces[player].Force, dragForces[player].Damping, timeStamp);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
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

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void LaunchRpc(PlayerRef player, Vector3 direction, float force)
        {
            if (!dragForces.ContainsKey(player)) return;

            rigidBody.AddForce(direction * force, ForceMode.Impulse);
        }

        private void ResetAngularDrag()
        {
            rigidBody.angularDrag = originalAngularDrag;
        }

        public Transform GetDragTransform(PlayerRef player)
        {
            if (!dragForces.ContainsKey(player)) return null;

            return dragForces[player].DragTransform;
        }
    }
}
