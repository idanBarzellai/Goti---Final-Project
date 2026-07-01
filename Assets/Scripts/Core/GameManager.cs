using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private LaserControlManager laserControlManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;
    [SerializeField] private LevelManager levelManager;

    [Header("Lose / Win UI")]
    [SerializeField] private LevelTimerManager levelTimerManager;
    [SerializeField] private GameWinPanelUI gameWinPanelUI;
    // [SerializeField] private LaserTriesUI laserTriesUI;

    [SerializeField] private Button fireButton;
    [SerializeField] private CanvasGroup fireButtonCanvasGroup;
[SerializeField] private SimplePopupMessageUI popupMessageUI;

    [Header("Hints")]
    [SerializeField] private Button hintButton;
    [SerializeField] private float hintFlickerDuration = 3f;

    public static GameManager Instance { get; private set; }
    public InventoryBarUI InventoryBarUI => inventoryBarUI;

    public event Action OnLevelSolved;
    public event Action OnFireLaserStarted;
    public event Action<LaserSimulationResult, bool> OnLaserResolved;

    private bool levelEnded;
    // private int triesRemaining;

    [Header("Laser Character Walk")]
[SerializeField] private LaserCharacterWalker laserCharacterWalker;
[SerializeField] private Transform boardRoot;
[SerializeField] private float characterPathZOffset = -0.25f;

private bool laserSequenceRunning;

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
    EnsureHintButton();

            if (levelTimerManager != null)
        levelTimerManager.OnTimerFinished += HandleTimerFinished;

    if (boardManager != null)
        boardManager.OnBoardStateChanged += RefreshFireButtonAvailability;

    RefreshFireButtonAvailability();

        // InitializeTriesFromCurrentLevel();
    }

   private void OnDestroy()
{
    if (levelTimerManager != null)
        levelTimerManager.OnTimerFinished -= HandleTimerFinished;

    if (boardManager != null)
        boardManager.OnBoardStateChanged -= RefreshFireButtonAvailability;

    if (hintButton != null)
        hintButton.onClick.RemoveListener(HintButtonClicked);
}
//    private void InitializeTriesFromCurrentLevel()
// {
//     int maxTries = 3;

//     LevelManager activeLevelManager = ActiveLevelManager;

//     if (activeLevelManager != null && activeLevelManager.CurrentLevel != null)
//         maxTries = Mathf.Max(1, activeLevelManager.CurrentLevel.maxLaserTries);

//     triesRemaining = maxTries;
//     laserTriesUI?.SetTries(triesRemaining, maxTries);
// }
public void RefreshFireButtonAvailability()
{
    bool hasUnusedInventoryPieces =
        inventoryBarUI != null &&
        inventoryBarUI.HasUnusedInventoryPieces();

    bool canActuallyClick =
        !levelEnded &&
        !laserSequenceRunning;

    bool canFire =
        canActuallyClick &&
        !hasUnusedInventoryPieces;

    if (fireButton != null)
        fireButton.interactable = canActuallyClick;

    if (fireButtonCanvasGroup != null)
        fireButtonCanvasGroup.alpha = canFire ? 1f : 0.1f;

    if (hintButton != null)
        hintButton.interactable = canActuallyClick;
}

public void HintButtonClicked()
{
    if (levelEnded || laserSequenceRunning)
        return;

    AudioManager.Instance?.PlayButtonClick();

    LevelManager activeLevelManager = ActiveLevelManager;
    LevelData currentLevel = activeLevelManager != null ? activeLevelManager.CurrentLevel : null;

    bool showedHint =
        boardManager != null &&
        boardManager.TryFlickerSolvedInventoryCell(currentLevel, hintFlickerDuration);

    if (!showedHint)
        popupMessageUI?.ShowMessage("No hint available");
}

public void ShowPopupMessage(string message)
{
    popupMessageUI?.ShowMessage(message);
}

private void EnsureHintButton()
{
    if (hintButton != null)
    {
        hintButton.onClick.RemoveListener(HintButtonClicked);
        hintButton.onClick.AddListener(HintButtonClicked);
    }
}
private void SetFireButtonInteractable(bool interactable)
{
    if (fireButton == null)
        return;

    fireButton.interactable = interactable;

    if (interactable)
        RefreshFireButtonAvailability();
}
public void FireLaserButtonClicked()
{
    if (laserSequenceRunning)
    return;

    Debug.Log("Fire button clicked");

    if (levelEnded)
        return;

        bool hasUnusedInventoryPieces =
    inventoryBarUI != null &&
    inventoryBarUI.HasUnusedInventoryPieces();

if (hasUnusedInventoryPieces)
{
    popupMessageUI?.ShowMessage("Place all pieces first");
    return;
}

    if (laserControlManager == null)
        return;

    AudioManager.Instance?.PlayFireLaser();
    OnFireLaserStarted?.Invoke();

    // if (triesRemaining <= 0)
    //     return;
laserCharacterWalker?.Clear();

    LaserSimulationResult result = laserControlManager.FireLaser();

    if (result == null)
        return;

    bool solved = CheckSolved(result);

laserSequenceRunning = true;
SetFireButtonInteractable(false);

if (laserCharacterWalker == null || boardRoot == null)
{
    laserSequenceRunning = false;
    SetFireButtonInteractable(true);
    ResolveLaserResultAfterVisual(result, solved);
    return;
}

    var paths = BeamPathWorldBuilder.BuildWorldPointPaths(
    result,
    boardManager,
    boardRoot,
    characterPathZOffset
);

    laserCharacterWalker.WalkPaths(
        paths,
        solved,
        () =>
{
    laserSequenceRunning = false;
    SetFireButtonInteractable(true);

    ResolveLaserResultAfterVisual(result, solved);
}
    );
}


private void ResolveLaserResultAfterVisual(LaserSimulationResult result, bool solved)
{
    if (levelEnded)
        return;

    OnLaserResolved?.Invoke(result, solved);

    if (solved)
    {
        HandleLevelSolved();
        gameWinPanelUI?.ShowWin(); 
        return;
    }

    // // triesRemaining--;
    // laserTriesUI?.SetTries(triesRemaining);

    // if (triesRemaining <= 0)
    // {
    //     HandleLose();
    // }
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

        if (!IsRequiredHitPiece(piece))
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

    LevelManager activeLevelManager = ActiveLevelManager;
    if (activeLevelManager != null)
        activeLevelManager.MarkCurrentLevelSolved();

    Debug.Log("LEVEL SOLVED");
    AudioManager.Instance?.PlayWin();
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
        AudioManager.Instance?.PlayReturnPiece();
        RefreshFireButtonAvailability();
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

    laserSequenceRunning = false;

    laserCharacterWalker?.Clear();
laserControlManager?.ClearLaser();

    LevelManager activeLevelManager = ActiveLevelManager;

    if (activeLevelManager != null)
        activeLevelManager.LoadNextLevel();
    else
        Debug.LogError("GameManager: No LevelManager found.");

    // InitializeTriesFromCurrentLevel();
RefreshFireButtonAvailability();


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

    laserSequenceRunning = false;

    laserCharacterWalker?.Clear();
laserControlManager?.ClearLaser();

    LevelManager activeLevelManager = ActiveLevelManager;

    if (activeLevelManager != null)
        activeLevelManager.ReloadCurrentLevel();
    else
        Debug.LogError("GameManager: No LevelManager found.");

    // InitializeTriesFromCurrentLevel();
RefreshFireButtonAvailability();


    if (levelTimerManager != null)
    {
        levelTimerManager.ResetTimer();
        levelTimerManager.StartTimer();
    }

    gameWinPanelUI?.Hide();
}

private bool IsRequiredHitPiece(BoardPiece piece)
{
    if (piece == null)
        return false;

    switch (piece.PieceType)
    {
        case PieceType.Entry:
        case PieceType.Block:
            return false;

        default:
            return true;
    }
}
}
