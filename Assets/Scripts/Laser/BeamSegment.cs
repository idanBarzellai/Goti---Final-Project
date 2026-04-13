using UnityEngine;

[System.Serializable]
public struct BeamSegment
{
    public Vector2Int fromCell;
    public Vector2Int toCell;

    public BeamSegment(Vector2Int fromCell, Vector2Int toCell)
    {
        this.fromCell = fromCell;
        this.toCell = toCell;
    }
}