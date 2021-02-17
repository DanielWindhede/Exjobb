using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ManualTrack : MonoBehaviour
{
    [SerializeField] CurvaturePoint[] _curvaturePoints;

    DisplayCurve _displayCurveScript;

    private void OnEnable()
    {
        _displayCurveScript = GetComponent<DisplayCurve>();
    }

    private void OnValidate()
    {
        _displayCurveScript.SetupCurve(BezierCurve.ConstructBezierCurve(_curvaturePoints));
    }
}
