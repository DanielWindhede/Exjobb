﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    [SerializeField] int _seed;
    [SerializeField] bool _centerPath = true;
    [SerializeField] Vector2 _centerPoint = Vector2.zero;

    [Header("Delaunay Triangulation Settings")]

    [SerializeField] private int _pointAmount;
    [SerializeField] private Vector2 _bounds;

    [Header("Circuit Settings")]

    [SerializeField] float _minCircuitLength = 3.5f;
    [SerializeField] float _maxCircuitLength = 7.0f;
    [SerializeField] float _maxStraightLength = 2.0f;
    [SerializeField] float _minStraightLength = 0.458f;
    [SerializeField] float _minLengthFromFinishLine = 0.25f;
    [SerializeField] float _minLengthStartGrid = 0.208f;
    [SerializeField] float _minNodeLength = 0.05f;

    [Header("Curve Settings")]

    [SerializeField] bool _autoCurve = true;
    [SerializeField] float _minCurve = 0.0f;
    [SerializeField] float _maxCurve = 2.0f;
    [SerializeField] float _maxControlPointLength = 1.0f;
    [SerializeField] float _autoCurveWeigth = 0.25f;
    [SerializeField] float _curvatureScale = 0.035f;

    [Header("Manual Settings")]

    [SerializeField] bool _useManualCircuit;
    [SerializeField] CircuitScriptableObject _manualCircuit;


    private DisplayCurve _displayCurveScript;
    private DisplayDelaunayTriangulation _displayDelaunayTriangulation;
    private DisplayVoronoiGraph _displayGraph;
    private DisplayPathing _displayPathing;

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

    public void Refresh()
    {
        Random.InitState(_seed);

        List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(_pointAmount, _bounds);
        _displayDelaunayTriangulation.Display(triangulation, _bounds);
        VoronoiGraph voronoiGraph = VoronoiDiagramGenerator.GenerateVoronoiFromDelaunay(triangulation, _bounds);
        _displayGraph.Display(voronoiGraph);

        int recursionCounter = 0;
        List<Vector2> path = Pathing.GenerateRandomCircuit(voronoiGraph, _minCircuitLength, _maxCircuitLength, _maxStraightLength, _minStraightLength, _minLengthStartGrid, _minLengthFromFinishLine, _minNodeLength, ref recursionCounter, ref _circuitInformation);
        _displayPathing.Display(path);

        if (_centerPath)
            CenterPath(_centerPoint, ref path);

        CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(path, _minCurve, _maxCurve, _autoCurveWeigth, _autoCurve);

        curvaturePoints = _useManualCircuit ? _manualCircuit._circuitPoints : curvaturePoints;

        _displayCurveScript.SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints, _maxControlPointLength));
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