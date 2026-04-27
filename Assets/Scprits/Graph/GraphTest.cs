using System.Collections.Generic;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public enum Algorithm
    {
        DFS,
        BFS,
        BFS_F,
        Dijkstra,
        AStar
    }
    public Transform uiNodeRoot;
    public UIGraphNode nodePrefab;
    private List<UIGraphNode> uiNodes = new List<UIGraphNode>();
    private Graph graph;
    public Algorithm algorithm;
    public int startId;
    public int endId;

    private void Start()
    {
        int[,] map = new int[5,5]
        {
            {2,-1,4,-1,3},
            {8,1,9,3,8},
            {2,6,3,-1,6},
            {7,-1,1,-1,2},
            {3,4,6,9,4}
        };

        graph = new Graph();
        graph.Init(map);
        InitUINodes(graph);
    }

    private void InitUINodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, uiNodeRoot);
            uiNode.SetNode(node);
            uiNode.Reset();
            uiNodes.Add(uiNode);
        }
    }

    private void ResetUINodes()
    {
        foreach (var uiNode in uiNodes)
        {
            uiNode.Reset();
        }
    }

    [ContextMenu("Search")]
    private void Search()
    {
        var search = new GraphSearch();
        search.Init(graph);

        switch (algorithm)
        {
            case Algorithm.DFS:
                search.DFS(graph.nodes[startId]);
                break;
            case Algorithm.BFS:
                search.BFS(graph.nodes[startId]);
                break;
            case Algorithm.BFS_F:
                search.BFS_F(graph.nodes[startId], graph.nodes[endId]);
                break;
            case Algorithm.Dijkstra:
                search.Dijkstra(graph.nodes[startId], graph.nodes[endId]);
                break;
            case Algorithm.AStar:
                search.AStar(graph.nodes[startId], graph.nodes[endId]);
                break;
        }

        ResetUINodes();
        if (search.path.Count <= 1)
        {
            if (search.path.Count == 1)
            {
                var only = search.path[0];
                uiNodes[only.id].SetColor(Color.red);
            }

            return;
        }

        for (int i = 0; i < search.path.Count; i++)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.red, Color.green, (float)i / (search.path.Count - 1));
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID : {node.id}\nWeight : {node.weight}\nPath : {i}");
        }
    }
}
