using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Visuals")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private BoardPiece piecePrefab;
    [SerializeField] private float cellSize = 1.2f;

    private BoardPiece[,] boardPieces;
    private GameObject[,] boardCells;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Transform BoardRoot => boardRoot;
    public float CellSize => cellSize;

    public event Action OnBoardStateChanged;
    public event Action OnBoardLoaded;

    public void LoadBoard(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("BoardManager: No LevelData provided.");
            return;
        }

        ClearBoard();

        Width = levelData.width;
        Height = levelData.height;

        boardPieces = new BoardPiece[Width, Height];
        boardCells = new GameObject[Width, Height];

        CreateCells();
        SpawnPlacedPieces(levelData.placedPieces);

        OnBoardLoaded?.Invoke();
    }

    private void CreateCells()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3 localPosition = GridToLocalPosition(new Vector2Int(x, y));
                GameObject cell = Instantiate(cellPrefab, boardRoot);
                cell.transform.localPosition = localPosition;
                cell.name = $"Cell_{x}_{y}";
                boardCells[x, y] = cell;
            }
        }
    }

 private void SpawnPlacedPieces(List<PieceData> placedPieces)
{
    if (placedPieces == null)
        return;

    foreach (PieceData pieceData in placedPieces)
    {
        if (!IsInsideBounds(pieceData.gridPosition))
        {
            Debug.LogWarning($"Piece out of bounds: {pieceData.pieceType} at {pieceData.gridPosition}");
            continue;
        }

        if (boardPieces[pieceData.gridPosition.x, pieceData.gridPosition.y] != null)
        {
            Debug.LogWarning($"Cell already occupied at {pieceData.gridPosition}");
            continue;
        }

        SpawnPiece(pieceData, false, false, false);
    }
}

  public BoardPiece SpawnPiece(
    PieceData pieceData,
    bool canMove,
    bool canRotate,
    bool canReturnToInventory)
{
    BoardPiece piece = Instantiate(piecePrefab, boardRoot);
    piece.transform.localPosition = GridToLocalPosition(pieceData.gridPosition);
    piece.name = $"{pieceData.pieceType}_{pieceData.gridPosition.x}_{pieceData.gridPosition.y}";

    piece.Initialize(
        pieceData.pieceType,
        pieceData.gridPosition,
        pieceData.direction,
        pieceData.isRequired,
        canMove,
        canRotate,
        canReturnToInventory,
        this
    );

    boardPieces[pieceData.gridPosition.x, pieceData.gridPosition.y] = piece;

    SetupSpawnedPiece(piece);

    return piece;
}
private void SetupSpawnedPiece(BoardPiece piece)
{
    if (piece == null)
        return;

    BoardPieceDragHandler dragHandler = piece.GetComponent<BoardPieceDragHandler>();
    if (dragHandler != null)
    {
        dragHandler.Initialize(this);
    }
}
    public void HandlePieceClicked(BoardPiece clickedPiece)
    {
        if (clickedPiece == null)
            return;

        if (!clickedPiece.CanRotate)
            return;

        clickedPiece.RotateClockwise();
        OnBoardChanged();
    }

    public bool TryMovePiece(BoardPiece piece, Vector2Int targetGridPosition)
    {
if (piece == null || !piece.CanMove)
    return false;

        if (!IsInsideBounds(targetGridPosition))
            return false;

        if (!IsCellEmpty(targetGridPosition))
            return false;

        Vector2Int oldPosition = piece.GridPosition;
        boardPieces[oldPosition.x, oldPosition.y] = null;

        piece.SetGridPosition(targetGridPosition);
        piece.transform.localPosition = GridToLocalPosition(targetGridPosition);

        boardPieces[targetGridPosition.x, targetGridPosition.y] = piece;

        OnBoardChanged();
        return true;
    }

    public bool TryRemovePieceToInventory(BoardPiece piece)
    {
  if (piece == null || !piece.CanReturnToInventory)
    return false;

        Vector2Int pos = piece.GridPosition;

        if (IsInsideBounds(pos) && boardPieces[pos.x, pos.y] == piece)
        {
            boardPieces[pos.x, pos.y] = null;
        }

        Destroy(piece.gameObject);
        OnBoardChanged();
        return true;
    }

    public bool IsCellEmpty(Vector2Int gridPosition)
    {
        if (!IsInsideBounds(gridPosition))
            return false;

        return boardPieces[gridPosition.x, gridPosition.y] == null;
    }

 public bool TryPlaceNewPieceFromData(PieceData pieceData, Vector2Int targetGridPosition)
{
    if (pieceData == null)
        return false;

    if (!IsInsideBounds(targetGridPosition))
        return false;

    if (!IsCellEmpty(targetGridPosition))
        return false;

    PieceData placedData = new PieceData
    {
        pieceType = pieceData.pieceType,
        gridPosition = targetGridPosition,
        direction = pieceData.direction,
        isRequired = pieceData.isRequired
    };

    SpawnPiece(placedData, true, true, true);
    OnBoardChanged();
    return true;
}
    public BoardPiece GetPieceAt(Vector2Int gridPosition)
    {
        if (!IsInsideBounds(gridPosition))
            return null;

        return boardPieces[gridPosition.x, gridPosition.y];
    }

    public BoardPiece FindEntryPiece()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                BoardPiece piece = boardPieces[x, y];
                if (piece != null && piece.PieceType == PieceType.Entry)
                    return piece;
            }
        }

        return null;
    }

    public IEnumerable<BoardPiece> GetAllPieces()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (boardPieces[x, y] != null)
                    yield return boardPieces[x, y];
            }
        }
    }

    public bool IsInsideBounds(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 &&
               gridPosition.x < Width &&
               gridPosition.y >= 0 &&
               gridPosition.y < Height;
    }

    public Vector3 GridToLocalPosition(Vector2Int gridPosition)
    {
        float offsetX = -(Width - 1) * cellSize * 0.5f;
        float offsetY = -(Height - 1) * cellSize * 0.5f;

        return new Vector3(
            offsetX + gridPosition.x * cellSize,
            offsetY + gridPosition.y * cellSize,
            0f
        );
    }

    public bool TryGetGridPositionFromWorld(Vector3 worldPosition, out Vector2Int gridPosition)
    {
        Vector3 localPosition = boardRoot.InverseTransformPoint(worldPosition);

        float offsetX = -(Width - 1) * cellSize * 0.5f;
        float offsetY = -(Height - 1) * cellSize * 0.5f;

        float rawX = (localPosition.x - offsetX) / cellSize;
        float rawY = (localPosition.y - offsetY) / cellSize;

        int x = Mathf.RoundToInt(rawX);
        int y = Mathf.RoundToInt(rawY);

        gridPosition = new Vector2Int(x, y);

        if (!IsInsideBounds(gridPosition))
            return false;

        Vector3 snappedLocal = GridToLocalPosition(gridPosition);
        float distance = Vector2.Distance(
            new Vector2(localPosition.x, localPosition.y),
            new Vector2(snappedLocal.x, snappedLocal.y)
        );

        float maxSnapDistance = cellSize * 0.45f;
        return distance <= maxSnapDistance;
    }

    private void OnBoardChanged()
    {
        OnBoardStateChanged?.Invoke();
    }

    private void ClearBoard()
    {
        if (boardRoot == null)
            return;

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }
    }
}