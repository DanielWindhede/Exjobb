﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiDiagramGenerator
{
    /// <summary>
    /// Create a Voronoi graph based of a Delaunay triangulation
    /// </summary>
    /// <param name="triangulation">List of all triangles in Delaunay triangulation</param>
    /// <returns>A traversable graph with points</returns>
    public static VoronoiGraph GenerateVoronoiFromDelaunay(List<Triangle> triangulation, Vector2 bounds)
    {
        RemoveTrianglesOutsideOfBounds(bounds, ref triangulation);

        List<GraphNode> allNodes = new List<GraphNode>();
        HashSet<Vector2> allPoints = new HashSet<Vector2>();

        //Creates a node from each triangel center and adds points
        foreach (Triangle triangle in triangulation)
        {
            allNodes.Add(new GraphNode(triangle.Center));
            foreach (Vector2 point in triangle.vertices)
                allPoints.Add(point);
        }

        //Connects nodes if triangels share edges
        for (int i = 0; i < triangulation.Count; i++)
        {
            //Used to skip looping over unnecessary triangels if all connections has been found
            int edgeCounter = 0;
            for (int j = 0; j < triangulation.Count && edgeCounter < 3; j++)
            {
                //Skip itself
                if (triangulation[j] == triangulation[i])
                    continue;

                if (triangulation[i].SharesAnyEdge(triangulation[j]))
                {
                    allNodes[i].AddNeighbourPosition(j);
                    edgeCounter++;
                }
            }
        }

        return new VoronoiGraph(allNodes, allPoints);
    }

    static void RemoveTrianglesOutsideOfBounds(Vector2 bounds, ref List<Triangle> triangulation)
    {
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];

            if (triangle.Center.x > bounds.x ||
                triangle.Center.x < 0 ||
                triangle.Center.y > bounds.y ||
                triangle.Center.y < 0)
            {
                triangulation.RemoveAt(i);
            }
        }
    }
}
