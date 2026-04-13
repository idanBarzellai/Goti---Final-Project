using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    public PieceType PieceType { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public Direction Direction { get; private set; }
    public bool IsRequired { get; private set; }

    public bool CanMove { get; private set; }
    public bool CanRotate { get; private set; }
    public bool CanReturnToInventory { get; private set; }

    private BoardManager boardManager;

    public void Initialize(
        PieceType pieceType,
        Vector2Int gridPosition,
        Direction direction,
        bool isRequired,
        bool canMove,
        bool canRotate,
        bool canReturnToInventory,
        BoardManager owningBoardManager)
    {
        PieceType = pieceType;
        GridPosition = gridPosition;
        Direction = direction;
        IsRequired = isRequired;

        CanMove = canMove;
        CanRotate = canRotate;
        CanReturnToInventory = canReturnToInventory;

        boardManager = owningBoardManager;

        RefreshVisualRotation();
    }

    public void SetGridPosition(Vector2Int newGridPosition)
    {
        GridPosition = newGridPosition;
    }

    public void RotateClockwise()
    {
        if (!CanRotate)
            return;

        Direction = PieceRotationUtility.RotateClockwise(Direction);
        RefreshVisualRotation();
    }

    public void RefreshVisualRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, PieceRotationUtility.ToZRotation(Direction));
    }

    public BoardManager GetBoardManager()
    {
        return boardManager;
    }
}