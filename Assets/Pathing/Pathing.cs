﻿using System.Collections;
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
    /// Points that has been visited previously
    /// </summary>
    private HashSet<int> _illegalPoints = new HashSet<int>();

    /// <summary>
    /// Neighbour indexes to current point
    /// </summary>
    private List<int> _currentNeighbours = new List<int>();
    private float _maxStraightLength;
    private float _minStraightLength;
    private float _minLengthStartGrid;
    private float _minLengthFromFinishLine;
    private Vector2 _finishLine;
    private float _minNodeLength;
    //Nodes that can't be accessed beyond this point
    private List<int> _tooCloseNodes = new List<int>();
    private float _minTurnAngle;

    /// <summary>
    /// There are paths from current point that can be taken
    /// </summary>
    public bool CanContinuePath { get { return _currentNeighbours.Count > 0; } }
    public bool TriedAllCases { get { return _path.Count == 0; } }

    public bool IsHypotheticalCircuitStraightTooLong { get { return ClosingStraightLength > _maxStraightLength; } }
    public bool IsHypotheticalCircuitStraightTooShort { get { return ClosingStraightLength < _minStraightLength; } }
    public float ClosingStraightLength { get { return DistanceBetweenTwoPoints(_path[_path.Count - 1], _path[0]); } }

    public bool ValidClosingAngle
    {
        get
        {
            Vector2 closingVector = _voronoiGraph.GetGraphNodeByIndex(_path[0]).position - _voronoiGraph.GetGraphNodeByIndex(_path[_path.Count - 1]).position;
            Vector2 previousVector = _voronoiGraph.GetGraphNodeByIndex(_path[_path.Count - 2]).position - _voronoiGraph.GetGraphNodeByIndex(_path[_path.Count - 1]).position;
            Vector2 firstVector = _voronoiGraph.GetGraphNodeByIndex(_path[0]).position - _voronoiGraph.GetGraphNodeByIndex(_path[1]).position;

            return Vector2.Angle(closingVector, previousVector) > _minTurnAngle && Vector2.Angle(closingVector, firstVector) > _minTurnAngle;
        }
    }

    /// <summary>
    /// Returns length on circuit if path was made to circuit by connecting first and last point
    /// </summary>
    public float HypothecialCircuitLength
    {
        get
        {
            if (_path.Count > 1)
                return CurrentLength + ClosingStraightLength;
            else
                return 0;
        }
    }

    public float LongestStraight
    {
        get
        {
            float longestStraight = ClosingStraightLength;
            List<Vector2> points = PathInPoints;

            //Skip hypothetical closing straight
            for (int i = 1; i < points.Count - 2; i++)
            {
                float thisStraight = (points[i] - points[i + 1]).magnitude;
                longestStraight = thisStraight > longestStraight ? thisStraight : longestStraight;
            }

            return longestStraight;
        }
    }

    public bool IsClockWise
    {
        get
        {
            List<Vector2> points = PathInPoints;

            float total = AddEdge(points[points.Count - 2], points[1]);
            for (int i = 1; i < points.Count - 2; i++)
                total += AddEdge(points[i], points[i + 1]);

            return total > 0;
        }
    }

    private float AddEdge(Vector2 p1, Vector2 p2)
    {
        return (p2.x - p1.x) * (p2.y + p1.y);
    }

    public VoronoiPath(VoronoiGraph voronoiGraph, int startIndex,
                       float maxStraightLength, float minStraightLength,
                       float minLengthFromFinishLine, float minLengthStartGrid,
                       float minNodeLength, float minTurnAngle)
    {
        _voronoiGraph = voronoiGraph;
        CurrentIndex = startIndex;
        _maxStraightLength = maxStraightLength;
        _minStraightLength = minStraightLength;
        _minLengthStartGrid = minLengthStartGrid;
        _minLengthFromFinishLine = minLengthFromFinishLine;
        _minNodeLength = minNodeLength;
        _minTurnAngle = minTurnAngle;
    }

    /// <summary>
    /// Returns a list of each points position in path order
    /// </summary>
    public List<Vector2> PathInPoints
    {
        get
        {
            List<Vector2> pathPositions = new List<Vector2>();

            pathPositions.Add(_finishLine);

            for (int i = 0; i < _path.Count - 1; i++)
                pathPositions.Add(_voronoiGraph.GetGraphNodeByIndex(_path[i]).position);

            pathPositions.Add(_finishLine);

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
        for (int i = _currentNeighbours.Count - 1; i >= 0; i--)
        {
            if (_illegalPoints.Contains(_currentNeighbours[i]))
                _currentNeighbours.RemoveAt(i);
        }

        //Remove neighbours which edge length is > than maxStraightLength
        for (int i = _currentNeighbours.Count - 1; i >= 0; i--)
        {
            //Travelling along this edge creates too long straight
            if (DistanceBetweenTwoPoints(CurrentIndex, _currentNeighbours[i]) > _maxStraightLength)
                _currentNeighbours.RemoveAt(i);
            //This neighbour is really close to current
            if (DistanceBetweenTwoPoints(CurrentIndex, _currentNeighbours[i]) < _minNodeLength)
                _illegalPoints.Add(_currentNeighbours[i]);
        }
    }

    /// <summary>
    /// Continues path with random available path from current to neighbour
    /// </summary>
    public void AddPointFromNeighbour()
    {
        _path.Add(CurrentIndex);
        CurrentIndex = _currentNeighbours[Random.Range(0, _currentNeighbours.Count)];

        if (_path.Count > 1)
            CurrentLength += DistanceBetweenTwoPoints(_path[_path.Count - 1], _path[_path.Count - 2]);
    }

    /// <summary>
    /// Connects last point to first point and creates a circuit. Might be intersecting, check that first
    /// </summary>
    public void ConnectCircuit()
    {
        _path.Add(_path[0]);
        CurrentLength += DistanceBetweenTwoPoints(_path[_path.Count - 1], _path[_path.Count - 2]);
    }

    /// <summary>
    /// Remove current index and step one step back in path
    /// </summary>
    public void BackPathOneStep()
    {
        if (_path.Count > 1)
            CurrentLength -= DistanceBetweenTwoPoints(_path[_path.Count - 1], _path[_path.Count - 2]);
        else
            CurrentLength = 0;

        CurrentIndex = _path[_path.Count - 1];
        _path.RemoveAt(_path.Count - 1);
    }

    public void BackCompletely(float minLength)
    {
        while (HypothecialCircuitLength > minLength)
            BackPathOneStep();
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

        for (int i = _path.Count - 2; i >= 2; i--)
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

    float DistanceBetweenTwoPoints(int index1, int index2)
    {
        return (_voronoiGraph.GetGraphNodeByIndex(index1).position - _voronoiGraph.GetGraphNodeByIndex(index2).position).magnitude;
    }

    /// <summary>
    /// Creates a point for finish line along hypothetical ending line
    /// </summary>
    public void InsertFinishLine()
    {
        Vector2 startPoint = _voronoiGraph.GetGraphNodeByIndex(_path[0]).position; // 250
        Vector2 endPoint = _voronoiGraph.GetGraphNodeByIndex(_path[_path.Count - 1]).position; // 208

        Vector2 direction = (endPoint - startPoint).normalized;

        Vector2 a = startPoint + direction * _minLengthStartGrid;
        Vector2 b = endPoint + -direction * _minLengthFromFinishLine;

        float t = Random.Range(0f, 1f);
        _finishLine = Vector2.Lerp(a, b, t);
    }
}

public class Pathing
{
    /// <summary>
    /// Attempts to create a circuit based on a voronoi graph that does not intersect and is within correct length
    /// </summary>
    /// <param name="voronoiGraph">The graph the path is based on</param>
    /// <param name="minLength">The minimum circuit length that is valid</param>
    /// <param name="maxLength">The maxiumum circuit length that is valid</param>
    /// <returns>An ordered list of points making up the circuit</returns>
    public static List<Vector2> GenerateRandomCircuit(VoronoiGraph voronoiGraph, float minLength, float maxLength,
                                                      float maxStraightLength, float minStraightLength, float minLengthStartGrid,
                                                      float minLengthFromFinishLine, float minNodeLength, float minTurnAngle,
                                                      ref int recursionCounter, ref CircuitInformation circuitInformation)
    {
        VoronoiPath path = new VoronoiPath(voronoiGraph, Random.Range(0, voronoiGraph.AllNodesCount), maxStraightLength, minStraightLength, minLengthFromFinishLine, minLengthStartGrid, minNodeLength, minTurnAngle);
        float preferredLength = Random.Range(minLength, maxLength);
        bool foundValidCircuit = true;

        while (true)
        {
            path.UpdateCurrentPoint();
            //Valid point! (for now UmU)
            if (path.CanContinuePath)
            {
                path.AddPointFromNeighbour();
                //path.UpdateLongestPath();
            }
            //Path can't proceed further -> back-track
            else
            {
                //There was a failure finding a valid circuit over the graph
                if (path.TriedAllCases)
                {
                    //path.SetLongestPathToPath();
                    foundValidCircuit = false;
                    break;
                }

                path.BackPathOneStep();
            }

            //Time to try and close
            if (path.HypothecialCircuitLength > preferredLength && path.HypothecialCircuitLength <= maxLength)
            {
                //Closing the circuit now would not be valid
                if (!path.DoesHypotheticalCircuitIntersect(out int amount) && !path.IsHypotheticalCircuitStraightTooLong && !path.IsHypotheticalCircuitStraightTooShort && path.ValidClosingAngle)
                    break;
            }
            //Circuit is too long
            else if (path.HypothecialCircuitLength > maxLength)
                path.BackCompletely(minLength);
        }

        if (!foundValidCircuit)
        {
            if (recursionCounter++ > 5)
            {
                circuitInformation.isValid = false;
                return new List<Vector2>();
            }

            return GenerateRandomCircuit(voronoiGraph, minLength, maxLength, maxStraightLength, minStraightLength, minLengthStartGrid, minLengthFromFinishLine, minNodeLength, minTurnAngle, ref recursionCounter, ref circuitInformation);
        }

        circuitInformation = SetCircuitInformation(path, preferredLength);

        path.InsertFinishLine();
        path.ConnectCircuit();

        return path.PathInPoints;
    }

    private static CircuitInformation SetCircuitInformation(VoronoiPath path, float preferredLength)
    {
        return new CircuitInformation(path.HypothecialCircuitLength, path.ClosingStraightLength, path.LongestStraight, path.PathInPoints.Count - 1, preferredLength, path.IsClockWise, true);
    }
}
