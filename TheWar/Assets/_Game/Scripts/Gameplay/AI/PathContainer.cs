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
    private Dictionary<string, List<BakedSegment>> _bakedSegmentsCache = new Dictionary<string, List<BakedSegment>>();

    private void OnValidate()
    {
        // Xóa cache khi chỉnh sửa trên Inspector để cập nhật lại Gizmos/Runtime
        _smoothedCache?.Clear();
        _bakedSegmentsCache?.Clear();
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

    /// <summary>
    /// Bake and cache Path Segments using Raycast to detect road widths based on Obstacle layer
    /// </summary>
    public List<BakedSegment> GetBakedSegments(string pathId)
    {
        if (_bakedSegmentsCache.TryGetValue(pathId, out List<BakedSegment> cachedSegments))
        {
            return cachedSegments;
        }

        PathData smoothedPath = GetPath(pathId);
        List<BakedSegment> bakedSegments = PathSegmentBaker.BakeSegments(smoothedPath, _roadWidth);

        _bakedSegmentsCache[pathId] = bakedSegments;
        return bakedSegments;
    }

    public int GetWaypointCount(string pathId)
    {
        PathData path = GetPath(pathId);
        return path != null ? path.waypointPositions.Count : 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_paths == null) return;

        foreach (var rawPath in _paths)
        {
            if (rawPath.waypointPositions == null || rawPath.waypointPositions.Count == 0)
                continue;

            // Draw Baked Segments with width representation
            List<BakedSegment> segments = GetBakedSegments(rawPath.pathId);
            if (segments == null) continue;

            foreach (var segment in segments)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(segment.StartNode, segment.EndNode);

                Vector3 direction = (segment.EndNode - segment.StartNode).normalized;
                Vector3 rightDir = Vector3.Cross(Vector3.up, direction).normalized;

                Vector3 centerPos = Vector3.Lerp(segment.StartNode, segment.EndNode, 0.5f);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(centerPos, centerPos - rightDir * segment.MaxLeftWidth); // Left Width
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(centerPos, centerPos + rightDir * segment.MaxRightWidth); // Right Width
            }
        }
    }
#endif
}
