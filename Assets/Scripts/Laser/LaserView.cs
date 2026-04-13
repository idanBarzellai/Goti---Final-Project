using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Transform boardRoot;

    [Header("Visuals")]
    [SerializeField] private float lineZOffset = -0.1f;
    [SerializeField] private float lineWidth = 0.12f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private bool useTextureTiling = true;
    [SerializeField] private float textureTilingMultiplier = 1f;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        ApplyVisualSettings();
    }

    public void ApplyVisualSettings()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }

        lineRenderer.textureMode = useTextureTiling
            ? LineTextureMode.Tile
            : LineTextureMode.Stretch;
    }

    public void Render(LaserSimulationResult result)
    {
        if (lineRenderer == null || boardManager == null || boardRoot == null || result == null)
            return;

        List<Vector3> points = BuildWorldPoints(result.segments);

        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        UpdateTextureTiling(points);
    }

    public void Clear()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = 0;
    }

    private List<Vector3> BuildWorldPoints(List<BeamSegment> segments)
    {
        List<Vector3> points = new List<Vector3>();

        if (segments == null || segments.Count == 0)
            return points;

        for (int i = 0; i < segments.Count; i++)
        {
            BeamSegment segment = segments[i];

            Vector3 startPoint = GridCenterToWorld(segment.fromCell);
            Vector3 endPoint = GridCenterToWorld(segment.toCell);

            if (i == 0)
            {
                points.Add(startPoint);
            }

            points.Add(endPoint);
        }

        return points;
    }

    private void UpdateTextureTiling(List<Vector3> points)
    {
        if (lineRenderer == null || lineRenderer.material == null || points == null || points.Count < 2)
            return;

        float totalLength = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
        }

        if (useTextureTiling)
        {
            lineRenderer.material.mainTextureScale = new Vector2(totalLength * textureTilingMultiplier, 1f);
        }
    }

    private Vector3 GridCenterToWorld(Vector2Int gridPosition)
    {
        Vector3 localPos = boardManager.GridToLocalPosition(gridPosition);
        Vector3 worldPos = boardRoot.TransformPoint(localPos);
        worldPos.z = lineZOffset;
        return worldPos;
    }
}