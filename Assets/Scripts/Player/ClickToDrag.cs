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

        [Header("Shoot")]
        [SerializeField] private float shootForce = 25f;
        private bool shootPressed;

        [Header("Drag Visual")]
        [SerializeField] private GameObject dragVisualPrefab;
        private GameObject dragVisualInstance;

        private void Update()
        {
            if (!HasStateAuthority) return;

            HandleDragInput();
            HandleDragZoom();
            HandleShootInput();
            VisualizeDrag();
        }

        private void HandleShootInput()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                shootPressed = true;
            }
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

        public override void FixedUpdateNetwork()
        {
            HandleDrag();

            HandleShoot();

            shootPressed = false;
        }

        private void HandleDrag()
        {
            if (currentDragObject == null) return;

            Vector3 dragTargetPoint = ray.origin + ray.direction * dragCurrentDistance;

            currentDragObject.UpdateDragSourceRpc(Runner.LocalPlayer, dragTargetPoint);
        }

        private void VisualizeDrag()
        {
            if(currentDragObject == null)
            {
                RemoveDragVisualInstance();
                return;
            }

            Transform dragTransform = currentDragObject.GetDragTransform(Runner.LocalPlayer);
            if(dragTransform == null)
            {
                RemoveDragVisualInstance();
                return;
            }

            if (dragVisualInstance == null)
            {
                dragVisualInstance = Instantiate(dragVisualPrefab, dragTransform);
            }
        }

        private void RemoveDragVisualInstance()
        {
            if (dragVisualInstance != null)
            {
                Destroy(dragVisualInstance);
                dragVisualInstance = null;
            }
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

        private void HandleShoot()
        {
            if (!shootPressed) return;
            if(currentDragObject == null) return;

            currentDragObject.LaunchRpc(Runner.LocalPlayer, ray.direction.normalized, shootForce);
            currentDragObject.RemoveDragSourceRpc(Runner.LocalPlayer);
        }
    }
}
