using System.Collections.Generic;
using UnityEngine;

public class PathContainer : MonoBehaviour
{
    [SerializeField] private List<PathData> _paths = new List<PathData>();

    public Vector3 GetWaypoint(string pathId, int index)
    {
        foreach (var path in _paths)
        {
            if (path.pathId == pathId)
            {
                if (index >= 0 && index < path.waypointPositions.Count)
                {
                    return path.waypointPositions[index];
                }
                break;
            }
        }
        return Vector3.zero;
    }

    public PathData GetPath(string pathId)
    {
        foreach (var path in _paths)
        {
            if (path.pathId == pathId)
            {
                return path;
            }
        }
        return null;
    }

    public int GetWaypointCount(string pathId)
    {
        foreach (var path in _paths)
        {
            if (path.pathId == pathId)
            {
                return path.waypointPositions.Count;
            }
        }
        return 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_paths == null) return;

        foreach (var path in _paths)
        {
            if (path.waypointPositions == null || path.waypointPositions.Count == 0)
                continue;

            Gizmos.color = Color.yellow;

            for (int i = 0; i < path.waypointPositions.Count; i++)
            {
                Gizmos.DrawSphere(path.waypointPositions[i], 0.3f);

                if (i < path.waypointPositions.Count - 1)
                {
                    Gizmos.DrawLine(path.waypointPositions[i], path.waypointPositions[i + 1]);
                }
            }
        }
    }
#endif
}
