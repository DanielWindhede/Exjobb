﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DisplayDelaunayTriangulation : MonoBehaviour
{
    [SerializeField] private bool _use = true;
    [SerializeField] private float _triangleLineThickness = 2f;
    [SerializeField] private bool _displayAll = true;
    [SerializeField] private int _index = 0;
    [SerializeField] private float _circleCenterRadius1 = 0.1f;
    [SerializeField] private float _circleCenterRadius2 = 0.1f;

    [SerializeField] private Color _circleColors = Color.red;
    [SerializeField] private Color _triangleColors = Color.white;
    [SerializeField] private Color _superTriangleColors = Color.green;
    [SerializeField] private Color _circleCenterColor1 = Color.white;
    [SerializeField] private Color _circleCenterColor2 = Color.black;

    [SerializeField] private bool _showCircles = true;
    [SerializeField] private bool _showTriangles = true;
    [SerializeField] private bool _showSuperTriangles = true;
    [SerializeField] private bool _showCircleCenters = true;

    List<Triangle> _currentTriangulation;
    Vector2 _bounds;

    public void Display(List<Triangle> triangulation, Vector2 bounds)
    {
        _currentTriangulation = triangulation;
        _bounds = bounds;
    }

    private void DrawCircles()
    {
        UnityEditor.Handles.color = _circleColors;
        for (int i = 0; i < _currentTriangulation.Count; i++)
        {
            if (_displayAll || i == _index)
                UnityEditor.Handles.DrawWireDisc(_currentTriangulation[i].Center, Vector3.back, _currentTriangulation[i].Radius);
        }
    }

    private void DrawTriangles()
    {
        Handles.color = _triangleColors;
        foreach (var triangle in _currentTriangulation)
        {
            Handles.DrawLine(triangle.A, triangle.B, _triangleLineThickness);
            Handles.DrawLine(triangle.B, triangle.C, _triangleLineThickness);
            Handles.DrawLine(triangle.C, triangle.A, _triangleLineThickness);
        }
    }

    private void DrawSuperTriangle()
    {
        Gizmos.color = _superTriangleColors;
        if (true)
        {
            // Bounds
            Handles.DrawDottedLine(Vector3.up, Vector2.up * _bounds.y, 15f);
            Handles.DrawDottedLine(Vector3.zero, Vector2.right * _bounds.x, 15f);
            Handles.DrawDottedLine(Vector2.right * _bounds.x, Vector2.one * _bounds, 15f);
            Handles.DrawDottedLine(Vector2.up * _bounds.y, Vector2.one * _bounds, 15f);

            // Super Triangle
            Triangle superTriangle = DelaunayTriangulationGenerator.GenerateSuperTriangle(_bounds);
            Gizmos.DrawLine(superTriangle.A, superTriangle.B);
            Gizmos.DrawLine(superTriangle.B, superTriangle.C);
            Gizmos.DrawLine(superTriangle.C, superTriangle.A);
        }
    }

    private void DrawCircleCenters()
    {
        for (int i = 0; i < _currentTriangulation.Count; i++)
        {
            if (_displayAll || i == _index)
            {
                UnityEditor.Handles.color = _circleCenterColor1;
                UnityEditor.Handles.DrawSolidDisc(_currentTriangulation[i].Center, Vector3.back, _circleCenterRadius1);
                UnityEditor.Handles.color = _circleCenterColor2;
                UnityEditor.Handles.DrawSolidDisc(_currentTriangulation[i].Center, Vector3.back, _circleCenterRadius2);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentTriangulation != null && _use)
        {
            if (_showCircles)
                DrawCircles();
            if (_showTriangles)
                DrawTriangles();
            if (_showSuperTriangles)
                DrawSuperTriangle();
            if (_showCircleCenters)
                DrawCircleCenters();
        }   
    }
}
