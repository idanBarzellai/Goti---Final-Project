using UnityEngine;

[System.Serializable]
public struct BeamSegment
{
    public Vector2Int fromCell;
    public Vector2Int toCell;
    public bool startsNewPath;

    public BeamSegment(Vector2Int fromCell, Vector2Int toCell, bool startsNewPath = false)
    {
        this.fromCell = fromCell;
        this.toCell = toCell;
        this.startsNewPath = startsNewPath;
    }
}