using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private LaserControlManager laserControlManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;
    [SerializeField] private LevelManager levelManager;

    [Header("Lose / Win UI")]
    [SerializeField] private LevelTimerManager levelTimerManager;
    [SerializeField] private GameWinPanelUI gameWinPanelUI;
    [SerializeField] private LaserTriesUI laserTriesUI;

    public static GameManager Instance { get; private set; }
    public InventoryBarUI InventoryBarUI => inventoryBarUI;

    public event Action OnLevelSolved;

    private bool levelEnded;
    private int triesRemaining;

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
        if (levelTimerManager != null)
            levelTimerManager.OnTimerFinished += HandleTimerFinished;

        InitializeTriesFromCurrentLevel();
    }

    private void OnDestroy()
    {
        if (levelTimerManager != null)
            levelTimerManager.OnTimerFinished -= HandleTimerFinished;
    }

   private void InitializeTriesFromCurrentLevel()
{
    int maxTries = 3;

    LevelManager activeLevelManager = ActiveLevelManager;

    if (activeLevelManager != null && activeLevelManager.CurrentLevel != null)
        maxTries = Mathf.Max(1, activeLevelManager.CurrentLevel.maxLaserTries);

    triesRemaining = maxTries;
    laserTriesUI?.SetTries(triesRemaining, maxTries);
}

public void FireLaserButtonClicked()
{
    Debug.Log("Fire button clicked");

    if (levelEnded)
        return;

    if (laserControlManager == null)
        return;

    if (triesRemaining <= 0)
        return;

    LaserSimulationResult result = laserControlManager.FireLaser();

    if (result == null)
        return;

    bool solved = CheckSolved(result);

    if (solved)
    {
        HandleLevelSolved();
        return;
    }

    triesRemaining--;
    laserTriesUI?.SetTries(triesRemaining);

    if (triesRemaining <= 0)
    {
        HandleLose();
    }
}

private bool CheckSolved(LaserSimulationResult result)
{
    if (levelEnded)
        return false;

    if (result == null || !result.didHitAnyTarget)
        return false;

    if (inventoryBarUI != null && inventoryBarUI.HasUnusedInventoryPieces())
        return false;

    foreach (BoardPiece piece in boardManager.GetAllPieces())
    {
        if (piece == null)
            continue;

        if (piece.PieceType == PieceType.Entry)
            continue;

        if (!result.hitPieces.Contains(piece))
            return false;
    }

    return true;
}

    private void HandleLevelSolved()
    {
        levelEnded = true;

        levelTimerManager?.StopTimer();

        Debug.Log("LEVEL SOLVED");
        OnLevelSolved?.Invoke();
    }

    private void HandleTimerFinished()
    {
        if (levelEnded)
            return;

        HandleLose();
    }

    private void HandleLose()
    {
        levelEnded = true;

        levelTimerManager?.StopTimer();
        gameWinPanelUI?.ShowFailed();
    }

    public void ReturnPieceToInventory(BoardPiece piece)
{
    if (piece == null || !piece.CanReturnToInventory || inventoryBarUI == null || boardManager == null)
        return;

    if (boardManager.TryRemovePieceToInventory(piece))
    {
        inventoryBarUI.RestoreUsedPiece(piece);
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

private LevelManager ActiveLevelManager
{
    get
    {
        if (levelManager != null)
            return levelManager;

        return LevelManager.Instance;
    }
}

public void LoadNextLevel()
{
    levelEnded = false;

    laserControlManager?.ClearLaser();

    LevelManager activeLevelManager = ActiveLevelManager;

    if (activeLevelManager != null)
        activeLevelManager.LoadNextLevel();
    else
        Debug.LogError("GameManager: No LevelManager found.");

    InitializeTriesFromCurrentLevel();

    if (levelTimerManager != null)
    {
        levelTimerManager.ResetTimer();
        levelTimerManager.StartTimer();
    }

    gameWinPanelUI?.Hide();
}

    public void ReloadLevel()
{
    levelEnded = false;

    laserControlManager?.ClearLaser();

    LevelManager activeLevelManager = ActiveLevelManager;

    if (activeLevelManager != null)
        activeLevelManager.ReloadCurrentLevel();
    else
        Debug.LogError("GameManager: No LevelManager found.");

    InitializeTriesFromCurrentLevel();

    if (levelTimerManager != null)
    {
        levelTimerManager.ResetTimer();
        levelTimerManager.StartTimer();
    }

    gameWinPanelUI?.Hide();
}
}