using System.Collections.Generic;
using UnityEngine;

public class GraphTest : MonoBehaviour
{

    public enum Algorithm
    {
        DFS,
        BFS,
        DFSRecursive,
        PathFindingBFS,

        Dijkstra,

        AStar,
    
    }
    public Transform uiNodeRoot;

    public UiGraphNode nodePrefab;

    private List<UiGraphNode> uiNodes = new List<UiGraphNode>();

    public Algorithm algorithm;
    public GraphSearch graphSearch;
    private Graph graph;

     public int startId;

     public int endId;

    // DFS / BFS 용: 벽(-1)이 있는 맵
    private readonly int[,] mapWithWall = new int[5, 5]
    {
        {1,-1, 1, 1, 1},
        {1,-1, 1, 1, 1},
        {1,-1, 1, 1, 1},
        {1,-1, 1, 1, 1},
        {1, 1, 1, 1, 1},
    };

    // Dijkstra / A* 용: 장애물 없는 열린 맵
    private readonly int[,] mapOpen = new int[5, 5]
    {
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1},
    };

    private void Start()
    {
        graph = new Graph();
        graph.Init(mapWithWall);
        InitUiNodes(graph);
    }

    private void InitUiNodes(Graph graph)
    {
        foreach(var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, uiNodeRoot);
            uiNode.SetNode(node);
            uiNode.Reset();
            uiNodes.Add(uiNode);
        }
    }

    private void ResetUiNodes()
    {
        foreach (var uiNode in uiNodes)
        {
            uiNode.Reset();
        }
    }

    [ContextMenu("Search")]
    public void Search()
    {
        bool isWeighted = algorithm == Algorithm.Dijkstra || algorithm == Algorithm.AStar;
        graph.Init(isWeighted ? mapOpen : mapWithWall);
        graphSearch.Init(graph);

        switch(algorithm)
        {
            case Algorithm.DFS:
            graphSearch.DFS(graph.nodes[startId]);
            break;

            case Algorithm.BFS:
            graphSearch.BFS(graph.nodes[startId]);
            break;

            case Algorithm.DFSRecursive:
            graphSearch.DFSRecursive(graph.nodes[startId]);
            break;

            case Algorithm.PathFindingBFS:
            graphSearch.PathFindingBFS(graph.nodes[startId], graph.nodes[endId]);
            break;

            case Algorithm.Dijkstra:
            graphSearch.Dijkstra(graph.nodes[startId], graph.nodes[endId]);
            break;

            case Algorithm.AStar:
            graphSearch.AStar(graph.nodes[startId], graph.nodes[endId]);
            break;
        }

        ResetUiNodes();
        if (graphSearch.path.Count <= 1)
        {
            if (graphSearch.path.Count == 1)
            {
                var only = graphSearch.path[0];
                uiNodes[only.id].SetColor(Color.red);
            }
            return;
        }
        for ( int i = 0; i < graphSearch.path.Count; ++i)
        {
            var node = graphSearch.path[i];
            var color = Color.Lerp(Color.red, Color.green, (float)i / (graphSearch.path.Count - 1));
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID: {node.id}\n Weight: {node.weight}\n Path: {i}");
        }
    }
    
}
