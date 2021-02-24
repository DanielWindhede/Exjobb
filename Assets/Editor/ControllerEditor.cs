using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
	private Controller _controller { get { return (Controller)base.target; } }

	private bool _autoRefresh = true;
	private bool _showBaseGUI = false;
	private bool _showTrackInformation = false;

	public override void OnInspectorGUI()
	{
		if (EditorApplication.isPlaying)
		{
			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginHorizontal();
				{
					_autoRefresh = EditorGUILayout.Toggle("Use Auto-refresh", _autoRefresh);

					if (_autoRefresh)
						EditorGUI.BeginChangeCheck();
					else if (GUILayout.Button("Refresh"))
						_controller.Refresh();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
                {
					if (GUILayout.Button("-"))
						_controller.Seed--;

					_controller.Seed = EditorGUILayout.IntField(_controller.Seed);

					if (GUILayout.Button("+"))
						_controller.Seed++;
				}

				EditorGUILayout.EndHorizontal();

				if (_autoRefresh && EditorGUI.EndChangeCheck())
					_controller.Refresh();

				_showTrackInformation = EditorGUILayout.Foldout(_showTrackInformation, "Track information", true, EditorStyles.foldout);
				if (_showTrackInformation)
				{
					var e = new GUILayout();
					
					EditorGUI.indentLevel = 1;
					CircuitInformation info = _controller.CircuitInformation;

					EditorGUILayout.LabelField("Circuit length: " + Math.Round(info.circuitLength, 3) + "km", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Preferred Circuit length: " + Math.Round(info.preferredCircuitLength, 3) + "km", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Turn Amount: " + info.turnAmount, EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Closing Straight length: " + Math.Round(info.closingStraightLength, 3) + "km", EditorStyles.boldLabel);
				}

				EditorGUI.indentLevel = 0;

				_showBaseGUI = EditorGUILayout.Foldout(_showBaseGUI, "Base Info", true, EditorStyles.foldout);
				if (_showBaseGUI)
				{
					EditorGUI.indentLevel = 1;
					base.OnInspectorGUI();
				}
			}
			EditorGUILayout.EndVertical();
		}
		else
		{
			base.OnInspectorGUI();
		}
	}
}