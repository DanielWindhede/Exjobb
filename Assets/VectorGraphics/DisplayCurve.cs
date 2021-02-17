using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

[ExecuteInEditMode]
public class DisplayCurve : MonoBehaviour
{
    [Header("TEST")]

    [SerializeField] List<Vector2> _testPoints;
    [SerializeField] bool _autoCurve = true;
    [SerializeField] float _minCurve = 0.0f;
    [SerializeField] float _maxCurve = 2.0f;

    [Header("Options")]

    [SerializeField] Color _color = Color.white;
    [SerializeField, Range(0.01f, 1f)] float _thickness = 0.1f;
    [SerializeField] float _test = 1.0f;

    [Header("TessellationOptions")]

    [SerializeField] float _stepDistance = 1000f;
    [SerializeField] float _maxCordDeviation = 0.05f;
    [SerializeField] float _maxTanAngleDeviation = 0.05f;
    [SerializeField] float _samplingStepSize = 0.01f;

    Scene _scene;
    Shape _path;
    VectorUtils.TessellationOptions _options;
    Mesh _mesh;

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
        _path = new Shape()
        {
            Contours = new BezierContour[] { new BezierContour() { Segments = points } },
            PathProps = new PathProperties() { Stroke = new Stroke() { Color = _color, HalfThickness = _thickness } }
        };

        _scene = new Scene() { Root = new SceneNode() { Shapes = new List<Shape> { _path } } };
    }

    #region Setup

    /// <summary>
    /// Setup the TessellationOptions that will persist throughout the playtime
    /// </summary>
    void SetupOptions()
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
    void SetupMesh()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    #endregion

    private void Update()
    {
        CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(_testPoints, _minCurve, _maxCurve, _autoCurve);
        SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints));

        if (_path != null)
            Display();    
    }

    void Display()
    {
        List<VectorUtils.Geometry> geometries = VectorUtils.TessellateScene(_scene, _options);
        VectorUtils.FillMesh(_mesh, geometries, 1.0f);
    }
}
