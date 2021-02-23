using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiPath
{
    /// <summary>
    /// Index of the last point in the path
    /// </summary>
    public int CurrentIndex { get; private set; }
    /// <summary>
    /// Path length in km
    /// </summary>
    public float CurrentLength { get; private set; } = 0;

    private VoronoiGraph _voronoiGraph;
    /// <summary>
    /// The current path
    /// </summary>
    public List<int> _path = new List<int>(); // TODO TA BORT
    /// <summary>
    /// The longest path that has been attempted so far
    /// </summary>
    private List<int> _longestPath = new List<int>();
    /// <summary>
    /// Points that has been visited previously
    /// </summary>
    private HashSet<int> _illegalPoints = new HashSet<int>();

    /// <summary>
    /// Neighbour indexes to current point
    /// </summary>
    private List<int> _currentNeighbours = new List<int>();

    /// <summary>
    /// There are paths from current point that can be taken
    /// </summary>
    public bool CanContinuePath { get { return _currentNeighbours.Count > 0; } }
    public bool TriedAllCases { get { return _path.Count == 0; } }

    /// <summary>
    /// Returns length on circuit if path was made to circuit by connecting first and last point
    /// </summary>
    public float HypothecialCircuitLength
    {
        get
        {
            if (_path.Count > 1)
                return CurrentLength + DistanceBetweenTwoPoints(_path.Count - 1, 0);
            else
                return 0;
        }
    }

    public VoronoiPath(VoronoiGraph voronoiGraph, int startIndex)
    {
        _voronoiGraph = voronoiGraph;
        CurrentIndex = startIndex;
        _path.Add(CurrentIndex);
    }

    /// <summary>
    /// Returns a list of each points position in path order
    /// </summary>
    public List<Vector2> PathInPoints
    {
        get
        {
            List<Vector2> pathPositions = new List<Vector2>();
            for (int i = 0; i < _path.Count; i++)
                pathPositions.Add(_voronoiGraph.GetGraphNodeByIndex(_path[i]).position);

            return pathPositions;
        }
    }

    /// <summary>
    /// Calculate neighbours from Current point
    /// </summary>
    public void UpdateCurrentPoint()
    {
        _illegalPoints.Add(CurrentIndex);
        _currentNeighbours = new List<int>(_voronoiGraph.GetGraphNodeByIndex(CurrentIndex).neighborNodeIndexes);
        foreach (int index in _illegalPoints)
            _currentNeighbours.Remove(index);
    }

    /// <summary>
    /// Continues path with random available path from current to neighbour
    /// </summary>
    public void AddPointFromNeighbour()
    {
        _path.Add(CurrentIndex);
        CurrentIndex = _currentNeighbours[Random.Range(0, _currentNeighbours.Count)];

        CurrentLength += DistanceBetweenTwoPoints(_path.Count - 1, _path.Count - 2);
    }

    public void ConnectCircuit()
    {
        _path.Add(_path[0]);
        CurrentLength += DistanceBetweenTwoPoints(_path.Count - 1, _path.Count - 2);
    }

    public void UpdateLongestPath()
    {
        if (_longestPath.Count < _path.Count)
            _longestPath = new List<int>(_path);
    }

    /// <summary>
    /// Remove current index and step one step back in path
    /// </summary>
    public void BackPathOneStep()
    {
        if (_path.Count > 1)
            CurrentLength -= DistanceBetweenTwoPoints(_path.Count - 1, _path.Count - 2);
        else
            CurrentLength = 0;

        CurrentIndex = _path[_path.Count - 1];
        _path.RemoveAt(_path.Count - 1);
    }

    public void SetLongestPathToPath()
    {
        _path = _longestPath;
    }

    float DistanceBetweenTwoPoints(int index1, int index2)
    {
        return (_voronoiGraph.GetGraphNodeByIndex(_path[index1]).position - _voronoiGraph.GetGraphNodeByIndex(_path[index2]).position).magnitude;
    }
}

public class Pathing
{
    public static List<Vector2> GenerateRandomCircuit(VoronoiGraph voronoiGraph, float maxLength, float minLength)
    {
        VoronoiPath path = new VoronoiPath(voronoiGraph, Random.Range(0, voronoiGraph.AllNodesCount));
        float preferredLength = Random.Range(minLength, maxLength);

        while (path.HypothecialCircuitLength < preferredLength)
        {
            path.UpdateCurrentPoint();
            //Valid point! (for now UmU)
            if (path.CanContinuePath)
            {
                path.AddPointFromNeighbour();
                path.UpdateLongestPath();
            }
            //Path can't proceed further -> back-track
            else
            {
                if (path.TriedAllCases)
                {
                    path.SetLongestPathToPath();
                    break;
                }

                path.BackPathOneStep();
            }

            //Time to try and close
            if (path.HypothecialCircuitLength > preferredLength)
            {

            }
        }

        /*
        Vector2 p1 = voronoiGraph.GetGraphNodeByIndex(path._path[path._path.Count - 1]).position;
        Vector2 p2 = voronoiGraph.GetGraphNodeByIndex(path._path[0]).position;
        */
        Vector2 p1 = voronoiGraph.GetGraphNodeByIndex(path._path[path._path.Count - 1]).position;
        Vector2 p2 = voronoiGraph.GetGraphNodeByIndex(path._path[0]).position;

        GameObject.Instantiate(new GameObject("p1"), p1, Quaternion.identity);
        GameObject.Instantiate(new GameObject("p2"), p2, Quaternion.identity);

        Debug.Log("P1: " + p1 + ", P2: " + p2);

        float t = 0;
        float u = 0;
        for (int i = path._path.Count - 1; i >= 1; i--)
        {
            Vector2 p3 = voronoiGraph.GetGraphNodeByIndex(path._path[i]).position;
            Vector2 p4 = voronoiGraph.GetGraphNodeByIndex(path._path[i - 1]).position;

            GameObject.Instantiate(new GameObject("p3 variation " + i), p3, Quaternion.identity);
            GameObject.Instantiate(new GameObject("p4 variation " + i), p4, Quaternion.identity);

            t = (p1.x - p3.x) * (p3.y - p4.y) - (p1.y - p3.y) * (p3.x - p4.x);
            t /= (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);

            u = (p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x);
            u /= (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);

            if (0 <= t && t <= 1 && 0 <= u && u <= 1)
            {
                Debug.Log("t: " + t + ", u: " + u);
                Vector2 point1 = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
                Debug.Log("p1: " + p1 + "p2: " + p2 + "p3: " + p3 + "p4: " + p4 + "Intersection Point: " + point1);
                Vector2 point2 = new Vector2(p3.x + u * (p4.x - p3.x), p3.y + t * (p4.y - p3.y));
                Debug.Log("p1: " + p1 + "p2: " + p2 + "p3: " + p3 + "p4: " + p4 + "Intersection Point: " + point2);

                /*
                if (0 <= t && t <= 1)
                {
                    Debug.Log("Intersection at line1: " + t);
                    Vector2 point = new Vector2(p1.x + t * (p2.x - p1.x), p1.y + t * (p2.y - p1.y));
                    Debug.Log("p1: " + p1 + "p2: " + p2 + "p3: " + p3 + "p4: " + p4 + "Intersection Point: " + point);
                }
                if (0 <= u && u <= 1)
                {
                    Debug.Log("Intersection at line2 : " + u);
                    Vector2 point = new Vector2(p3.x + u * (p4.x - p3.x), p3.y + t * (p4.y - p3.y));
                    Debug.Log("p1: " + p1 + "p2: " + p2 + "p3: " + p3 + "p4: " + p4 + "Intersection Point: " + point);
                }
                */
            }
        }

        path.ConnectCircuit();

        return path.PathInPoints;
    }
}





/*
public static List<Vector2> GenerateRandomCircuit(VoronoiGraph voronoiGraph, float maxLength, float minLength)
    {
        VoronoiPath path = new VoronoiPath(voronoiGraph, Random.Range(0, voronoiGraph.AllNodesCount));
        //HashSet<int> illegalPoints = new HashSet<int>();
        //List<int> path = new List<int>();
        //List<int> longestPath = new List<int>();
        //int currentIndex = Random.Range(0, voronoiGraph.AllNodesCount);
        float preferredLength = Random.Range(minLength, maxLength);


        int testCount = 0;
        int testLimit = 16;

        while (testCount < testLimit)
        {
            //illegalPoints.Add(currentIndex);
            //List<int> neighbourIndexes = new List<int>(voronoiGraph.GetGraphNodeByIndex(currentIndex).neighborNodeIndexes);

            //foreach (int index in illegalPoints)
            //    neighbourIndexes.Remove(index);

            //Path can't proceed further -> back-track
            //if (neighbourIndexes.Count == 0)
            //{
                //if (path.Count == 0)
                //{
                //    path = longestPath;
                //    break;
                //}

                //currentIndex = path[path.Count - 1];
                //path.RemoveAt(path.Count - 1);

                //testCount--;
            //}
            //Valid point! (for now UmU)
            //else
            //{
                //path.Add(currentIndex);

                //if (longestPath.Count < path.Count)
                //    longestPath = new List<int>(path);

                //currentIndex = neighbourIndexes[Random.Range(0, neighbourIndexes.Count)];
                //testCount++;
            //}
        }

        List<Vector2> pathPositions = new List<Vector2>();
        for (int i = 0; i < path.Count; i++)
            pathPositions.Add(voronoiGraph.GetGraphNodeByIndex(path[i]).position);

        pathPositions.Add(pathPositions[0]);

        return pathPositions;
    }
*/
