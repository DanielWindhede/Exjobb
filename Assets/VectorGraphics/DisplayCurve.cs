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

    [SerializeField, Range(0.001f, 100f)] float _size = 1.0f;
    [SerializeField] Color _color = Color.white;
    [SerializeField, Range(0.01f, 1f)] float _thickness = 0.1f;
    [SerializeField] Transform _holder;
    [SerializeField] Vector2 _turnOffset = new Vector2(0.025f, 0.025f);
    [SerializeField] Vector3 _arrowOffset = new Vector3(0.075f, 0.035f, 0.0f);
    [SerializeField] GameObject _turnPrefab;
    [SerializeField] GameObject _arrowPrefab;
    [SerializeField] GameObject _finishLinePrefab;

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

        //MASTER STUFF
        CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(_testPoints, _minCurve, _maxCurve, _autoCurve);
        SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints));
        //MASTER STUFF

        if (Application.isPlaying)
            SetupSpawnables();
        Display();
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
        if (!Application.isPlaying)
        {
            //MASTER STUFF
            CurvaturePoint[] curvaturePoints = Curvature.GenerateCurvaturePointSet(_testPoints, _minCurve, _maxCurve, _autoCurve);
            SetupCurve(BezierCurve.ConstructBezierCurve(curvaturePoints));
            //MASTER STUFF

            Display();
        }
    }

    /// <summary>
    /// Add visuals for turns, arrow and finish line
    /// </summary>
    void SetupSpawnables()
    {
        BezierPathSegment[] allTurns = _path.Contours[0].Segments;
        //Turns
        //Skip first and second one since that is the same as last one and 2nd is start line 
        for (int i = 2; i < allTurns.Length; i++)
        {
            GameObject turnObj = Instantiate(_turnPrefab, allTurns[i].P0 + _turnOffset, Quaternion.identity, _holder) as GameObject;
            turnObj.GetComponent<TurnNumber>().SetNumber(i - 1);
        }
        //Arrow
        GameObject arrow = Instantiate(_arrowPrefab, allTurns[1].P0, Quaternion.identity, _holder) as GameObject;
        arrow.transform.right = allTurns[2].P0 - allTurns[1].P0;
        arrow.transform.Translate(_arrowOffset);

        //Finish line
        GameObject finishLine = Instantiate(_finishLinePrefab, allTurns[1].P0, Quaternion.identity, _holder) as GameObject;
        finishLine.transform.up = allTurns[2].P0 - allTurns[1].P0;
    }

    void Display()
    {
        List<VectorUtils.Geometry> geometries = VectorUtils.TessellateScene(_scene, _options);
        VectorUtils.FillMesh(_mesh, geometries, _size);
    }
}
