using UnityEngine;

public static class PieceRotationUtility
{
    public static Direction RotateClockwise(Direction direction)
    {
        return (Direction)(((int)direction + 1) % 4);
    }

    public static float ToZRotation(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return 0f;
            case Direction.Right: return -90f;
            case Direction.Down: return 180f;
            case Direction.Left: return 90f;
            default: return 0f;
        }
    }

    public static Vector2Int ToVector2Int(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return new Vector2Int(0, 1);
            case Direction.Right: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(0, -1);
            case Direction.Left: return new Vector2Int(-1, 0);
            default: return Vector2Int.zero;
        }
    }
}