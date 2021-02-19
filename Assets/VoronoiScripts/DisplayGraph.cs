using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayGraph : MonoBehaviour
{
    [SerializeField] private bool _show = true;

    [SerializeField] private bool _showLines = true;

    [SerializeField] private Color _lineColor = Color.green;

    Graph _graph;

    public void Display(Graph graph)
    {
        _graph = graph;
    }

    private void DrawLines()
    {
        Gizmos.color = _lineColor;

        foreach (Node node in _graph._allNodes)
        {
            foreach (Node neighbourNode in node.neighborNodes)
            {
                Gizmos.DrawLine(node.position, neighbourNode.position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_graph != null && _show)
        {
            if (_showLines)
                DrawLines();
        }
    }
}
