using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Controller))]
public class ControllerEditor : Editor
{
	private Controller _controller { get { return (Controller)base.target; } }

	private bool _autoRefresh;
	private bool showBaseGUI;

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

				showBaseGUI = EditorGUILayout.Foldout(showBaseGUI, "Base Info", true, EditorStyles.foldout);

				if (showBaseGUI)
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