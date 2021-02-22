using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing
{
    public static List<Vector2> GenerateRandomCircuit(VoronoiGraph voronoiGraph, float maxLength, float minLength)
    {
        HashSet<int> illegalPoints = new HashSet<int>();
        List<int> path = new List<int>();
        List<int> longestPath = new List<int>();
        int currentIndex = Random.Range(0, voronoiGraph.AllNodesCount);
        float preferredLength = Random.Range(minLength, maxLength);


        int testCount = 0;
        int testLimit = 16;

        while (testCount < testLimit)
        {
            illegalPoints.Add(currentIndex);
            List<int> neighbourIndexes = new List<int>(voronoiGraph.GetGraphNodeByIndex(currentIndex).neighborNodeIndexes);

            foreach (int index in illegalPoints)
                neighbourIndexes.Remove(index);

            //Path can't proceed further -> back-track
            if (neighbourIndexes.Count == 0)
            {
                if (path.Count == 0)
                {
                    path = longestPath;
                    break;
                }

                currentIndex = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);

                testCount--;
            }
            //Valid point! (for now UmU)
            else
            {
                path.Add(currentIndex);

                if (longestPath.Count < path.Count)
                    longestPath = new List<int>(path);

                currentIndex = neighbourIndexes[Random.Range(0, neighbourIndexes.Count)];
                testCount++;
            }
        }

        List<Vector2> pathPositions = new List<Vector2>();
        for (int i = 0; i < path.Count; i++)
            pathPositions.Add(voronoiGraph.GetGraphNodeByIndex(path[i]).position);

        pathPositions.Add(pathPositions[0]);

        return pathPositions;
    }
}
