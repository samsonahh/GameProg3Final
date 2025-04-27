using Fusion;
using System;
using UnityEngine;

namespace Samson
{
    public struct PlayerDragData : INetworkStruct
    {
        public Vector3 LocalDragPoint;
        public Vector3 TargetPosition;
        public Vector3 AimOrigin;
        public float Force;
        public float Damping;
        public float TimeStamp;

        public override string ToString()
        {
            return $"DragPoint: {LocalDragPoint}, Target: {TargetPosition}, Aim: {AimOrigin}, Force: {Force}, Damping: {Damping}, TimeStamp: {TimeStamp}";
        }
    }

    public class PlayerObjectDragger : NetworkBehaviour
    {
        [Header("Drag Config")]
        [SerializeField] private float dragForce = 10f;
        [SerializeField] private float dragDamping = 5f;
        [SerializeField] private float dragBreakDistance = 12.5f;

        [Header("Drag Adjustment Config")]
        [SerializeField] private float dragMinDistance = 1f;
        [SerializeField] private float dragMaxDistance = 5f;
        [SerializeField] private float zoomSpeed = 0.25f;

        [Networked] public NetworkBool IsDragging { get; set; }
        [Networked] public PlayerDragData CurrentDragData { get; set; }

        private DraggableObject currentDragObject;
        private float dragCurrentDistance;
        private Ray aimRay;

        public Action<DraggableObject, PlayerDragData> OnDragStart = delegate { };
        public Action<DraggableObject, PlayerDragData> OnDragUpdate = delegate { };
        public Action<DraggableObject> OnDragEnd = delegate { };

        [Header("Launch Config")]
        [SerializeField] private float shootForce = 25f;

        private void Update()
        {
            AssignAimRay();

            ReadDragInput();
            ReadUndragInput();
            HandleDragZoom();

            UpdateDrag();

            ReadLaunchInput();
        }

        private void AssignAimRay()
        {
            aimRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        private void ReadDragInput()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

            if (Physics.Raycast(aimRay, out RaycastHit hit, dragMaxDistance, LayerMask.GetMask("Draggable")))
            {
                if (hit.transform.TryGetComponent(out DraggableObject draggableObject))
                {
                    currentDragObject = draggableObject;
                    StartDrag(draggableObject, hit.point);
                }
            }
        }

        private void ReadUndragInput()
        {
            if (!Input.GetKeyUp(KeyCode.Mouse0)) return;
            if (currentDragObject == null) return;

            EndDrag(currentDragObject);
            currentDragObject = null;
        }

        private void StartDrag(DraggableObject draggedObject, Vector3 dragPoint)
        {
            dragCurrentDistance = Vector3.Distance(dragPoint, aimRay.origin);

            Vector3 targetPosition = aimRay.origin + aimRay.direction * dragCurrentDistance;
            CurrentDragData = new PlayerDragData
            {
                LocalDragPoint = draggedObject.transform.InverseTransformPoint(dragPoint),
                TargetPosition = targetPosition,
                AimOrigin = aimRay.origin,
                Force = dragForce,
                Damping = dragDamping,
                TimeStamp = Runner.SimulationTime,
            };

            IsDragging = true;
            draggedObject.StartDraggingRpc(Runner.LocalPlayer);
            OnDragStart.Invoke(draggedObject, CurrentDragData);
        }

        private void UpdateDrag()
        {
            if(!IsDragging) return;
            if (currentDragObject == null) return;

            Vector3 targetPosition = aimRay.origin + aimRay.direction * dragCurrentDistance;
            if (Vector3.Distance(currentDragObject.transform.position, targetPosition) > dragBreakDistance)
            {
                EndDrag(currentDragObject);
                return;
            }

            CurrentDragData = new PlayerDragData
            {
                LocalDragPoint = CurrentDragData.LocalDragPoint,
                TargetPosition = targetPosition,
                AimOrigin = aimRay.origin,
                Force = dragForce,
                Damping = dragDamping,
                TimeStamp = Runner.SimulationTime,
            };

            OnDragUpdate.Invoke(currentDragObject, CurrentDragData);
        }

        private void EndDrag(DraggableObject draggedObject)
        {
            currentDragObject = null;
            IsDragging = false;
            OnDragEnd.Invoke(draggedObject);
        }

        private void HandleDragZoom()
        {
            if (currentDragObject == null) return;

            dragCurrentDistance += Input.mouseScrollDelta.y * zoomSpeed;
            dragCurrentDistance = Mathf.Clamp(dragCurrentDistance, dragMinDistance, dragMaxDistance);
        }

        private void ReadLaunchInput()
        {
            if(!Input.GetKeyDown(KeyCode.E)) return;

            if (currentDragObject == null) return;

            currentDragObject.LaunchRpc(Runner.LocalPlayer, aimRay.direction.normalized, shootForce);
            EndDrag(currentDragObject);
        }
    }
}
