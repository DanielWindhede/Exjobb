using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
	private Controller _controller { get { return (Controller)base.target; } }
	private int _seedHistorySize = 25;
	private List<int> _seedHistory = new List<int>();
	private bool _autoRefresh = true;
	private bool _showBaseGUI = false;
	private bool _showSeedHistory = false;
	private bool _showTrackInformation = false;

	public override void OnInspectorGUI()
	{
		if (EditorApplication.isPlaying)
		{
			if (_seedHistory.Count == 0)
				_seedHistory.Add(_controller.Seed);

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

				if (GUILayout.Button("Random Seed"))
					_controller.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);


				if (_autoRefresh && EditorGUI.EndChangeCheck())
				{
					if (_seedHistory.Count > _seedHistorySize)
						_seedHistory.RemoveAt(0);

					if (!_seedHistory.Contains(_controller.Seed))
						_seedHistory.Add(_controller.Seed);

					_controller.Refresh();
				}

				_showSeedHistory = EditorGUILayout.Foldout(_showSeedHistory, "Seed History", true, EditorStyles.foldout);
				if (_showSeedHistory)
				{
					EditorGUI.indentLevel = 1;
                    for (int i = _seedHistory.Count - 2; i >= 0; i--)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.IntField(_seedHistory[i]);

							if (GUILayout.Button("Use Seed"))
							{
								_controller.Seed = _seedHistory[i];
								_controller.Refresh();
							}
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				EditorGUI.indentLevel = 0;

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
					EditorGUILayout.LabelField("Longest Straight length: " + Math.Round(info.longestStraightLength, 3) + "km", EditorStyles.boldLabel);
					EditorGUILayout.LabelField("Clockwise: " + info.clockWise, EditorStyles.boldLabel);
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