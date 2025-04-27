using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    [RequireComponent(typeof(PlayerObjectDragger))]
    public class PlayerDragVisualizer : NetworkBehaviour
    {
        private PlayerObjectDragger objectDragger;

        [Header("Drag Visual Config")]
        [SerializeField] private GameObject dragSphereVisualizerPrefab;
        private Material lineMaterial;

        private struct DragVisualData
        {
            public GameObject AimVisual;
            public GameObject PointVisual;
            public LineRenderer AimLine;
            public LineRenderer MagnetLine;
        }

        private Dictionary<PlayerRef, DragVisualData> draggers = new();

        private void Awake()
        {
            objectDragger = GetComponent<PlayerObjectDragger>();

            objectDragger.OnDragStart += ObjectDragger_OnDragStart;
            objectDragger.OnDragUpdate += ObjectDragger_OnDragUpdate;
            objectDragger.OnDragEnd += ObjectDragger_OnDragEnd;

            lineMaterial = dragSphereVisualizerPrefab.GetComponent<Renderer>().sharedMaterial;
        }

        private void OnDestroy()
        {
            objectDragger.OnDragStart -= ObjectDragger_OnDragStart;
            objectDragger.OnDragUpdate -= ObjectDragger_OnDragUpdate;
            objectDragger.OnDragEnd -= ObjectDragger_OnDragEnd;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            DestroyDragVisual(runner.LocalPlayer);
        }

        private void ObjectDragger_OnDragStart(DraggableObject draggedObject, PlayerDragData dragData)
        {
            CreateDragVisual(Runner.LocalPlayer, draggedObject.gameObject, dragData.LocalDragPoint, dragData.TargetPosition, dragData.AimOrigin);
        }

        private void ObjectDragger_OnDragUpdate(DraggableObject draggedObject, PlayerDragData dragData)
        {
            UpdateDragVisual(Runner.LocalPlayer, dragData.TargetPosition, dragData.AimOrigin);
        }

        private void ObjectDragger_OnDragEnd(DraggableObject draggedObject)
        {
            DestroyDragVisual(Runner.LocalPlayer);
        }

        private void CreateDragVisual(PlayerRef player, GameObject draggedObject, Vector3 dragPointLocal, Vector3 targetPoint, Vector3 aimOrigin)
        {
            if (draggers.ContainsKey(player)) return;

            GameObject dragAimVisual = Instantiate(dragSphereVisualizerPrefab, targetPoint, Quaternion.identity);
            GameObject dragPointVisual = Instantiate(dragSphereVisualizerPrefab, draggedObject.transform.TransformPoint(dragPointLocal), Quaternion.identity, draggedObject.transform);
            LineRenderer aimLine = CreateLine(aimOrigin, targetPoint, Color.red);
            if (player == Runner.LocalPlayer) aimLine.gameObject.layer = LayerMask.NameToLayer("HideFromLocal");
            LineRenderer magnetLine = CreateLine(dragPointVisual.transform.position, targetPoint, Color.red);

            draggers.Add(player, new DragVisualData
            {
                AimVisual = dragAimVisual,
                PointVisual = dragPointVisual,
                AimLine = aimLine,
                MagnetLine = magnetLine,
            });
        }

        private void UpdateDragVisual(PlayerRef player, Vector3 targetPoint, Vector3 aimOrigin)
        {
            if (!draggers.TryGetValue(player, out DragVisualData dragVisualData)) return;

            // Target sphere
            dragVisualData.AimVisual.transform.position = targetPoint;

            // Drag point sphere is auto updated by the object transform

            // Aim ray
            dragVisualData.AimLine.SetPosition(0, aimOrigin);
            dragVisualData.AimLine.SetPosition(1, targetPoint);

            // Object to target
            dragVisualData.MagnetLine.SetPosition(0, dragVisualData.PointVisual.transform.position);
            dragVisualData.MagnetLine.SetPosition(1, targetPoint);
        }

        private void DestroyDragVisual(PlayerRef player)
        {
            if (!draggers.TryGetValue(player, out DragVisualData dragVisualData)) return;

            Destroy(dragVisualData.AimVisual);
            Destroy(dragVisualData.PointVisual);
            Destroy(dragVisualData.AimLine.gameObject);
            Destroy(dragVisualData.MagnetLine.gameObject);

            draggers.Remove(player);
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
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            return lineRenderer;
        }
    }
}
