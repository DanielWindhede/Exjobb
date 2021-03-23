using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class Controller : MonoBehaviour
{
    public int Seed
    {
        get { return _seed; }
        set { _seed = value; }
    }

    public CircuitInformation CircuitInformation
    {
        get { return _circuitInformation; }
    }

    [Header("Main Settings")]

    [SerializeField] int _amountOfTestRuns;
    [SerializeField] int _seed;
    [SerializeField] bool _centerPath = true;
    [SerializeField] Vector2 _centerPoint = Vector2.zero;

    [Header("Delaunay Triangulation Settings")]

    [SerializeField] float _complexityRatio = 8.33f;

    [Header("Circuit Settings")]

    [SerializeField] float _minCircuitLength = 3.5f;
    [SerializeField] float _maxCircuitLength = 7.0f;
    [SerializeField] float _maxStraightLength = 2.0f;
    [SerializeField] float _minStraightLength = 0.458f;
    [SerializeField] float _minLengthFromFinishLine = 0.25f;
    [SerializeField] float _minLengthStartGrid = 0.208f;
    [SerializeField] float _minNodeLength = 0.05f;
    [SerializeField] float _minTurnAngle = 10f;

    [Header("Curve Settings")]

    [SerializeField] bool _autoCurve = true;
    [SerializeField] float _minCurve = 0.0f;
    [SerializeField] float _maxCurve = 2.0f;
    [SerializeField] float _maxControlPointLength = 1.0f;
    [SerializeField] float _autoCurveWeigth = 0.25f;

    [Header("Manual Settings")]

    [SerializeField] bool _useManualCircuit;
    [SerializeField] CircuitScriptableObject _manualCircuit;


    private DisplayCurve _displayCurveScript;
    private DisplayDelaunayTriangulation _displayDelaunayTriangulation;
    private DisplayVoronoiGraph _displayGraph;
    private DisplayPathing _displayPathing;

    private Vector2 _bounds = new Vector2(3, 3);
    private CircuitInformation _circuitInformation;

    private void Awake()
    {
        _displayCurveScript = GetComponentInChildren<DisplayCurve>();
        _displayDelaunayTriangulation = GetComponentInChildren<DisplayDelaunayTriangulation>();
        _displayGraph = GetComponentInChildren<DisplayVoronoiGraph>();
        _displayPathing = GetComponentInChildren<DisplayPathing>();
    }

    private void Start()
    {
        Refresh();
    }

    /// <summary>
    /// Calculate amount of points to place dependant on complexity ratio and bounds (should be static bounds)
    /// </summary>
    /// <returns></returns>
    private int GetPointAmount()
    {
        return Mathf.RoundToInt(_complexityRatio * _bounds.x * _bounds.y);
    }

    public void Refresh(bool onlyData = false)
    {
        Random.InitState(_seed);

        int pointAmount = GetPointAmount();

        List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(pointAmount, _bounds);

        if (!onlyData)
            _displayDelaunayTriangulation.Display(triangulation, _bounds);
        VoronoiGraph voronoiGraph = VoronoiDiagramGenerator.GenerateVoronoiFromDelaunay(triangulation, _bounds);

        if (!onlyData)
            _displayGraph.Display(voronoiGraph);

        int recursionCounter = 0;
        List<Vector2> path = Pathing.GenerateRandomCircuit(voronoiGraph, _minCircuitLength, _maxCircuitLength, _maxStraightLength, _minStraightLength, _minLengthStartGrid, _minLengthFromFinishLine, _minNodeLength, _minTurnAngle, ref recursionCounter, ref _circuitInformation);

        if (!onlyData && _circuitInformation.isValid)
        {
            _displayPathing.Display(path);

            if (_centerPath)
                CenterPath(_centerPoint, ref path);

            CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(path, _minCurve, _maxCurve, _autoCurveWeigth, _autoCurve);

            curvaturePoints = _useManualCircuit ? _manualCircuit._circuitPoints : curvaturePoints;

            _displayCurveScript.SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints, _maxControlPointLength));
        }          
    }

    public void TestRuns()
    {
        int failedRuns = 0;

        float longestLongestStraight = float.MinValue;
        float shortestLongestStraight = float.MaxValue;
        float longestCircuit = float.MinValue;
        float shortestCircuit = float.MaxValue;
        int maxTurns = int.MinValue;
        int minTurns = int.MaxValue;

        double totalCircuitLength = 0;
        double closingStraightLength = 0;
        double longestStraigthLength = 0;
        long turnAmount = 0;
        double preferredCircuitLength = 0;
        long amountClockwise = 0;

        for (int i = 0; i < _amountOfTestRuns; i++)
        {
            Refresh(true);

            if (_circuitInformation.isValid)
            {
                if (_circuitInformation.longestStraightLength > longestLongestStraight)
                    longestLongestStraight = _circuitInformation.longestStraightLength;
                if (_circuitInformation.longestStraightLength < shortestLongestStraight)
                    shortestLongestStraight = _circuitInformation.longestStraightLength;
                if (_circuitInformation.circuitLength > longestCircuit)
                    longestCircuit = _circuitInformation.circuitLength;
                if (_circuitInformation.circuitLength < shortestCircuit)
                    shortestCircuit = _circuitInformation.circuitLength;
                if (_circuitInformation.turnAmount > maxTurns)
                    maxTurns = _circuitInformation.turnAmount;
                if (_circuitInformation.turnAmount < minTurns)
                    minTurns = _circuitInformation.turnAmount;

                totalCircuitLength += _circuitInformation.circuitLength;
                closingStraightLength += _circuitInformation.closingStraightLength;
                longestStraigthLength += _circuitInformation.longestStraightLength;
                turnAmount += _circuitInformation.turnAmount;
                preferredCircuitLength += _circuitInformation.preferredCircuitLength;
                if (_circuitInformation.clockWise)
                    amountClockwise++;
            }
            else
                failedRuns++;

            _seed = Random.Range(int.MinValue, int.MaxValue);
        }

        int succededRuns = _amountOfTestRuns - failedRuns;

        float averageCircuitLength = (float)totalCircuitLength / succededRuns;
        float averageClosingStraigthLength = (float)closingStraightLength / succededRuns;
        float averageLongestStraigthLength = (float)longestStraigthLength / succededRuns;
        int averageTurnAmount = Mathf.RoundToInt((float)turnAmount / succededRuns);
        float averagePreferredCircuitLength = (float)preferredCircuitLength / succededRuns;
        float percentageClockwise = ((float)amountClockwise / succededRuns) * 100;

        string filePath = Application.persistentDataPath;
        string filename = "/" + _amountOfTestRuns + "_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
        filePath += filename;
        File.WriteAllText(filePath, string.Empty);

        StreamWriter writer = new StreamWriter(filePath, true);

        writer.WriteLine("For: " + _amountOfTestRuns + " iterations with parameters: ");
        writer.WriteLine("Completed " + succededRuns + " succeeded runs!");
        writer.WriteLine("Complexity ratio: " + _complexityRatio + " points/km2");
        writer.WriteLine("Min Curve: " + _minCurve + ", Max Curve: " + _maxCurve);
        writer.WriteLine("Control point length: " + _maxControlPointLength);
        writer.WriteLine("===================");
        writer.WriteLine("Longest circuit Straight: " + longestLongestStraight + " km");
        writer.WriteLine("Shortest longest circuit Straight: " + shortestLongestStraight + " km");
        writer.WriteLine("Longest circuit Length: " + longestCircuit + " km");
        writer.WriteLine("Shortest circuit Length: " + shortestCircuit + " km");
        writer.WriteLine("Circuit with most turns: " + maxTurns + " st");
        writer.WriteLine("Circuit with least turns: " + minTurns + " st");
        writer.WriteLine("===================");
        writer.WriteLine("Average Circuit Length: " + averageCircuitLength + " km");
        writer.WriteLine("Average Closing Straight Length: " + averageClosingStraigthLength + " km");
        writer.WriteLine("Average Longest Straight Length: " + averageLongestStraigthLength + " km");
        writer.WriteLine("Average Turn Amount: " + averageTurnAmount + " st");
        writer.WriteLine("Average Preferred Circuit Length: " + averagePreferredCircuitLength + " km");
        writer.WriteLine("Percentage Clockwise: " + percentageClockwise + " %");

        writer.Close();

        Debug.Log("Done!");
    }

    /// <summary>
    /// Centers the path around the centerPoint
    /// </summary>
    /// <param name="centerPoint">The center point in world space for the path</param>
    /// <param name="path">The path to center</param>
    private void CenterPath(Vector2 centerPoint, ref List<Vector2> path)
    {
        Vector2 center = Vector2.zero;
        foreach (Vector2 point in path)
            center += point;

        center /= path.Count;
        Vector2 centerOffset = center - centerPoint; 

        for (int i = 0; i < path.Count; i++)
            path[i] -= centerOffset;
    }
}