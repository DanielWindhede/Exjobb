using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class DisplayCurve : MonoBehaviour
{
    [Header("Options")]

    [SerializeField] bool _showFinishedProduct = true;
    [SerializeField, Range(0, 180)] float _minTurnAngle = 15f;
    [SerializeField, Range(0.001f, 100f)] float _size = 1.0f;
    [SerializeField] Color _color = Color.white;
    [SerializeField, Range(0.0001f, 1f)] float _thickness = 0.1f;
    [SerializeField] Transform _holder;
    [SerializeField] float _turnOffset = 0.05f;
    [SerializeField] Vector3 _arrowOffset = new Vector3(0.075f, 0.035f, 0.0f);
    [SerializeField] GameObject _turnPrefab;
    [SerializeField] GameObject _arrowPrefab;
    [SerializeField] GameObject _finishLinePrefab;

    [Header("Debug Display Options")]

    [SerializeField] bool _use = true;
    [SerializeField] bool _showPoints = true;
    [SerializeField] bool _showControlPoints = true;
    [SerializeField] bool _showBezierCurve = true;
    [SerializeField] Color _pointColor = Color.black;
    [SerializeField] Color _controlPointColor = Color.red;
    [SerializeField] Color _controlPointLineColor = Color.black;
    [SerializeField] Color _bezierCurveColor = Color.cyan;
    [SerializeField] float _pointRadius = 0.005f;
    [SerializeField] float _controlPointRadius = 0.0025f;
    [SerializeField] float _bezierCurveWidth = 0.005f;

    [Header("TessellationOptions")]

    [SerializeField] float _stepDistance = 1000f;
    [SerializeField] float _maxCordDeviation = 0.05f;
    [SerializeField] float _maxTanAngleDeviation = 0.05f;
    [SerializeField] float _samplingStepSize = 0.01f;

    Scene _scene;
    Shape _path;
    VectorUtils.TessellationOptions _options;
    Mesh _mesh;

    BezierPathSegment[] _points;

    private void OnEnable()
    {
        SetupOptions();
        SetupMesh();
    }

    /// <summary>
    /// Sets the values for a bezier curve and display it
    /// </summary>
    /// <param name="points">Line segments</param>
    public void SetupCurve(BezierPathSegment[] points)
    {
        ClearSpawnables();
        _points = points;

        if (_showFinishedProduct)
        {
            _path = new Shape()
            {
                Contours = new BezierContour[] { new BezierContour() { Segments = points } },
                PathProps = new PathProperties() { Stroke = new Stroke() { Color = _color, HalfThickness = _thickness } }
            };

            _scene = new Scene() { Root = new SceneNode() { Shapes = new List<Shape> { _path } } };

            SetupSpawnables();
            Display();
        } 
    }

    #region Setup

    /// <summary>
    /// Setup the TessellationOptions that will persist throughout the playtime
    /// </summary>
    private void SetupOptions()
    {
        _options = new VectorUtils.TessellationOptions()
        {
            StepDistance = _stepDistance,
            MaxCordDeviation = _maxCordDeviation,
            MaxTanAngleDeviation = _maxTanAngleDeviation,
            SamplingStepSize = _samplingStepSize
        };
    }

    /// <summary>
    /// Create new mesh that will display line and apply it to meshFilter
    /// </summary>
    private void SetupMesh()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    #endregion

    private void ClearSpawnables()
    {
        for (int i = _holder.childCount - 1; i >= 0; i--)
            Destroy(_holder.GetChild(i).gameObject);
    }

    /// <summary>
    /// Add visuals for turns, arrow and finish line
    /// </summary>
    private void SetupSpawnables()
    {
        BezierPathSegment[] allTurns = _path.Contours[0].Segments;
        //Turns
        SetupTurns(in allTurns);

        //Arrow
        GameObject arrow = Instantiate(_arrowPrefab, allTurns[0].P0, Quaternion.identity, _holder) as GameObject;
        arrow.transform.right = allTurns[1].P0 - allTurns[0].P0;
        arrow.transform.Translate(_arrowOffset);

        //Finish line
        GameObject finishLine = Instantiate(_finishLinePrefab, allTurns[0].P0, Quaternion.identity, _holder) as GameObject;
        finishLine.transform.up = allTurns[1].P0 - allTurns[0].P0;
    }

    private void SetupTurns(in BezierPathSegment[] allTurns)
    {
        //Skip first and second one since that is the same as last one and 2nd is start line 
        for (int i = 1; i < allTurns.Length; i++)
        {
            Vector2 prePoint = allTurns[Utility.Modulo(i - 1, allTurns.Length - 1)].P0;
            Vector2 nextPoint = allTurns[Utility.Modulo(i + 1, allTurns.Length - 1)].P0;
            Vector2 preVector = allTurns[i].P0 - prePoint;
            Vector2 nextVector = allTurns[i].P0 - nextPoint;

            //Skip corners that are barely turning
            if (Vector2.Angle(-preVector, nextVector) >= _minTurnAngle)
            {
                Vector2 spawnPosition = allTurns[i].P0 + (preVector + nextVector).normalized * _turnOffset;
                GameObject turnObj = Instantiate(_turnPrefab, spawnPosition, Quaternion.identity, _holder) as GameObject;
                turnObj.GetComponent<TurnNumber>().SetNumber(i);
            }          
        }
    }

    private void Display()
    {
        List<VectorUtils.Geometry> geometries = VectorUtils.TessellateScene(_scene, _options);
        VectorUtils.FillMesh(_mesh, geometries, _size);
    }

    private void DisplayPoints()
    {
        UnityEditor.Handles.color = _pointColor;

        foreach (BezierPathSegment segment in _points)
            UnityEditor.Handles.DrawSolidDisc(segment.P0, Vector3.back, _pointRadius);
    }

    private void DisplayControlPoints()
    {
        for (int i = 0; i < _points.Length; i++)
        {
            UnityEditor.Handles.color = _controlPointColor;

            UnityEditor.Handles.DrawSolidDisc(_points[i].P1, Vector3.back, _controlPointRadius);
            UnityEditor.Handles.DrawSolidDisc(_points[i].P2, Vector3.back, _controlPointRadius);

            Gizmos.color = _controlPointLineColor;

            Gizmos.DrawLine(_points[i].P0, _points[i].P1);
            if (i + 1 < _points.Length)
                Gizmos.DrawLine(_points[i].P2, _points[i + 1].P0);
        }
    }

    private void DisplayBezierCurve()
    {
        for (int i = 0; i < _points.Length - 1; i++)
            UnityEditor.Handles.DrawBezier(_points[i].P0, _points[i + 1].P0, _points[i].P1, _points[i].P2, _bezierCurveColor, null, _bezierCurveWidth);
    }

    private void OnDrawGizmos()
    {
        if (_points != null && _use)
        {
            if (_showPoints)
                DisplayPoints();
            if (_showControlPoints)
                DisplayControlPoints();
            if (_showBezierCurve)
                DisplayBezierCurve();
        }
    }
}
