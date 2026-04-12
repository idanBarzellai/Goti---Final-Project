using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    public PieceType PieceType { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public Direction Direction { get; private set; }
    public bool IsFixed { get; private set; }
    public bool IsRequired { get; private set; }

    private BoardManager boardManager;

    public void Initialize(
        PieceType pieceType,
        Vector2Int gridPosition,
        Direction direction,
        bool isFixed,
        bool isRequired,
        BoardManager owningBoardManager)
    {
        PieceType = pieceType;
        GridPosition = gridPosition;
        Direction = direction;
        IsFixed = isFixed;
        IsRequired = isRequired;
        boardManager = owningBoardManager;

        RefreshVisualRotation();
    }

    public void SetGridPosition(Vector2Int newGridPosition)
    {
        GridPosition = newGridPosition;
    }

    public bool CanRotate()
    {
        if (IsFixed)
            return false;

        if (PieceType == PieceType.Entry || PieceType == PieceType.Target)
            return false;

        return true;
    }

    public void RotateClockwise()
    {
        if (!CanRotate())
            return;

        Direction = PieceRotationUtility.RotateClockwise(Direction);
        RefreshVisualRotation();
    }

    public void RefreshVisualRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, PieceRotationUtility.ToZRotation(Direction));
    }

    private void OnMouseDown()
    {
        if (boardManager == null)
            return;

        boardManager.HandlePieceClicked(this);
    }
}