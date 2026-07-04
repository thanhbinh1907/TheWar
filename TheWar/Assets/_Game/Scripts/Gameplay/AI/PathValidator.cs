using UnityEngine;

public static class PathValidator
{
    public static (bool isValid, string errorMessage) ValidatePath(PathData path)
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

        return (true, string.Empty);
    }
}
