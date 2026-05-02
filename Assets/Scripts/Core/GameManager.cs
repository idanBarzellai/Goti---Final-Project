using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private LaserControlManager laserControlManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;
    [SerializeField] private LevelManager levelManager;

[SerializeField] private LevelTimerManager levelTimerManager;

    public static GameManager Instance { get; private set; }
    public InventoryBarUI InventoryBarUI => inventoryBarUI;

    public event Action OnLevelSolved;

private bool levelSolved;   

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (boardManager != null)
            boardManager.OnBoardStateChanged += HandleBoardStateChanged;

            if (levelTimerManager != null)
    levelTimerManager.OnTimerFinished += HandleTimerFinished;
    }

    private void OnDestroy()
    {
        if (boardManager != null)
            boardManager.OnBoardStateChanged -= HandleBoardStateChanged;

            if (levelTimerManager != null)
    levelTimerManager.OnTimerFinished -= HandleTimerFinished;
    }

    private void HandleBoardStateChanged()
    {
        CheckSolved();
    }

    private void HandleTimerFinished()
{
    Debug.Log("SUNRISE - LEVEL FAILED");
}

   public void ReturnPieceToInventory(BoardPiece piece)
{
    if (piece == null || !piece.CanReturnToInventory || inventoryBarUI == null || boardManager == null)
        return;

    PieceData returnedData = new PieceData
    {
        pieceType = piece.PieceType,
        gridPosition = Vector2Int.zero,
        direction = piece.Direction,
        isRequired = piece.IsRequired
    };

    if (boardManager.TryRemovePieceToInventory(piece))
    {
        inventoryBarUI.AddInventoryPiece(returnedData);
    }
}

   public bool IsInventoryScreenArea(Vector2 screenPosition, RectTransform inventoryArea, Camera eventCamera)
{
    if (inventoryArea == null)
        return false;

    Camera uiCamera = eventCamera;

    Canvas canvas = inventoryArea.GetComponentInParent<Canvas>();
    if (canvas != null)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = null;
        }
        else if (canvas.worldCamera != null)
        {
            uiCamera = canvas.worldCamera;
        }
    }

    return RectTransformUtility.RectangleContainsScreenPoint(inventoryArea, screenPosition, uiCamera);
}

   public bool CheckSolved()
{
    if (levelSolved)
        return true;

    if (boardManager == null || laserControlManager == null)
        return false;

    LaserSimulationResult result = laserControlManager.LastResult;
    if (result == null || !result.didHitAnyTarget)
        return false;

    BoardPiece[] requiredPieces = boardManager
        .GetAllPieces()
        .Where(p => p != null && p.IsRequired)
        .ToArray();

    foreach (BoardPiece requiredPiece in requiredPieces)
    {
        if (!result.hitPieces.Contains(requiredPiece))
            return false;
    }

    levelSolved = true;
    Debug.Log("LEVEL SOLVED");
    OnLevelSolved?.Invoke();

    return true;
}

    public void LoadNextLevel()
{
    levelSolved = false;

    if (levelManager != null)
        levelManager.LoadNextLevel();
}
    public void ReloadLevel()
{
    levelSolved = false;

    if (levelManager != null)
        levelManager.ReloadCurrentLevel();
}
}