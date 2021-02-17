using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] private List<Vector2> values;
    [SerializeField] private List<Vector2> points;
    private MeshGenerator _renderer;
    private MeshGenerator Renderer
    {
        get
        {
            if (_renderer == null)
                _renderer = new MeshGenerator(GetComponent<MeshFilter>());
            return _renderer;
        }
    }

    private void Start()
    {
        //GenerateDelaunayTriangulatedGraph();
    }

    struct Triangle
    {
        public Vector2 a, b, c;
        
        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    private List<Triangle> BowyerWatson()
    {
        List<Vector2> v = new List<Vector2>();

        HashSet<Vector2> p = new HashSet<Vector2>(points);

        /*
        Vector2 A, B, C, point;
        A = values[0];
        B = values[1];
        C = values[2];
        point = values[3];
        */

        List<Triangle> triangulation = new List<Triangle>();

        triangulation.Add(new Triangle(new Vector2(-10, -10),
                                       new Vector2(10, -10),
                                       new Vector2(0, 10)));

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
                edges.Add(new Edge(triangle.a, triangle.b));
                edges.Add(new Edge(triangle.b, triangle.c));
                edges.Add(new Edge(triangle.c, triangle.a));
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
                //add newTri to triangulation
            }
        }
        foreach (var triangle in triangulation)
        {
            // if triangle contains a vertex from original super-triangle
            // remove triangle from triangulation
        }
        return triangulation;

        /*
        Matrix4x4 matrix = GetMatrixFromTri(A, B, C, point);
        print(IsPointWithinCircumference(matrix));
        */

        values = new List<Vector2>(v);
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
        return GetMatrixFromTriangle(triangle.a, triangle.b, triangle.c, point);
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

        foreach (var col in cols)
        {
            print(col);
        }

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
        Renderer.GenerateMesh(values);
    }

    private void OnDrawGizmos()
    {
        foreach (var point in points)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y, -1), 0.1f);
        }
    }
}

public class MeshGenerator
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector3[] _fullVertices;
    private Vector2[] _uv;
    private int[] _triangles;

    public MeshGenerator(MeshFilter meshFilter)
    {
        _meshFilter = meshFilter;
    }

    public void GenerateMesh(List<Vector2> values)
    {
        int count = values.Count;

        if (count > 0)
        {
            _mesh = new Mesh();

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

            //_mesh.RecalculateBounds();
            _meshFilter.mesh = _mesh;
        }
    }
}