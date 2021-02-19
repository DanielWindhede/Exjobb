using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Delaunay Triangulation Settings")]

    [SerializeField] private int _pointAmount;
    [SerializeField] private Vector2 _bounds;

    [Header("Settings")]

    [SerializeField] int _seed;
    [SerializeField] bool _useManualPoints = true;
    [SerializeField] List<Vector2> _manualPoints;
    [SerializeField] bool _autoCurve = true;
    [SerializeField] float _minCurve = 0.0f;
    [SerializeField] float _maxCurve = 2.0f;
    [SerializeField] float _curvatureScale = 0.035f;

    DisplayCurve _displayCurveScript;
    DisplayDelaunayTriangulation _displayDelaunayTriangulation;

    private void Awake()
    {
        _displayCurveScript = GetComponentInChildren<DisplayCurve>();
        _displayDelaunayTriangulation = GetComponentInChildren<DisplayDelaunayTriangulation>();
    }

    private void Start()
    {
        Random.InitState(_seed);

        List<Triangle> triangulation = DelaunayTriangulationGenerator.GenerateDelaunayTriangulatedGraph(_pointAmount, _bounds);
        _displayDelaunayTriangulation.Display(triangulation, _bounds);

        //if (_useManualPoints)
        //{
        //    CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(_manualPoints, _curvatureScale, _minCurve, _maxCurve, _autoCurve);
        //    _displayCurveScript.SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints));
        //}      
    }
}
