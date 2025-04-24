using Fusion;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Samson
{
    public class ClickToDrag : NetworkBehaviour
    {
        [SerializeField] private float dragForce = 10f;
        [SerializeField] private float dragDamping = 5f;
        [SerializeField] private float dragBreakDistance = 5f;
        [SerializeField] private float draggedObjectAngularDrag = 2f;
        [SerializeField] private float dragMinDistance = 1f;
        [SerializeField] private float dragMaxDistance = 5f;
        [SerializeField] private float zoomSpeed = 10f;
        private float dragCurrentDistance;

        private Ray ray;
        private DraggableObject currentDragObject;

        private void Update()
        {
            if (!HasStateAuthority) return;

            HandleDragInput();
            HandleDragZoom();
        }

        private void HandleDragInput()
        {
            if (!Input.GetKey(KeyCode.Mouse0))
            {
                BreakDrag();
                return;
            }

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(currentDragObject != null) return;

            if (Physics.Raycast(ray, out RaycastHit hit, dragMaxDistance, LayerMask.GetMask("Draggable")))
            {
                if (hit.rigidbody == null)
                {
                    BreakDrag();
                    return;
                }

                StartDrag(hit);
            }
        }

        private void FixedUpdate()
        {
            if (currentDragObject == null) return;

            Vector3 dragTargetPoint = ray.origin + ray.direction * dragCurrentDistance;

            currentDragObject.UpdateDragSourceRpc(Runner.LocalPlayer, dragTargetPoint);
        }

        private void HandleDragZoom()
        {
            if (dragCurrentDistance == -1) return;
            dragCurrentDistance += Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
            dragCurrentDistance = Mathf.Clamp(dragCurrentDistance, dragMinDistance, dragMaxDistance);
        }

        private void StartDrag(RaycastHit hit) 
        {
            dragCurrentDistance = Vector3.Distance(hit.point, ray.origin);
            Vector3 dragTargetPoint = ray.origin + ray.direction * dragCurrentDistance;

            currentDragObject = hit.rigidbody.GetComponent<DraggableObject>();
            currentDragObject.AddDragSourceRpc(Runner.LocalPlayer, dragTargetPoint, hit.point, dragForce, dragDamping, draggedObjectAngularDrag);
        }

        private void BreakDrag()
        {
            if (currentDragObject != null)
            {
                currentDragObject.RemoveDragSourceRpc(Runner.LocalPlayer);
            }
            currentDragObject = null;
        }
    }
}
