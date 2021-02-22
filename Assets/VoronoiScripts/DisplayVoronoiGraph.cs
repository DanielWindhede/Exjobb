using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayVoronoiGraph : MonoBehaviour
{
    [SerializeField] private bool _show = true;

    [SerializeField] private bool _showLines = true;
    [SerializeField] private bool _showPoints = true;

    [SerializeField] private Color _lineColor = Color.green;
    [SerializeField] private Color _pointColor1 = Color.black;
    [SerializeField] private Color _pointColor2 = Color.white;

    [SerializeField] private float _pointRadius1 = 0.01f;
    [SerializeField] private float _pointRadius2 = 0.005f;

    VoronoiGraph _graph = null;

    public void Display(VoronoiGraph graph)
    {
        _graph = graph;
    }

    private void DrawLines()
    {
        Gizmos.color = _lineColor;

        for (int i = 0; i < _graph.AllNodesCount; i++)
        {
            foreach (int neighbourNode in _graph.GetGraphNodeByIndex(i).neighborNodeIndexes)
            {
                Gizmos.DrawLine(_graph.GetGraphNodeByIndex(i).position, _graph.GetGraphNodeByIndex(neighbourNode).position);
            }
        }
    }

    private void DrawPoints()
    {
        foreach (Vector2 point in _graph._allPoints)
        {
            UnityEditor.Handles.color = _pointColor1;
            UnityEditor.Handles.DrawSolidDisc(point, Vector3.back, _pointRadius1);
            UnityEditor.Handles.color = _pointColor2;
            UnityEditor.Handles.DrawSolidDisc(point, Vector3.back, _pointRadius2);
        }
    }

    private void OnDrawGizmos()
    {
        if (_graph != null && _show)
        {
            if (_showLines)
                DrawLines();
            if (_showPoints)
                DrawPoints();
        }
    }
}
