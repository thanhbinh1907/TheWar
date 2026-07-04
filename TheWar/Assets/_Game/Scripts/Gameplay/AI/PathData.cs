using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathData
{
    public string pathId;
    public List<Vector3> waypointPositions = new List<Vector3>();
}
