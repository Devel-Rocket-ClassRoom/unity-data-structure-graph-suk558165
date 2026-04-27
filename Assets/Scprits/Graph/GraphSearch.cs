using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphSearch
{
    private Graph graph;
    public List<GraphNode> path = new List<GraphNode>();

    public void Init(Graph graph)
    {
        this.graph = graph;

        for (int i = 0; i < this.graph.nodes.Length; i++)
        {
            this.graph.nodes[i].previous = null;
        }
    }
    public void DFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var stack = new Stack<GraphNode>();

        stack.Push(node);
        visited.Add(node);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            path.Add(currentNode);

            foreach (var adjacents in currentNode.adjacents)
            {
                if (!adjacents.CanVisit || visited.Contains(adjacents))
                    continue;

                visited.Add(adjacents);
                stack.Push(adjacents);
            }
        }
    }

    public void DFS_Recur(GraphNode node)
    {
        path.Clear();
        var visited = new HashSet<GraphNode>();
        
        DFS_R(node, ref visited);
    }

    public void DFS_R(GraphNode node, ref HashSet<GraphNode> visited)
    {
        path.Add(node);
        visited.Add(node);
        foreach (var adjacents in node.adjacents)
        {
            if (!adjacents.CanVisit || visited.Contains(adjacents))
                continue;
            DFS_R(adjacents, ref visited);
        }
    }

    public void BFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(node);
        visited.Add(node);

        while(queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            path.Add(currentNode);

            foreach (var adjacents in currentNode.adjacents)
            {
                if (!adjacents.CanVisit || visited.Contains(adjacents))
                    continue;

                visited.Add(adjacents);
                queue.Enqueue(adjacents);
            }
        }
    }

    public void BFS_F(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>();
        var queue = new Queue<GraphNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);
        
        while(queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            if (ReferenceEquals(currentNode, endNode))
            {
                var preNode = currentNode;
                path.Add(preNode);
                while (preNode.previous != null)
                {
                    path.Add(preNode.previous);
                    preNode = preNode.previous;
                }
                path.Reverse();
                break;
            }
            foreach (var adjacents in currentNode.adjacents)
            {
                if (!adjacents.CanVisit || visited.Contains(adjacents))
                    continue;
                adjacents.previous = currentNode;
                visited.Add(adjacents);
                queue.Enqueue(adjacents);
            }
        }
    }

    public void Dijkstra(GraphNode startNode, GraphNode endNode)
    {
        var pq = new PriorityQueue<GraphNode, int>(Comparer<int>.Default);
        var visited = new HashSet<GraphNode>();
        path.Clear();
        pq.Enqueue(startNode, 0);
        visited.Add(startNode);
        int[] dist = new int[graph.nodes.Length];

        for (int i = 0; i < dist.Length; i++)
        {
            dist[i] = int.MaxValue;
        }
        dist[startNode.id] = 0;
        
        while(pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            visited.Add(currentNode);
            // if (currentNode.weight > dist[currentNode.id])
            //     continue;

            if (ReferenceEquals(currentNode, endNode))
            {
                var preNode = currentNode;
                path.Add(preNode);
                while (preNode.previous != null)
                {
                    path.Add(preNode.previous);
                    preNode = preNode.previous;
                }
                path.Reverse();
                break;
            }
            
            foreach (var adjacents in currentNode.adjacents)
            {
                if (!adjacents.CanVisit || visited.Contains(adjacents))
                    continue;

                int newDist = dist[currentNode.id] + adjacents.weight;
                
                if (newDist < dist[adjacents.id])
                {
                    dist[adjacents.id] = newDist;
                    adjacents.previous = currentNode;
                    pq.Enqueue(adjacents, newDist);
                }
            }
        }
    }

    private int Heuristic(GraphNode a, GraphNode b)
    {
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public void AStar(GraphNode startNode, GraphNode endNode)
    {
        var pq = new PriorityQueue<GraphNode, int>(Comparer<int>.Default);
        var visited = new HashSet<GraphNode>();
        path.Clear();
        int[] dist = new int[graph.nodes.Length];

        for (int i = 0; i < dist.Length; i++)
        {
            dist[i] = int.MaxValue;
        }
        dist[startNode.id] = 0;
        pq.Enqueue(startNode, dist[startNode.id] + Heuristic(startNode, endNode));
        
        while(pq.Count > 0)
        {
            var currentNode = pq.Dequeue();
            visited.Add(currentNode);
            // if (currentNode.weight > dist[currentNode.id])
            //     continue;

            if (ReferenceEquals(currentNode, endNode))
            {
                var preNode = currentNode;
                path.Add(preNode);
                while (preNode.previous != null)
                {
                    path.Add(preNode.previous);
                    preNode = preNode.previous;
                }
                path.Reverse();
                break;
            }
            
            foreach (var adjacents in currentNode.adjacents)
            {
                if (!adjacents.CanVisit || visited.Contains(adjacents))
                    continue;

                int newDist = dist[currentNode.id] + adjacents.weight;
                
                if (newDist < dist[adjacents.id])
                {
                    dist[adjacents.id] = newDist;
                    adjacents.previous = currentNode;
                    pq.Enqueue(adjacents, newDist + Heuristic(currentNode, endNode));
                }
            }
        }
    }
}
