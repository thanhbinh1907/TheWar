using System.Collections.Generic;
using UnityEngine;

public struct BakedSegment
{
    public Vector3 StartNode;
    public Vector3 EndNode;
    public float MaxLeftWidth;
    public float MaxRightWidth;
}

public static class PathSegmentBaker
{
    /// <summary>
    /// Bake Path Segments using Raycast to detect road widths based on Obstacle layer
    /// </summary>
    public static List<BakedSegment> BakeSegments(PathData smoothedPath, float maxRoadWidth)
    {
        if (smoothedPath == null || smoothedPath.waypointPositions.Count < 2)
            return null;

        List<BakedSegment> bakedSegments = new List<BakedSegment>();
        int obstacleLayerMask = LayerMask.GetMask("Default");

        for (int i = 0; i < smoothedPath.waypointPositions.Count - 1; i++)
        {
            Vector3 startNode = smoothedPath.waypointPositions[i];
            Vector3 endNode = smoothedPath.waypointPositions[i + 1];

            Vector3 direction = (endNode - startNode).normalized;
            if (direction == Vector3.zero) continue;

            // Calculate right vector (90 degrees clockwise relative to Y-up)
            Vector3 rightDir = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 leftDir = -rightDir;

            float sphereRadius = 0.4f; // Bán kính của quái
            // Khởi tạo chiều rộng tối đa nhưng phải trừ đi bán kính quái để tâm quái không đè lên vạch kẻ đường
            float maxLeft = Mathf.Max(0f, (maxRoadWidth / 2f) - sphereRadius);
            float maxRight = Mathf.Max(0f, (maxRoadWidth / 2f) - sphereRadius);
            
            // Quét tại 3 điểm (Đầu, Giữa, Cuối) của segment để bảo đảm không bị đâm góc cua
            Vector3 centerPos = Vector3.Lerp(startNode, endNode, 0.5f);
            Vector3[] samplePoints = { startNode, centerPos, endNode };

            foreach (var pt in samplePoints)
            {
                Vector3 rayStart = pt + Vector3.up * 0.5f;

                // Spherecast left (bắn xa tối đa tới mép đường)
                if (Physics.SphereCast(rayStart, sphereRadius, leftDir, out RaycastHit hitLeft, maxRoadWidth / 2f, obstacleLayerMask))
                {
                    maxLeft = Mathf.Min(maxLeft, hitLeft.distance);
                }

                // Spherecast right
                if (Physics.SphereCast(rayStart, sphereRadius, rightDir, out RaycastHit hitRight, maxRoadWidth / 2f, obstacleLayerMask))
                {
                    maxRight = Mathf.Min(maxRight, hitRight.distance);
                }
            }

            bakedSegments.Add(new BakedSegment
            {
                StartNode = startNode,
                EndNode = endNode,
                MaxLeftWidth = maxLeft,
                MaxRightWidth = maxRight
            });
        }

        return bakedSegments;
    }
}
