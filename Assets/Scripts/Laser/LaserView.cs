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
    // [SerializeField] private float textureTilingMultiplier = 1f;

    private readonly List<LineRenderer> activeLineRenderers = new List<LineRenderer>();

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        ApplySettings(lineRenderer);
    }

    public void Render(LaserSimulationResult result)
    {
        Clear();

        if (lineRenderer == null || boardManager == null || boardRoot == null || result == null)
            return;

        List<List<Vector3>> paths = BuildWorldPointPaths(result.segments);

        for (int i = 0; i < paths.Count; i++)
        {
            LineRenderer rendererToUse = i == 0
                ? lineRenderer
                : Instantiate(lineRenderer, transform);

            rendererToUse.gameObject.SetActive(true);
            ApplySettings(rendererToUse);

            rendererToUse.positionCount = paths[i].Count;

            for (int p = 0; p < paths[i].Count; p++)
                rendererToUse.SetPosition(p, paths[i][p]);

            activeLineRenderers.Add(rendererToUse);
        }
    }

    public void Clear()
    {
        foreach (LineRenderer activeRenderer in activeLineRenderers)
        {
            if (activeRenderer == null)
                continue;

            if (activeRenderer == lineRenderer)
            {
                activeRenderer.positionCount = 0;
            }
            else
            {
                Destroy(activeRenderer.gameObject);
            }
        }

        activeLineRenderers.Clear();

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    private List<List<Vector3>> BuildWorldPointPaths(List<BeamSegment> segments)
    {
        List<List<Vector3>> paths = new List<List<Vector3>>();
        List<Vector3> currentPath = null;

        foreach (BeamSegment segment in segments)
        {
            if (currentPath == null || segment.startsNewPath)
            {
                currentPath = new List<Vector3>();
                currentPath.Add(GridCenterToWorld(segment.fromCell));
                paths.Add(currentPath);
            }

            currentPath.Add(GridCenterToWorld(segment.toCell));
        }

        return paths;
    }

    private void ApplySettings(LineRenderer targetRenderer)
    {
        if (targetRenderer == null)
            return;

        targetRenderer.startWidth = lineWidth;
        targetRenderer.endWidth = lineWidth;

        if (lineMaterial != null)
            targetRenderer.material = lineMaterial;

        targetRenderer.textureMode = useTextureTiling
            ? LineTextureMode.Tile
            : LineTextureMode.Stretch;
    }

    private Vector3 GridCenterToWorld(Vector2Int gridPosition)
    {
        Vector3 localPos = boardManager.GridToLocalPosition(gridPosition);
        Vector3 worldPos = boardRoot.TransformPoint(localPos);
        worldPos.z = lineZOffset;
        return worldPos;
    }
}