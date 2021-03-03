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
    public static BezierPathSegment[] ConstructBezierCurve(CurvaturePoint[] points, float maxControlPointLength)
    {
        BezierPathSegment[] path = new BezierPathSegment[points.Length];

        //Construct first iteration path with curvature
        for (int i = 0; i < points.Length; i++)
        {
            //Set the actual point
            path[i].P0 = points[i].point;
            //Outgoing control point from actual point
            path[i].P1 = CalculateControlPoint(i, true, points[i].curveAmount, maxControlPointLength, points);
            //In going control point to next point
            int nextPointIndex = Utility.Modulo(i + 1, points.Length - 1);
            path[i].P2 = CalculateControlPoint(nextPointIndex, false, points[nextPointIndex].curveAmount, maxControlPointLength, points);
        }

        return path;
    }

    /// <summary>
    /// Return direction as seen from currentPoint
    /// </summary>
    /// <param name="pointIndex">Reference middle point index</param>
    /// <param name="forward">Should the direction point along the points direction?</param>
    /// <param name="points">The points</param>
    static Vector2 CalculateControlPoint(int pointIndex, bool forward, float curveAmount, float maxControlPointLength, CurvaturePoint[] points)
    {
        int prePoint = Utility.Modulo(pointIndex - 1, points.Length - 1);
        int nextPoint = Utility.Modulo(pointIndex + 1, points.Length - 1);


        /* OLD WAY
        //Pointing toward next point
        Vector2 direction = (points[nextPoint].point - points[prePoint].point).normalized;
        //Reverse if looking backward
        direction *= forward ? 1 : -1;
        Vector2 controlPointPosition = points[pointIndex].point + direction * curveAmount;

        return controlPointPosition;
        */





        Vector2 toPre = points[prePoint].point - points[pointIndex].point;
        Vector2 toNext = points[nextPoint].point - points[pointIndex].point;

        Vector2 controlPointDirection = toNext.normalized - toPre.normalized;
        controlPointDirection *= forward ? 1 : -1;

        Vector2 controlPointOffset = controlPointDirection.normalized * (forward ? toNext.magnitude / 2f : toPre.magnitude / 2f);
        controlPointOffset *= points[pointIndex].curveAmount;

        //Clamp to maxLength if too long
        if (controlPointOffset.magnitude > maxControlPointLength)
            controlPointOffset = controlPointOffset.normalized * maxControlPointLength;

        return points[pointIndex].point + controlPointOffset;
    }
}


/*
        Vector2 controlPointPosition = points[pointIndex].point + direction * curveAmount;
        Vector2 toControlPoint = controlPointPosition - points[pointIndex].point;
        Vector2 toNextPoint = points[nextPoint].point - points[pointIndex].point;
        //Reverse if looking backward
        toNextPoint *= forward ? 1 : -1;
        Vector2 projection = Vector3.Project(toControlPoint, toNextPoint);
        //Projection moves along more than half of distance between points
        if (projection.magnitude >= toNextPoint.magnitude / 2)
        {
            projection = projection.normalized * toNextPoint.magnitude / 2;
            toControlPoint = Vector3.Project(projection, toControlPoint);
            controlPointPosition = points[pointIndex].point + toControlPoint;
            points[pointIndex].curveAmount = toControlPoint.magnitude;

            //Already did previous point P2 wrong! Back one and redo!
            if (forward)
                redoLast = true;
        }
*/




//Vector2 controlPointPosition = points[pointIndex].point + direction * curveAmount;
//Vector2 toControlPoint = controlPointPosition - points[pointIndex].point;
//Vector2 toNextPoint = points[nextPoint].point - points[pointIndex].point;
////Reverse if looking backward
//toNextPoint *= forward? 1 : -1;

//        if (toControlPoint.magnitude > toNextPoint.magnitude)
//        {
//            toControlPoint = toControlPoint.normalized* toNextPoint.magnitude;
//controlPointPosition = points[pointIndex].point + toControlPoint;
//            points[pointIndex].curveAmount = toControlPoint.magnitude;

//            //Redo last point P2 value
//            if (forward)
//                redoLast = true;
//        }