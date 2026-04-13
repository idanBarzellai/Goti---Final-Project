using UnityEngine;

[System.Serializable]
public struct BeamStep
{
    public Vector2Int gridPosition;
    public Direction direction;

    public BeamStep(Vector2Int gridPosition, Direction direction)
    {
        this.gridPosition = gridPosition;
        this.direction = direction;
    }
}