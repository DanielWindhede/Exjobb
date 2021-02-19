using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DelaunayTriangulationGenerator : MonoBehaviour
{
    private static List<Triangle> tempTriangles = new List<Triangle>();

    public static List<Triangle> GenerateDelaunayTriangulatedGraph(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = GeneratePoints(pointAmount, bounds);

        Triangle superTriangle = GenerateSuperTriangle(bounds);
        List<Triangle> triangulation = BowyerWatson(new List<Triangle> { superTriangle }, points, bounds);

        // Removes super triangle
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];
            if (triangle.SharesAnyVertex(superTriangle))
                triangulation.RemoveAt(i);
        }

        tempTriangles = triangulation;

        return tempTriangles;
    }

    public static List<Vector2> GeneratePoints(int pointAmount, Vector2 bounds)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointAmount; i++) // possibilty for duplicates
        {
            float x = UnityEngine.Random.Range(0f, bounds.x);
            float y = UnityEngine.Random.Range(0f, bounds.y);
            points.Add(new Vector2(x, y));
        }

        return points;
    }

    public static List<Triangle> BowyerWatson(List<Triangle> mesh, List<Vector2> points, Vector2 bounds)
    {
        List<Triangle> triangulation = mesh;

        // Add all points sequentially to the triangulation
        foreach (Vector2 point in points)
        {
            // Finds all triangles no longer valid after point insertion
            HashSet<Triangle> badTriangles = new HashSet<Triangle>();
            foreach (Triangle triangle in triangulation)
            {
                if (triangle.IsPointWithinCircumference(point))
                    badTriangles.Add(triangle);
            }

            // Handle bad triangles
            List<Edge> edges = new List<Edge>();
            foreach (Triangle triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.A, triangle.B));
                edges.Add(new Edge(triangle.B, triangle.C));
                edges.Add(new Edge(triangle.C, triangle.A));
            }   
            
            foreach (Triangle triangle in badTriangles)
                triangulation.Remove(triangle);


            // all edges around triangulation
            HashSet<Edge> boundry = new HashSet<Edge>();
            // Gets edges without duplicates
            boundry = new HashSet<Edge>(edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(x => x.Key));

            // Creates new triangles connecting to added point
            foreach (Edge edge in boundry)
            {
                // forms a triangle from edge to point
                Triangle newTriangle = new Triangle(edge.Point1, edge.Point2, point);
                triangulation.Add(newTriangle);
            }
        }

        return triangulation;
    }

    /// <summary>
    /// Generate a right triangle which includes innerbounds fully
    /// </summary>
    /// <param name="innerBounds">Rectangle that must be included</param>
    /// <param name="angle">Aesthetic, leave blank if unsure (0 - 90)</param>
    public static Triangle GenerateSuperTriangle(Vector2 innerBounds, float angle = 45)
    {
        angle = Mathf.Clamp(angle, 0f, 90f);

        float angleA = angle;
        float angleB = 90 - angle;
        Vector2 pointA = new Vector2(0, innerBounds.y + innerBounds.x * Mathf.Tan(angleA * Mathf.Deg2Rad));
        Vector2 pointB = Vector2.zero;
        Vector2 pointC = new Vector2(innerBounds.x + innerBounds.y * Mathf.Tan(angleB * Mathf.Deg2Rad), 0);
        return new Triangle(pointA, pointB, pointC);
    }
}
























public class MeshGenerator
{
    public static Mesh GenerateMesh(List<Vector2> values)
    {
        int count = values.Count;

        if (count > 0)
        {
            int[] _triangles;
            Vector3[] _vertices;
            Vector3[] _fullVertices;
            Vector2[] _uv;
            Mesh _mesh = new Mesh();

            _vertices = new Vector3[count + 1];
            _uv = new Vector2[count + 1];
            _triangles = new int[3 * count];

            // uv
            _uv[0] = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                _uv[i + 1] = Vector2.one;
            }

            // vertices and tris
            _vertices[0] = Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                _vertices[i + 1] = values[i];

                // Tris
                _triangles[i * 3] = 0;
                for (int j = 0; j < 2; j++)
                {
                    _triangles[i * 3 + j + 1] = i + j + 1;
                }
            }
            _triangles[3 * count - 1] = 1;
            _fullVertices = new Vector3[count + 1];
            Array.Copy(_vertices, _fullVertices, _vertices.Length);

            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;

            return _mesh;
        }
        return null;
    }
}