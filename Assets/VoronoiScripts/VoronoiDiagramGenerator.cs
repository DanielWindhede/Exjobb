using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiDiagramGenerator
{
    public static Graph GenerateVoronoiFromDelaunay(List<Triangle> triangulation)
    {
        List<Node> allNodes = new List<Node>();
        foreach (Triangle triangle in triangulation)
            allNodes.Add(new Node(triangle.Center));

        for (int i = 0; i < triangulation.Count; i++)
        {
            int edgeCounter = 0;
            for (int j = 0; j < triangulation.Count && edgeCounter < 3; j++)
            {
                if (triangulation[i].SharesAnyEdge(triangulation[j]))
                {
                    allNodes[i].AddNeighbour(allNodes[j]);
                    edgeCounter++;
                }
            }
        }

        return new Graph(allNodes);
    }
}
