using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private List<Vector2> values;
    [SerializeField] private List<Vector2> points;

    private void Start()
    {
        //GenerateDelaunayTriangulatedGraph();
    }

    public struct Triangle
    {
        public Vector2[] vertices;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.vertices = new Vector2[]
            { 
                a, b, c
            };
        }

        public bool SharesAnyVertex(Triangle tri)
        {
            foreach (Vector2 vertex in vertices)
            {
                for (int i = 0; i < tri.vertices.Length; i++)
                {
                    if (vertex == tri.vertices[i])
                        return true;
                }
            }
            return false;
        }
    }

    private List<Triangle> BowyerWatson(List<Vector2> points)
    {
        List<Vector2> v = new List<Vector2>();

        //HashSet<Vector2> p = new HashSet<Vector2>(points);
        HashSet<Vector2> p = new HashSet<Vector2>(points);
        /*
        Vector2 A, B, C, point;
        A = values[0];
        B = values[1];
        C = values[2];
        point = values[3];
        */

        List<Triangle> triangulation = new List<Triangle>();

        /*
        Triangle superTriangle = new Triangle(new Vector2(10, 20),
                                       new Vector2(0, 0),
                                       new Vector2(20, 0));
        */

        Triangle superTriangle = GenerateSuperTriangle(tempBounds, tempAngle);
        triangulation.Add(superTriangle);

        foreach (var point in points)
        {
            HashSet<Triangle> badTriangles = new HashSet<Triangle>();
            foreach (var triangle in triangulation)
            {
                Matrix4x4 matrix = GetMatrixFromTriangle(triangle, point);
                if (IsPointWithinCircumference(matrix))
                {
                    badTriangles.Add(triangle);
                }
            }

            HashSet<Edge> polygon = new HashSet<Edge>();
            List<Edge> edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.vertices[0], triangle.vertices[1]));
                edges.Add(new Edge(triangle.vertices[1], triangle.vertices[2]));
                edges.Add(new Edge(triangle.vertices[2], triangle.vertices[0]));
            }
            IEnumerable<Edge> boundry = edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(x => x.Key); // Gets edges without duplicates
            polygon = new HashSet<Edge>(boundry);

            foreach (var triangle in badTriangles)
            {
                triangulation.Remove(triangle);
            }

            foreach (var edge in polygon)
            {
                //newTri := form a triangle from edge to point
                Triangle newTri = new Triangle(edge.Point1, edge.Point2, point);
                triangulation.Add(newTri);
                //add newTri to triangulation
            }
        }
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangulation[i];
            if (triangle.SharesAnyVertex(superTriangle))
                triangulation.RemoveAt(i);
        }

        return triangulation;

        /*
        Matrix4x4 matrix = GetMatrixFromTri(A, B, C, point);
        print(IsPointWithinCircumference(matrix));
        */

        values = new List<Vector2>(v);
    }

    public List<Vector2> GeneratePoints(int pointAmount, Vector2 bounds)
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

    public Triangle GenerateSuperTriangle(Vector2 innerBounds, float angle = 45)
    {
        float angleA = angle;
        float angleB = 90 - angle;
        Vector2 pointA = new Vector2(0, innerBounds.y + innerBounds.x * Mathf.Tan(angleA * Mathf.Deg2Rad));
        Vector2 pointB = Vector2.zero;
        Vector2 pointC = new Vector2(innerBounds.x + innerBounds.y * Mathf.Tan(angleB * Mathf.Deg2Rad), 0);
        return new Triangle(pointA, pointB, pointC);
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
            var edge = obj as Edge;

            var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)Point1.x ^ (int)Point1.y ^ (int)Point2.x ^ (int)Point2.y;
            return hCode.GetHashCode();
        }
    }

    private Matrix4x4 GetMatrixFromTriangle(Triangle triangle, Vector2 point)
    {
        return GetMatrixFromTriangle(triangle.vertices[0], triangle.vertices[1], triangle.vertices[2], point);
    }
    private Matrix4x4 GetMatrixFromTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 point)
    {
        Vector4[] cols = new Vector4[4];
        Vector2[] arr = new Vector2[]
        {
            A, B, C, point
        };

        for (int i = 0; i < arr.Length; i++)
            cols[i] = GetCol(arr[i]);

        /*
        foreach (var col in cols)
        {
            print(col);
        }
        */

        return new Matrix4x4(cols[0], cols[1], cols[2], cols[3]);
    }

    private Vector4 GetCol(Vector2 val)
    {
        return new Vector4(val.x, val.y, GetM3Operand(val), 1);
    }

    private float GetM3Operand(Vector2 val)
    {
        return val.x * val.x + val.y * val.y;
    }

    private bool IsPointWithinCircumference(Matrix4x4 m)
    {
        return m.determinant > 0;
    }

    private void OnValidate()
    {
        GetComponent<MeshFilter>().mesh = MeshGenerator.GenerateMesh(values);
        if (generate)
        {
            generate = false;
            var points = GeneratePoints(tempPointAmount, tempBounds);
            g = BowyerWatson(points);
        }
    }

    List<Triangle> g = new List<Triangle>();

    public float tempAngle = 45;
    public Vector2 tempBounds;
    public int tempPointAmount = 20;
    public bool showSuperTriangle;
    public bool generate;

    private void OnDrawGizmos()
    {
        /*
        foreach (var point in points)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y, -1), 0.1f);
        }
        */
        

        foreach (var triangle in g)
        {
            Gizmos.DrawLine(triangle.vertices[0], triangle.vertices[1]);
            Gizmos.DrawLine(triangle.vertices[1], triangle.vertices[2]);
            Gizmos.DrawLine(triangle.vertices[2], triangle.vertices[0]);
        }

        if (showSuperTriangle)
        {
            Gizmos.DrawLine(Vector3.zero, Vector2.up * tempBounds.y);
            Gizmos.DrawLine(Vector3.zero, Vector2.right * tempBounds.x);
            Gizmos.DrawLine(Vector2.right * tempBounds.x, Vector2.one * tempBounds);
            Gizmos.DrawLine(Vector2.up * tempBounds.y, Vector2.one * tempBounds);

            Triangle superTriangle = GenerateSuperTriangle(tempBounds, tempAngle);
            Gizmos.DrawLine(superTriangle.vertices[0], superTriangle.vertices[1]);
            Gizmos.DrawLine(superTriangle.vertices[1], superTriangle.vertices[2]);
            Gizmos.DrawLine(superTriangle.vertices[2], superTriangle.vertices[0]);
        }
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