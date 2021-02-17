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
    public static BezierPathSegment[] ConstructBezierCurve(List<Vector2> points, float curveAmount = 0)
    {
        BezierPathSegment[] path = new BezierPathSegment[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            //Set the actual point
            path[i].P0 = points[i];
            //Outgoing control point from actual point
            path[i].P1 = CalculateControlPoint(i, true, curveAmount, in points);
            path[i].P2 = CalculateControlPoint(Modulo(i + 1, points.Count), false, curveAmount, in points);
        }

        return path;
    }

    /// <summary>
    /// Return direction as seen from currentPoint
    /// </summary>
    /// <param name="pointIndex">Reference middle point index</param>
    /// <param name="forward">Should the direction point along the points direction?</param>
    /// <param name="points">The points</param>
    static Vector2 CalculateControlPoint(int pointIndex, bool forward, float curveAmount, in List<Vector2> points)
    {
        int prePoint = Modulo(pointIndex - 1, points.Count - 1);
        int nextPoint = Modulo(pointIndex + 1, points.Count - 1);
        //Pointing toward next point
        Vector2 direction = (points[nextPoint] - points[prePoint]).normalized;
        //Reverse if looking backward
        direction *= forward ? 1 : -1;

        return points[pointIndex] + direction * curveAmount;
    }

    static int Modulo(int x, int m)
    {
        return (x % m + m) % m;
    }
}
