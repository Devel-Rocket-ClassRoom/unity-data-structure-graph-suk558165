using System.Collections.Generic;
using UnityEngine;

public abstract class Algorithm : MonoBehaviour
{
    protected Graph graph;

    public List<GraphNode> path = new List<GraphNode>();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    public abstract void Run(GraphNode start);
}
