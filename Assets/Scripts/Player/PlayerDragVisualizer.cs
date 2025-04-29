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

        [Header("References")]
        [SerializeField] private Transform playerDragTargetTransform;

        [Header("Drag Visual Config")]
        [SerializeField] private GameObject dragSphereVisualizerPrefab;
        [SerializeField] private float nonLocalLerpSpeed = 15f;
        private Material lineMaterial;

        private struct DragVisualData
        {
            public GameObject AimVisual;
            public GameObject PointVisual;
            public LineRenderer MagnetLine;
        }

        private Dictionary<PlayerRef, DragVisualData> draggers = new();

        private void Awake()
        {
            objectDragger = GetComponent<PlayerObjectDragger>();

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
            if(!draggers.TryGetValue(Object.InputAuthority, out DragVisualData dragVisualData)) return;
            
            if (Runner.LocalPlayer != Object.InputAuthority)
            {
                dragVisualData.AimVisual.transform.position = Vector3.Lerp(dragVisualData.AimVisual.transform.position, dragData.TargetPosition, nonLocalLerpSpeed * Time.deltaTime);

                dragVisualData.MagnetLine.SetPosition(0, dragVisualData.PointVisual.transform.position);
                dragVisualData.MagnetLine.SetPosition(1, dragVisualData.AimVisual.transform.position);
            }
            else
            {
                dragVisualData.AimVisual.transform.position = dragData.TargetPosition;

                dragVisualData.MagnetLine.SetPosition(0, dragVisualData.PointVisual.transform.position);
                dragVisualData.MagnetLine.SetPosition(1, dragData.TargetPosition);
            }

            draggers[Runner.LocalPlayer] = dragVisualData;

            // For rigging
            playerDragTargetTransform.position = dragData.TargetPosition;
        }

        private void ObjectDragger_OnDragStart(DraggableObject draggedObject, PlayerDragData dragData)
        {
            CreateDragVisual(Runner.LocalPlayer, draggedObject.GetComponent<NetworkObject>(), dragData.LocalDragPoint, dragData.TargetPosition, dragData.AimOrigin);
            Debug.Log($"{Runner.LocalPlayer}'s {gameObject.name} created visual for local player {Runner.LocalPlayer}");
            CreateDragVisualRpc(Runner.LocalPlayer, draggedObject.GetComponent<NetworkObject>(), dragData.LocalDragPoint, dragData.TargetPosition, dragData.AimOrigin);
            Debug.Log($"{Runner.LocalPlayer}'s {gameObject.name} sending RPC to create visuals for {Runner.LocalPlayer}");
        }

        private void ObjectDragger_OnDragEnd(DraggableObject draggedObject)
        {
            DestroyDragVisual(Runner.LocalPlayer);
            Debug.Log($"Destroyed visual for local player {Runner.LocalPlayer}");
            DestroyDragVisualRpc(Runner.LocalPlayer);
            Debug.Log($"Sending RPC to destroy visuals for {Runner.LocalPlayer}");
        }

        [Rpc(RpcSources.All, RpcTargets.Proxies)]
        private void CreateDragVisualRpc(PlayerRef player, NetworkObject draggedObject, Vector3 dragPointLocal, Vector3 targetPoint, Vector3 aimOrigin)
        {
            Debug.Log($"Receieved RPC: {Runner.LocalPlayer}'s {gameObject.name} creating drag visual for player {player}");

            CreateDragVisual(player, draggedObject, dragPointLocal, targetPoint, aimOrigin);
        }

        private void CreateDragVisual(PlayerRef player, NetworkObject draggedObject, Vector3 dragPointLocal, Vector3 targetPoint, Vector3 aimOrigin)
        {
            if (draggers.ContainsKey(player)) return;

            GameObject dragAimVisual = Instantiate(dragSphereVisualizerPrefab, targetPoint, Quaternion.identity);
            GameObject dragPointVisual = Instantiate(dragSphereVisualizerPrefab, draggedObject.transform.TransformPoint(dragPointLocal), Quaternion.identity, draggedObject.transform);
            LineRenderer magnetLine = CreateLine(dragPointVisual.transform.position, targetPoint, Color.red);

            draggers.Add(player, new DragVisualData
            {
                AimVisual = dragAimVisual,
                PointVisual = dragPointVisual,
                MagnetLine = magnetLine,
            });
        }

        [Rpc(RpcSources.All, RpcTargets.Proxies)]
        private void DestroyDragVisualRpc(PlayerRef player)
        {
            Debug.Log($"Receieved RPC: {Runner.LocalPlayer}'s {gameObject.name} destroying drag visual for player {player}");

            DestroyDragVisual(player);
        }

        private void DestroyDragVisual(PlayerRef player)
        {
            if (!draggers.TryGetValue(player, out DragVisualData dragVisualData)) return;

            if(dragVisualData.AimVisual != null) Destroy(dragVisualData.AimVisual);
            if (dragVisualData.PointVisual != null) Destroy(dragVisualData.PointVisual);
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
