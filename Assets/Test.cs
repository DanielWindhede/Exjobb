using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Transform[] controlPoints;

    Scene _scene;
    Shape _path;
    VectorUtils.TessellationOptions _options;
    Mesh _mesh;

    private void OnEnable()
    {
        _path = new Shape()
        {
            Contours = new BezierContour[] { new BezierContour() { Segments = new BezierPathSegment[2] } },
            PathProps = new PathProperties()
            {
                Stroke = new Stroke() { Color = Color.white, HalfThickness = 0.1f }
            }
        };

        _scene = new Scene()
        {
            Root = new SceneNode() { Shapes = new List<Shape> { _path } }
        };

        _options = new VectorUtils.TessellationOptions()
        {
            StepDistance = 1000.0f,
            MaxCordDeviation = 0.05f,
            MaxTanAngleDeviation = 0.05f,
            SamplingStepSize = 0.01f
        };

        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void Update()
    {
        if (_scene == null)
            OnEnable();

        _path.Contours[0].Segments[0].P0 = (Vector2)controlPoints[0].localPosition;
        _path.Contours[0].Segments[0].P1 = (Vector2)controlPoints[1].localPosition;
        _path.Contours[0].Segments[0].P2 = (Vector2)controlPoints[2].localPosition;
        _path.Contours[0].Segments[1].P0 = (Vector2)controlPoints[3].localPosition;

        List<VectorUtils.Geometry> geometries = VectorUtils.TessellateScene(_scene, _options);
        VectorUtils.FillMesh(_mesh, geometries, 1.0f);
    }
}
