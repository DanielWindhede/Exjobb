using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiDiagramGenerator
{
    public static VoronoiGraph GenerateVoronoiFromDelaunay(List<Triangle> triangulation)
    {
        List<GraphNode> allNodes = new List<GraphNode>();
        HashSet<Vector2> allPoints = new HashSet<Vector2>();
        foreach (Triangle triangle in triangulation)
        {
            allNodes.Add(new GraphNode(triangle.Center));
            foreach (Vector2 point in triangle.vertices)
                allPoints.Add(point);
        }

        for (int i = 0; i < triangulation.Count; i++)
        {
            int edgeCounter = 0;
            for (int j = 0; j < triangulation.Count && edgeCounter < 3; j++)
            {
                if (triangulation[j] == triangulation[i])
                    continue;

                if (triangulation[i].SharesAnyEdge(triangulation[j]))
                {
                    allNodes[i].AddNeighbourPosition(allNodes[j].position);
                    edgeCounter++;
                }
            }
        }

        return new VoronoiGraph(allNodes, allPoints);
    }
}
