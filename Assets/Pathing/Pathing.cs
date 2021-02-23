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


        int testCount = 0;
        int testLimit = 16;

        while (true)
        {
            path.UpdateCurrentPoint();
            //Valid point! (for now UmU)
            if (path.CanContinuePath)
            {
                path.AddPointFromNeighbour();
                path.UpdateLongestPath();
                testCount++;
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
                testCount--;
            }

            //Time to try and close
            if (path.HypothecialCircuitLength > preferredLength)
            {

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
