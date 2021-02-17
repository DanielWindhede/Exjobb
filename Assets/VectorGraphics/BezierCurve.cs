using Unity.VectorGraphics;
using UnityEngine;
using System.Collections.Generic;

public class BezierCurve
{
    /// <summary>
    /// Create a bezier curve from a list of points. 
    /// </summary>
    /// <param name="points">ordered set of points to create a line</param>
    /// <param name="curveAmount">How much the curve should curve</param>
    /// <returns></returns>
    public static BezierPathSegment[] ConstructBezierCurve(CurvaturePoint[] points)
    {
        BezierPathSegment[] path = new BezierPathSegment[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            //Set the actual point
            path[i].P0 = points[i].point;
            //Outgoing control point from actual point
            path[i].P1 = CalculateControlPoint(i, true, points[i].curveAmount, in points);
            //In going control point to next point
            int nextPointIndex = Utility.Modulo(i + 1, points.Length);
            path[i].P2 = CalculateControlPoint(nextPointIndex, false, points[nextPointIndex].curveAmount, in points);
        }

        return path;
    }

    /// <summary>
    /// Return direction as seen from currentPoint
    /// </summary>
    /// <param name="pointIndex">Reference middle point index</param>
    /// <param name="forward">Should the direction point along the points direction?</param>
    /// <param name="points">The points</param>
    static Vector2 CalculateControlPoint(int pointIndex, bool forward, float curveAmount, in CurvaturePoint[] points)
    {
        int prePoint = Utility.Modulo(pointIndex - 1, points.Length - 1);
        int nextPoint = Utility.Modulo(pointIndex + 1, points.Length - 1);
        //Pointing toward next point
        Vector2 direction = (points[nextPoint].point - points[prePoint].point).normalized;
        //Reverse if looking backward
        direction *= forward ? 1 : -1;

        return points[pointIndex].point + direction * curveAmount;
    }
}
