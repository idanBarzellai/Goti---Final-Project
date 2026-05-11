using System.Collections.Generic;
using UnityEngine;

public static class BeamPathWorldBuilder
{
    public static List<Vector3> BuildWorldPoints(
        LaserSimulationResult result,
        BoardManager boardManager,
        Transform boardRoot,
        float zOffset = -0.2f)
    {
        List<Vector3> points = new List<Vector3>();

        if (result == null || result.segments == null || boardManager == null || boardRoot == null)
            return points;

        for (int i = 0; i < result.segments.Count; i++)
        {
            BeamSegment segment = result.segments[i];

            Vector3 start = GridToWorld(segment.fromCell, boardManager, boardRoot, zOffset);
            Vector3 end = GridToWorld(segment.toCell, boardManager, boardRoot, zOffset);

            if (i == 0)
                points.Add(start);

            points.Add(end);
        }

        return points;
    }

    private static Vector3 GridToWorld(
        Vector2Int gridPosition,
        BoardManager boardManager,
        Transform boardRoot,
        float zOffset)
    {
        Vector3 localPos = boardManager.GridToLocalPosition(gridPosition);
        Vector3 worldPos = boardRoot.TransformPoint(localPos);
        worldPos.z = zOffset;
        return worldPos;
    }
}