using UnityEngine;

public class Triangle
{
    public Vector2[] vertices;

    public Vector2 A { get { return vertices[0]; } }
    public Vector2 B { get { return vertices[1]; } }
    public Vector2 C { get { return vertices[2]; } }

    public Vector2 Center { get; private set; }
    public float Radius { get; private set; }
    public bool IsPointWithinCircumference(Vector2 point)
    {
        return (point - Center).sqrMagnitude < Radius * Radius;
    }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        this.vertices = new Vector2[]
        {
            a, b, c
        };

        // Can be optimized:)
        float alpha = ((B - C).sqrMagnitude * Vector3.Dot(A - B, A - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
        float beta = ((A - C).sqrMagnitude * Vector3.Dot(B - A, B - C)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);
        float gamma = ((A - B).sqrMagnitude * Vector3.Dot(C - A, C - B)) / (2f * Vector3.Cross(A - B, B - C).sqrMagnitude);

        Center = alpha * A + beta * B + gamma * C;

        Radius = ((A - B).magnitude * (B - C).magnitude * (C - A).magnitude) / (2f * Vector3.Cross(A - B, B - C).magnitude);
    }

    public bool SharesAnyVertex(Triangle triangle)
    {
        foreach (Vector2 vertex in vertices)
        {
            for (int i = 0; i < triangle.vertices.Length; i++)
            {
                if (vertex == triangle.vertices[i])
                    return true;
            }
        }
        return false;
    }
}

public class Edge
{
    public Vector2 Point1 { get; }
    public Vector2 Point2 { get; }

    public Edge(Vector2 point1, Vector2 point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (obj.GetType() != GetType())
            return false;
        Edge edge = obj as Edge;

        bool samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
        bool samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
        return samePoints || samePointsReversed;
    }

    public override int GetHashCode()
    {
        int hCode = (int)Point1.x ^ (int)Point1.y ^ (int)Point2.x ^ (int)Point2.y;
        return hCode.GetHashCode();
    }
}
