using UnityEngine;

public static class PathValidator
{
    public static (bool isValid, string errorMessage) ValidatePath(PathData path, float roadWidth)
    {
        if (path == null)
        {
            return (false, "PathData is null.");
        }

        if (path.waypointPositions == null || path.waypointPositions.Count == 0)
        {
            return (false, $"Path '{path.pathId}' has no waypoints.");
        }

        for (int i = 0; i < path.waypointPositions.Count - 1; i++)
        {
            if (Vector3.Distance(path.waypointPositions[i], path.waypointPositions[i + 1]) < 0.01f)
            {
                return (false, $"Path '{path.pathId}' has duplicate consecutive waypoints at index {i} and {i + 1}.");
            }
        }

        // Kiểm tra khoảng cách từ tâm đường (centerline) đến các Socket/Obstacle
        float minDistance = roadWidth / 2f;
        var sockets = Object.FindObjectsOfType<TowerDefense.Core.TowerSocket>();
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                for (int i = 0; i < path.waypointPositions.Count - 1; i++)
                {
                    Vector3 a = path.waypointPositions[i];
                    Vector3 b = path.waypointPositions[i + 1];
                    float dist = DistancePointToLineSegment(socket.transform.position, a, b);

                    // Bỏ qua trục Y nếu đường chỉ nằm trên mặt phẳng ngang (tùy chọn, ở đây tính khoảng cách 3D chuẩn)
                    if (dist < minDistance)
                    {
                        return (false, $"Path '{path.pathId}' lấn vào vị trí của TowerSocket tại {socket.transform.position} (Khoảng cách: {dist:F2} < {minDistance}).");
                    }
                }
            }
        }

        return (true, string.Empty);
    }

    private static float DistancePointToLineSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = p - a;
        
        float lengthSqr = ab.sqrMagnitude;
        if (lengthSqr == 0f) return Vector3.Distance(p, a);
        
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / lengthSqr);
        Vector3 projection = a + t * ab;
        
        return Vector3.Distance(p, projection);
    }
}
