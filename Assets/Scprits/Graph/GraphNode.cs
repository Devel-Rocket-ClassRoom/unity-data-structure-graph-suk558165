using UnityEngine;
using System.Collections.Generic;

public class GraphNode
{
    public int id;
    public int weight = 1;

    public GraphNode previous = null;

    public List<GraphNode> adjacents = new List<GraphNode>();

    public bool CanVisit => adjacents.Count > 0 && weight > 0;
}
