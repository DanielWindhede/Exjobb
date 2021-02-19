using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] int _seed;
    [SerializeField] bool _useManualPoints = true;
    [SerializeField] List<Vector2> _manualPoints;
    [SerializeField] bool _autoCurve = true;
    [SerializeField] float _minCurve = 0.0f;
    [SerializeField] float _maxCurve = 2.0f;
    [SerializeField] float _curvatureScale = 0.035f;

    DisplayCurve _displayCurveScript;

    private void Awake()
    {
        _displayCurveScript = GetComponentInChildren<DisplayCurve>();
    }

    private void Start()
    {
        Random.InitState(_seed);

        if (_useManualPoints)
        {
            CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(_manualPoints, _curvatureScale, _minCurve, _maxCurve, _autoCurve);
            _displayCurveScript.SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints));
        }      
    }
}
