using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class DisplayCurve : MonoBehaviour
{
    [Header("Options")]

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

        SetupSpawnables();
        Display();
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

    /// <summary>
    /// Add visuals for turns, arrow and finish line
    /// </summary>
    void SetupSpawnables()
    {
        BezierPathSegment[] allTurns = _path.Contours[0].Segments;
        //Turns
        SetupTurns(in allTurns);

        //Arrow
        GameObject arrow = Instantiate(_arrowPrefab, allTurns[1].P0, Quaternion.identity, _holder) as GameObject;
        arrow.transform.right = allTurns[2].P0 - allTurns[1].P0;
        arrow.transform.Translate(_arrowOffset);

        //Finish line
        GameObject finishLine = Instantiate(_finishLinePrefab, allTurns[1].P0, Quaternion.identity, _holder) as GameObject;
        finishLine.transform.up = allTurns[2].P0 - allTurns[1].P0;
    }

    void SetupTurns(in BezierPathSegment[] allTurns)
    {
        //Skip first and second one since that is the same as last one and 2nd is start line 
        for (int i = 2; i < allTurns.Length; i++)
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
                turnObj.GetComponent<TurnNumber>().SetNumber(i - 1);
            }          
        }
    }

    void Display()
    {
        List<VectorUtils.Geometry> geometries = VectorUtils.TessellateScene(_scene, _options);
        VectorUtils.FillMesh(_mesh, geometries, _size);
    }
}
