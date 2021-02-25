using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Circuit", menuName = "Circuit")]
public class CircuitScriptableObject : ScriptableObject
{
    [SerializeField] public CurvaturePoint[] _circuitPoints;
}
