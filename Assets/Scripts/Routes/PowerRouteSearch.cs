using System;
using System.Collections.Generic;

namespace Dreamscape.Routes
{
    public static class PowerRouteSearch
    {
        public readonly struct Node
        {
            public string Id { get; }
            public bool Available { get; }
            public bool GrantsDoubleJump { get; }

            public Node(string id, bool available = true, bool grantsDoubleJump = false)
            {
                Id = id;
                Available = available;
                GrantsDoubleJump = grantsDoubleJump;
            }
        }

        [Serializable]
        public struct Connection
        {
            public int from;
            public int to;
            public bool requiresDoubleJump;
            public bool blocked;

            public Connection(int from, int to, bool requiresDoubleJump = false, bool blocked = false)
            {
                this.from = from;
                this.to = to;
                this.requiresDoubleJump = requiresDoubleJump;
                this.blocked = blocked;
            }
        }

        public sealed class Result
        {
            public int[] Path { get; }
            public int VisitedStates { get; }
            public string Error { get; }
            public bool Found => Path.Length > 0;

            internal Result(int[] path, int visitedStates, string error = null)
            {
                Path = path;
                VisitedStates = visitedStates;
                Error = error;
            }
        }

        public static Result FindPath(IReadOnlyList<Node> nodes, IReadOnlyList<Connection> connections, int start, int goal)
        {
            Result Invalid(string reason) => new Result(Array.Empty<int>(), 0, reason);
            if (nodes == null || nodes.Count == 0 || connections == null)
                return Invalid("O grafo precisa de pontos e uma lista de conexões.");
            if (start < 0 || start >= nodes.Count || goal < 0 || goal >= nodes.Count)
                return Invalid("A entrada ou a saída está fora do grafo.");

            var ids = new HashSet<string>();
            var outgoing = new List<Connection>[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(nodes[i].Id) || !ids.Add(nodes[i].Id))
                    return Invalid("Cada ponto precisa de um identificador único e não vazio.");
                outgoing[i] = new List<Connection>();
            }
            foreach (Connection edge in connections)
            {
                if (edge.from < 0 || edge.from >= nodes.Count || edge.to < 0 || edge.to >= nodes.Count)
                    return Invalid("Uma conexão aponta para um ponto inexistente.");
                if (!edge.blocked)
                    outgoing[edge.from].Add(edge);
            }
            if (!nodes[start].Available || !nodes[goal].Available)
                return new Result(Array.Empty<int>(), 0);

            var initial = (node: start, power: nodes[start].GrantsDoubleJump);
            var pending = new Queue<(int node, bool power)>();
            var visited = new HashSet<(int node, bool power)> { initial };
            var previous = new Dictionary<(int node, bool power), (int node, bool power)>();
            pending.Enqueue(initial);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                if (current.node == goal)
                {
                    var path = new List<int> { current.node };
                    while (previous.TryGetValue(current, out var parent))
                    {
                        path.Add(parent.node);
                        current = parent;
                    }
                    path.Reverse();
                    return new Result(path.ToArray(), visited.Count);
                }

                foreach (Connection edge in outgoing[current.node])
                {
                    if (!nodes[edge.to].Available || (edge.requiresDoubleJump && !current.power))
                        continue;
                    var next = (node: edge.to, power: current.power || nodes[edge.to].GrantsDoubleJump);
                    if (!visited.Add(next))
                        continue;
                    previous[next] = current;
                    pending.Enqueue(next);
                }
            }
            return new Result(Array.Empty<int>(), visited.Count);
        }
    }
}
