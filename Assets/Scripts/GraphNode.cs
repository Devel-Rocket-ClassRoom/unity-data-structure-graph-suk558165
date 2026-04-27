using System.Collections.Generic;
using UnityEngine;

public class GraphNode 
{
    public int id;

    public int weight;

    public GraphNode prividus = null;

    public List<GraphNode> adjacents = new List<GraphNode>();

    public bool CanVisit => adjacents.Count > 0 && weight > 0;
}
