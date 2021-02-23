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
    private List<int> _path = new List<int>();
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

    /// <summary>
    /// Connects last point to first point and creates a circuit. Might be intersecting, check that first
    /// </summary>
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

    /// <summary>
    /// Checks if the closing of the circuit from this position would make intersections. If it does -> how many?
    /// </summary>
    /// <param name="amount">How many intersections were made in trying to close the circuit</param>
    /// <returns>True if circuit intersects</returns>
    public bool DoesHypotheticalCircuitIntersect(out int amount)
    {
        amount = 0;
        //Hypothetical circuit closing line to check against
        Vector2 p1 = _voronoiGraph.GetGraphNodeByIndex(_path[0]).position;
        Vector2 p2 = _voronoiGraph.GetGraphNodeByIndex(_path[_path.Count - 1]).position;

        for (int i = _path.Count - 2; i >= 3; i--)
        {
            //Points in line to check against 
            Vector2 p3 = _voronoiGraph.GetGraphNodeByIndex(_path[i]).position;
            Vector2 p4 = _voronoiGraph.GetGraphNodeByIndex(_path[i - 1]).position;

            float t = (p2.x - p3.x) * (p3.y - p4.y) - (p2.y - p3.y) * (p3.x - p4.x);
            t /= (p2.x - p1.x) * (p3.y - p4.y) - (p2.y - p1.y) * (p3.x - p4.x);

            float u = (p1.x - p2.x) * (p2.y - p3.y) - (p1.y - p2.y) * (p2.x - p3.x);
            u /= (p2.x - p1.x) * (p3.y - p4.y) - (p2.y - p1.y) * (p3.x - p4.x);

            //Line intersect
            if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                amount++;
        }

        return amount > 0;
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
    static GameObject g;
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
                path.DoesHypotheticalCircuitIntersect(out int amount);
                Debug.Log("Intersect: " + (amount > 0) + ", " + amount + " times");
            }
        }

        path.ConnectCircuit();

        return path.PathInPoints;
    }
}
