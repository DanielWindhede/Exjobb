﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DisplayPathing : MonoBehaviour
{
    [SerializeField] bool _use = true;
    [SerializeField] float _lineThickness = 2f;
    [SerializeField] bool _showPath = true;
    [SerializeField] Color _pathColor = Color.magenta;
    [SerializeField] Vector2 _offset;

    List<Vector2> _path;

    public void Display(List<Vector2> path)
    {
        _path = path;
    }

    void DisplayPath()
    {
        Handles.color = _pathColor;
        for (int i = 1; i < _path.Count; i++)
            Handles.DrawLine(_path[i - 1] + _offset, _path[i] + _offset, _lineThickness);

        /*
        Gizmos.color = _pathColor;

        for (int i = 1; i < _path.Count; i++) 
            Gizmos.DrawLine(_path[i - 1] + _offset, _path[i] + _offset);
        */
    }

    private void OnDrawGizmos()
    {
        if (_path != null && _use)
        {
            if (_showPath)
                DisplayPath();
        }
    }
}
