﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input point for creating a bezierCurve later
/// </summary>
[System.Serializable]
public struct CurvaturePoint
{
    public Vector2 point;
    public float curveAmount;

    public CurvaturePoint(Vector2 point, float curveAmount)
    {
        this.point = point;
        this.curveAmount = curveAmount;
    }
}

public class Curvature
{
    public static float ANTI_ARTEFACT_EPSILON = 0.02f;

    /// <summary>
    /// Generate a set of CurvaturePoint that has point position and curvature for that point
    /// </summary>
    /// <param name="graphPoints">List of points forming a graph</param>
    public static CurvaturePoint[] GenerateCurvaturePointSet(List<Vector2> graphPoints, float minCurvature, float maxCurvature, float autoCurveWeight, bool autoCurveSharpCorners = true)
    {
        CurvaturePoint[] curvaturePoints = new CurvaturePoint[graphPoints.Count];
        for (int i = 0; i < graphPoints.Count; i++)
            curvaturePoints[i] = new CurvaturePoint(graphPoints[i], CalculateCurvature(minCurvature, maxCurvature, autoCurveWeight, i, autoCurveSharpCorners, in graphPoints));

        return curvaturePoints;
    }

    /// <summary>
    /// Returns a random value between min and max
    /// </summary>
    static float CalculateCurvature(float min, float max, float autoCurveWeight, int pointIndex, bool autoCurveSharpCorners, in List<Vector2> points)
    {
        Vector2 entryVector = (points[Utility.Modulo(pointIndex - 1, points.Count - 1)] - points[pointIndex]).normalized;
        Vector2 exitVector = (points[Utility.Modulo(pointIndex + 1, points.Count - 1)] - points[pointIndex]).normalized;

        float dotValue = 0f;
        if (autoCurveSharpCorners)
        {
            dotValue = Vector2.Dot(entryVector, exitVector);
            //No negative values
            dotValue = Mathf.Clamp(dotValue, 0, 1);
        }

        // OLD WAY
        //return (Random.Range(min, max) + dotValue) * 0.1f;

        return Mathf.Clamp(Random.Range(min, max) + ANTI_ARTEFACT_EPSILON + dotValue * autoCurveWeight, 0.0f, 1.0f);
    }
}
