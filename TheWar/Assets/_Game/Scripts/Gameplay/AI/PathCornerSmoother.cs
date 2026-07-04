using System.Collections.Generic;
using UnityEngine;

public static class PathCornerSmoother
{
    public static List<Vector3> RoundCorners(List<Vector3> rawWaypoints, float cornerRadius, int segmentsPerCorner)
    {
        if (rawWaypoints == null || rawWaypoints.Count < 3)
        {
            // Không đủ điểm để tạo góc cua
            return rawWaypoints == null ? new List<Vector3>() : new List<Vector3>(rawWaypoints);
        }

        // Đảm bảo segmentsPerCorner tối thiểu là 1 để tránh lỗi chia cho 0 (DivideByZero) gây ra NaN tọa độ
        segmentsPerCorner = Mathf.Max(1, segmentsPerCorner);

        List<Vector3> smoothedWaypoints = new List<Vector3>();

        // Điểm đầu tiên luôn giữ nguyên
        smoothedWaypoints.Add(rawWaypoints[0]);

        for (int i = 1; i < rawWaypoints.Count - 1; i++)
        {
            Vector3 pPrev = rawWaypoints[i - 1];
            Vector3 pCorner = rawWaypoints[i];
            Vector3 pNext = rawWaypoints[i + 1];

            Vector3 dirPrev = (pPrev - pCorner).normalized;
            Vector3 dirNext = (pNext - pCorner).normalized;

            float distPrev = Vector3.Distance(pPrev, pCorner);
            float distNext = Vector3.Distance(pNext, pCorner);

            // Giới hạn bán kính bo góc không vượt quá một nửa đoạn đường kề để tránh các góc đè lên nhau
            float actualRadius = Mathf.Min(cornerRadius, distPrev * 0.5f, distNext * 0.5f);

            // Hai điểm bắt đầu và kết thúc của đường cong tại góc này
            Vector3 pStartCurve = pCorner + dirPrev * actualRadius;
            Vector3 pEndCurve = pCorner + dirNext * actualRadius;

            // Tính các điểm trên cung Quadratic Bezier
            for (int j = 0; j <= segmentsPerCorner; j++)
            {
                float t = (float)j / segmentsPerCorner;
                Vector3 curvePoint = CalculateQuadraticBezierPoint(t, pStartCurve, pCorner, pEndCurve);
                
                // Tránh thêm các điểm trùng lặp
                if (smoothedWaypoints.Count == 0 || Vector3.Distance(smoothedWaypoints[smoothedWaypoints.Count - 1], curvePoint) > 0.01f)
                {
                    smoothedWaypoints.Add(curvePoint);
                }
            }
        }

        // Điểm cuối cùng luôn giữ nguyên
        if (Vector3.Distance(smoothedWaypoints[smoothedWaypoints.Count - 1], rawWaypoints[rawWaypoints.Count - 1]) > 0.01f)
        {
            smoothedWaypoints.Add(rawWaypoints[rawWaypoints.Count - 1]);
        }

        return smoothedWaypoints;
    }

    private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2;        // t^2 * P2

        return p;
    }
}
