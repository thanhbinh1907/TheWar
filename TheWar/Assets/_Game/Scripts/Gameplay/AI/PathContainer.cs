using System.Collections.Generic;
using UnityEngine;

public class PathContainer : MonoBehaviour
{
    [SerializeField] private List<PathData> _paths = new List<PathData>();

    [Header("Smoothing")]
    [SerializeField] private float _cornerRadius = 2f;
    [SerializeField] private int _segmentsPerCorner = 5;

    [Header("Road Constraints")]
    [SerializeField] private float _roadWidth = 5f;

    public float RoadWidth => _roadWidth;

    private Dictionary<string, PathData> _smoothedCache = new Dictionary<string, PathData>();

    private void OnValidate()
    {
        // Xóa cache khi chỉnh sửa trên Inspector để cập nhật lại Gizmos/Runtime
        _smoothedCache?.Clear();
    }

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
        if (_smoothedCache.TryGetValue(pathId, out PathData cachedPath))
        {
            return cachedPath;
        }

        foreach (var path in _paths)
        {
            if (path.pathId == pathId)
            {
                // Bake smoothed path
                PathData smoothedPath = new PathData
                {
                    pathId = path.pathId,
                    waypointPositions = PathCornerSmoother.RoundCorners(path.waypointPositions, _cornerRadius, _segmentsPerCorner)
                };
                _smoothedCache[pathId] = smoothedPath;
                return smoothedPath;
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

        foreach (var rawPath in _paths)
        {
            if (rawPath.waypointPositions == null || rawPath.waypointPositions.Count == 0)
                continue;

            // Lấy path đã được làm mượt để vẽ Gizmo
            PathData path = GetPath(rawPath.pathId);
            if (path == null) continue;

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
