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
        private NetworkObject networkObject;

        [Header("Drag Visual Config")]
        [SerializeField] private GameObject dragSphereVisualizerPrefab;
        [SerializeField] private float nonLocalLerpSpeed = 15f;
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
            networkObject = GetComponent<NetworkObject>();

            objectDragger.OnDragStart += ObjectDragger_OnDragStart;
            objectDragger.OnDragEnd += ObjectDragger_OnDragEnd;

            lineMaterial = dragSphereVisualizerPrefab.GetComponent<Renderer>().sharedMaterial;
        }

        private void OnDestroy()
        {
            objectDragger.OnDragStart -= ObjectDragger_OnDragStart;
            objectDragger.OnDragEnd -= ObjectDragger_OnDragEnd;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            DestroyDragVisual(runner.LocalPlayer);
            DestroyDragVisualRpc(runner.LocalPlayer);
        }

        private void Update()
        {
            if(!objectDragger.IsDragging) return;

            PlayerDragData dragData = objectDragger.CurrentDragData;
            if(!draggers.TryGetValue(networkObject.InputAuthority, out DragVisualData dragVisualData)) return;
            
            if (Runner.LocalPlayer != networkObject.InputAuthority)
            {
                dragVisualData.AimVisual.transform.position = Vector3.Lerp(dragVisualData.AimVisual.transform.position, dragData.TargetPosition, nonLocalLerpSpeed * Time.deltaTime);

                dragVisualData.AimLine.SetPosition(0, Vector3.Lerp(dragVisualData.AimLine.GetPosition(0), dragData.AimOrigin, nonLocalLerpSpeed * Time.deltaTime));
                dragVisualData.AimLine.SetPosition(1, dragVisualData.AimVisual.transform.position);

                dragVisualData.MagnetLine.SetPosition(0, dragVisualData.PointVisual.transform.position);
                dragVisualData.MagnetLine.SetPosition(1, dragVisualData.AimVisual.transform.position);
            }
            else
            {
                dragVisualData.AimVisual.transform.position = dragData.TargetPosition;

                dragVisualData.AimLine.SetPosition(0, dragData.AimOrigin);
                dragVisualData.AimLine.SetPosition(1, dragData.TargetPosition);

                dragVisualData.MagnetLine.SetPosition(0, dragVisualData.PointVisual.transform.position);
                dragVisualData.MagnetLine.SetPosition(1, dragData.TargetPosition);
            }

            draggers[Runner.LocalPlayer] = dragVisualData;
        }

        private void ObjectDragger_OnDragStart(DraggableObject draggedObject, PlayerDragData dragData)
        {
            CreateDragVisual(Runner.LocalPlayer, draggedObject.GetComponent<NetworkObject>(), dragData.LocalDragPoint, dragData.TargetPosition, dragData.AimOrigin);
            CreateDragVisualRpc(Runner.LocalPlayer, draggedObject.GetComponent<NetworkObject>(), dragData.LocalDragPoint, dragData.TargetPosition, dragData.AimOrigin);
        }

        private void ObjectDragger_OnDragEnd(DraggableObject draggedObject)
        {
            DestroyDragVisual(Runner.LocalPlayer);
            DestroyDragVisualRpc(Runner.LocalPlayer);
        }

        [Rpc(RpcSources.All, RpcTargets.Proxies)]
        private void CreateDragVisualRpc(PlayerRef player, NetworkObject draggedObject, Vector3 dragPointLocal, Vector3 targetPoint, Vector3 aimOrigin)
        {
            CreateDragVisual(player, draggedObject, dragPointLocal, targetPoint, aimOrigin);
        }

        private void CreateDragVisual(PlayerRef player, NetworkObject draggedObject, Vector3 dragPointLocal, Vector3 targetPoint, Vector3 aimOrigin)
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

        [Rpc(RpcSources.All, RpcTargets.Proxies)]
        private void DestroyDragVisualRpc(PlayerRef player)
        {
            DestroyDragVisual(player);
        }

        private void DestroyDragVisual(PlayerRef player)
        {
            if (!draggers.TryGetValue(player, out DragVisualData dragVisualData)) return;

            if(dragVisualData.AimVisual != null) Destroy(dragVisualData.AimVisual);
            if (dragVisualData.PointVisual != null) Destroy(dragVisualData.PointVisual);
            if (dragVisualData.AimLine != null) Destroy(dragVisualData.AimLine.gameObject);
            if (dragVisualData.MagnetLine != null) Destroy(dragVisualData.MagnetLine.gameObject);

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
