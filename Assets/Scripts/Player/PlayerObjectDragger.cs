using Fusion;
using UnityEngine;

namespace Samson
{
    public struct PlayerDragData : INetworkStruct
    {
        public Vector3 LocalDragPoint;
        public Vector3 TargetPosition;
        public float Force;
        public float Damping;
        public float TimeStamp;
    }

    public class PlayerObjectDragger : NetworkBehaviour
    {
        [Header("Drag Config")]
        [SerializeField] private float dragForce = 10f;
        [SerializeField] private float dragDamping = 5f;
        [SerializeField] private float dragBreakDistance = 12.5f;
        [SerializeField] private float draggedObjectAngularDrag = 2f;

        [Header("Drag Adjustment Config")]
        [SerializeField] private float dragMinDistance = 1f;
        [SerializeField] private float dragMaxDistance = 5f;
        [SerializeField] private float zoomSpeed = 0.25f;
        private float dragCurrentDistance;

        private DraggableObject currentDragObject;
        private PlayerDragData currentDragData;
        private Ray aimRay;

        [Header("Drag Visual")]
        [SerializeField] private GameObject dragSphereVisualizerPrefab;
        private GameObject dragAimVisualInstance;
        private GameObject dragPointVisualInstance;
        private LineRenderer aimLineRenderer;
        private LineRenderer magnetLineRenderer;

        [Header("Launch Config")]
        [SerializeField] private float shootForce = 25f;

        private void Update()
        {
            AssignAimRay();

            ReadDragInput();
            ReadUndragInput();
            HandleDragZoom();

            TryDragUpdate();

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
                    OnDragStart(draggableObject, hit.point);
                }
            }
        }

        private void ReadUndragInput()
        {
            if (!Input.GetKeyUp(KeyCode.Mouse0)) return;
            if (currentDragObject == null) return;

            OnDragEnd(currentDragObject);
            currentDragObject = null;
        }

        private void OnDragStart(DraggableObject draggedObject, Vector3 dragPoint)
        {
            dragCurrentDistance = Vector3.Distance(dragPoint, aimRay.origin);

            Vector3 targetPosition = aimRay.origin + aimRay.direction * dragCurrentDistance;
            currentDragData = new PlayerDragData
            {
                LocalDragPoint = draggedObject.transform.InverseTransformPoint(dragPoint),
                TargetPosition = targetPosition,
                Force = dragForce,
                Damping = dragDamping,
                TimeStamp = Runner.SimulationTime,
            };

            draggedObject.StartDraggingRpc(Runner.LocalPlayer, currentDragData);

            // Start visual
            UpdateDragVisual(draggedObject.transform.position, targetPosition);
        }

        private void TryDragUpdate()
        {
            if (currentDragObject == null) return;

            Vector3 targetPosition = aimRay.origin + aimRay.direction * dragCurrentDistance;
            if (Vector3.Distance(currentDragObject.transform.position, targetPosition) > dragBreakDistance)
            {
                OnDragEnd(currentDragObject);
                return;
            }

            currentDragData = new PlayerDragData
            {
                LocalDragPoint = currentDragData.LocalDragPoint,
                TargetPosition = targetPosition,
                Force = dragForce,
                Damping = dragDamping,
                TimeStamp = Runner.SimulationTime,
            };

            currentDragObject.UpdateDraggingRpc(Runner.LocalPlayer, currentDragData);

            //Update visual
            UpdateDragVisual(currentDragData.LocalDragPoint, targetPosition);
        }

        private void OnDragEnd(DraggableObject draggedObject)
        {
            if(draggedObject != null) draggedObject.StopDraggingRpc(Runner.LocalPlayer);
            currentDragObject = null;

            // Destroy visual
            DestroyDragVisual();
        }

        private void HandleDragZoom()
        {
            if (currentDragObject == null) return;

            dragCurrentDistance += Input.mouseScrollDelta.y * zoomSpeed;
            dragCurrentDistance = Mathf.Clamp(dragCurrentDistance, dragMinDistance, dragMaxDistance);
        }

        private void UpdateDragVisual(Vector3 dragPoint, Vector3 targetPoint)
        {
            float lerpSpeed = 15f;

            // Target sphere
            if(dragAimVisualInstance == null)
            {
                dragAimVisualInstance = Instantiate(dragSphereVisualizerPrefab, targetPoint, Quaternion.identity);
            }
            else
            {
                dragAimVisualInstance.transform.position = targetPoint;
            }

            // Drag point sphere
            Vector3 worldDragPoint = currentDragObject.transform.TransformPoint(currentDragData.LocalDragPoint);
            if(dragPointVisualInstance == null)
            {
                dragPointVisualInstance = Instantiate(dragSphereVisualizerPrefab, worldDragPoint, Quaternion.identity);
            }
            else
            {
                dragPointVisualInstance.transform.position = 
                    Vector3.Lerp(dragPointVisualInstance.transform.position, worldDragPoint, lerpSpeed * Time.deltaTime);
            }

            // Aim ray
            if(aimLineRenderer == null)
            {
                aimLineRenderer = CreateLine(aimRay.origin, targetPoint, Color.red);
                aimLineRenderer.gameObject.layer = LayerMask.NameToLayer("HideFromLocal");
            }
            else
            {
                aimLineRenderer.SetPosition(0, aimRay.origin);
                aimLineRenderer.SetPosition(1, targetPoint);
            }

            // Object to target
            if(magnetLineRenderer == null)
            {
                magnetLineRenderer = CreateLine(worldDragPoint, targetPoint, Color.red);
            }
            else
            {
                magnetLineRenderer.SetPosition(0, Vector3.Lerp(dragPointVisualInstance.transform.position, worldDragPoint, lerpSpeed * Time.deltaTime));
                magnetLineRenderer.SetPosition(1, targetPoint);
            }
        }

        private void DestroyDragVisual()
        {
            if (dragAimVisualInstance != null)
            {
                Destroy(dragAimVisualInstance);
                dragAimVisualInstance = null;
            }

            if (dragPointVisualInstance != null)
            {
                Destroy(dragPointVisualInstance);
                dragPointVisualInstance = null;
            }

            if (aimLineRenderer != null)
            {
                Destroy(aimLineRenderer.gameObject);
                aimLineRenderer = null;
            }

            if(magnetLineRenderer != null)
            {
                Destroy(magnetLineRenderer.gameObject);
                magnetLineRenderer = null;
            }
        }

        private void ReadLaunchInput()
        {
            if(!Input.GetKeyDown(KeyCode.E)) return;

            if (currentDragObject == null) return;

            currentDragObject.LaunchRpc(Runner.LocalPlayer, aimRay.direction.normalized, shootForce);
            OnDragEnd(currentDragObject);
        }

        public LineRenderer CreateLine(Vector3 start, Vector3 end, Color color)
        {
            GameObject lineObj = new GameObject("Line");
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            // Simple unlit material
            lineRenderer.material = new Material(dragSphereVisualizerPrefab.GetComponent<Renderer>().sharedMaterial);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            return lineRenderer;
        }
    }
}
