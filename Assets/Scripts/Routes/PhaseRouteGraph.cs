using System;
using Dreamscape.PowerUps;
using UnityEngine;

namespace Dreamscape.Routes
{
    [DisallowMultipleComponent]
    public sealed class PhaseRouteGraph : MonoBehaviour
    {
        [Serializable]
        public sealed class Point
        {
            public string id;
            public Transform marker;
            public Collider floor;
            public bool safe = true;
            public DoubleJumpPickup pickup;

            public bool Available
            {
                get
                {
                    if (!safe || marker == null || !marker.gameObject.activeInHierarchy ||
                        floor == null || !floor.enabled || !floor.gameObject.activeInHierarchy)
                        return false;
                    Bounds bounds = floor.bounds;
                    var projected = new Vector3(marker.position.x, bounds.center.y, marker.position.z);
                    return bounds.Contains(projected);
                }
            }
        }

        [SerializeField] Point[] points;
        [SerializeField] PowerRouteSearch.Connection[] connections;
        [SerializeField] int start;
        [SerializeField] int goal;

        public PowerRouteSearch.Result LastResult { get; private set; }

        void Start() => ValidateAndLog();

        public PowerRouteSearch.Result Evaluate(bool blockPowerConnections = false)
        {
            if (points == null || connections == null)
                return new PowerRouteSearch.Result(Array.Empty<int>(), 0, "Configure os pontos e conexões da fase.");
            var nodes = new PowerRouteSearch.Node[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Point point = points[i];
                if (point == null || point.marker == null || point.floor == null)
                    return new PowerRouteSearch.Result(Array.Empty<int>(), 0, $"O ponto {i} está sem marcador ou piso.");
                bool grantsPower = point.pickup != null && point.pickup.isActiveAndEnabled;
                nodes[i] = new PowerRouteSearch.Node(point.id, point.Available, grantsPower);
            }
            var edges = (PowerRouteSearch.Connection[])connections.Clone();
            if (blockPowerConnections)
                for (int i = 0; i < edges.Length; i++)
                    edges[i].blocked |= edges[i].requiresDoubleJump;
            return PowerRouteSearch.FindPath(nodes, edges, start, goal);
        }

        [ContextMenu("Validar rota segura")]
        public void ValidateAndLog()
        {
            LastResult = Evaluate();
            LogResult(LastResult, "DS-14");
        }

        [ContextMenu("Demonstrar rota interrompida (bloquear salto com poder)")]
        public void DemonstrateBlockedRoute() => LogResult(Evaluate(true), "DS-14 demonstração");

        void LogResult(PowerRouteSearch.Result result, string label)
        {
            if (!result.Found)
            {
                Debug.LogWarning($"{label}: sem rota segura. {result.Error} Estados visitados: {result.VisitedStates}.", this);
                return;
            }
            var path = new string[result.Path.Length];
            for (int i = 0; i < path.Length; i++)
                path[i] = points[result.Path[i]].id;
            Debug.Log($"{label}: rota segura: {string.Join(" -> ", path)}. Estados visitados: {result.VisitedStates}.", this);
        }

        void OnDrawGizmosSelected()
        {
            if (points == null || connections == null)
                return;
            foreach (Point point in points)
            {
                if (point?.marker == null)
                    continue;
                Gizmos.color = point.Available ? Color.green : Color.red;
                Gizmos.DrawSphere(point.marker.position + Vector3.up * 0.2f, 0.15f);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(point.marker.position + Vector3.up * 0.4f, point.id);
#endif
            }
            foreach (PowerRouteSearch.Connection edge in connections)
            {
                if (edge.from < 0 || edge.from >= points.Length || edge.to < 0 || edge.to >= points.Length ||
                    points[edge.from]?.marker == null || points[edge.to]?.marker == null)
                    continue;
                Gizmos.color = edge.blocked || !points[edge.from].Available || !points[edge.to].Available ? Color.red :
                    edge.requiresDoubleJump ? Color.yellow : Color.cyan;
                Vector3 from = points[edge.from].marker.position + Vector3.up * 0.2f;
                Vector3 to = points[edge.to].marker.position + Vector3.up * 0.2f;
                Gizmos.DrawLine(from, to);
                Vector3 direction = (to - from).normalized;
                Vector3 side = Vector3.Cross(direction, Vector3.up) * 0.15f;
                Gizmos.DrawLine(to, to - direction * 0.4f + side);
                Gizmos.DrawLine(to, to - direction * 0.4f - side);
            }
        }
    }
}
