using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardManager : MonoBehaviour
{

    [Header("Level")]
    [SerializeField] private LevelData currentLevelData;

    [Header("Board Visuals")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private BoardPiece piecePrefab;
    [SerializeField] private float cellSize = 1.2f;

    private BoardPiece[,] boardPieces;
    private GameObject[,] boardCells;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event Action OnBoardStateChanged;

   private void Awake()
{
    LoadLevel(currentLevelData);
}

    public void LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("BoardManager: No LevelData assigned.");
            return;
        }

        ClearBoard();

        Width = levelData.width;
        Height = levelData.height;

        boardPieces = new BoardPiece[Width, Height];
        boardCells = new GameObject[Width, Height];

        CreateCells();
        SpawnPlacedPieces(levelData);
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

    private void SpawnPlacedPieces(LevelData levelData)
    {
        foreach (PieceData pieceData in levelData.placedPieces)
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

            SpawnPiece(pieceData);
        }
    }

public BoardPiece SpawnPiece(PieceData pieceData)
{
    BoardPiece piece = Instantiate(piecePrefab, boardRoot);
    piece.transform.localPosition = GridToLocalPosition(pieceData.gridPosition);
    piece.name = $"{pieceData.pieceType}_{pieceData.gridPosition.x}_{pieceData.gridPosition.y}";

    piece.Initialize(
        pieceData.pieceType,
        pieceData.gridPosition,
        pieceData.direction,
        pieceData.isFixed,
        pieceData.isRequired,
        this
    );

    boardPieces[pieceData.gridPosition.x, pieceData.gridPosition.y] = piece;

    return piece;
}

    public void HandlePieceClicked(BoardPiece clickedPiece)
    {
        if (clickedPiece == null)
            return;

        if (!clickedPiece.CanRotate())
            return;

        clickedPiece.RotateClockwise();

        OnBoardChanged();
    }

private void OnBoardChanged()
{
    Debug.Log("Board changed.");
    OnBoardStateChanged?.Invoke();
}

    public BoardPiece GetPieceAt(Vector2Int gridPosition)
    {
        if (!IsInsideBounds(gridPosition))
            return null;

        return boardPieces[gridPosition.x, gridPosition.y];
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

    private void ClearBoard()
    {
        if (boardRoot == null)
            return;

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }
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
        isFixed = false,
        isRequired = pieceData.isRequired
    };

    SpawnPiece(placedData);
    OnBoardChanged();
    return true;
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
}