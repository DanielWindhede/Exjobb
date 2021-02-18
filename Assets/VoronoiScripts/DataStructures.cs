using UnityEngine;

public struct Triangle
{
    public Vector2[] vertices;

    public Vector2 A { get { return vertices[0]; } }
    public Vector2 B { get { return vertices[1]; } }
    public Vector2 C { get { return vertices[2]; } }

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        this.vertices = new Vector2[]
        {
                a, b, c
        };
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
