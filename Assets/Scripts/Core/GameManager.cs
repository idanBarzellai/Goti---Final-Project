using System;
using System.Collections;
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
[SerializeField, Min(0f)] private float incompletePathMessageDuration = 3f;

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
private Coroutine screenShakeRoutine;
private Coroutine moonLaunchRoutine;
private RectTransform moonRect;
private Image moonImage;
private Vector2 moonRestPosition;
private Color moonRestColor;

[Header("Bump Feedback")]
[SerializeField] private float bumpShakeDuration = 0.16f;
[SerializeField] private float bumpShakeStrength = 0.08f;

[Header("Moon Launch Feedback")]
[SerializeField] private Color launchedMoonColor = new Color(0.78f, 0.98f, 0.81f, 0.75f);
[SerializeField] private float moonVibrationStrength = 2.5f;
[SerializeField] private float moonVibrationInterval = 0.05f;
[SerializeField] private float previousPathFadeDuration = 3f;

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
    ResolveMoonReference();
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
    StartMoonLaunchFeedback();

    // if (triesRemaining <= 0)
    //     return;
laserCharacterWalker?.Clear();
laserCharacterWalker?.ConfigureFromEntry(boardManager != null ? boardManager.FindEntryPiece() : null);

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

    bool loseSoundPlayed = false;
    laserCharacterWalker.WalkPaths(
        paths,
        solved,
        result.wasBlocked,
        result.exitedBoard,
        worldPosition =>
        {
            AnimateTraversedCell(worldPosition);

            if (loseSoundPlayed || solved || result == null || !result.didHitAnyTarget || boardManager == null)
                return;

            if (boardManager.TryGetGridPositionFromWorld(worldPosition, out Vector2Int gridPosition))
            {
                BoardPiece reachedPiece = boardManager.GetPieceAt(gridPosition);
                if (reachedPiece != null && reachedPiece.PieceType == PieceType.Target)
                {
                    loseSoundPlayed = true;
                    AudioManager.Instance?.PlayLose();
                }
            }
        },
        HandleGotiBump,
        () => gameWinPanelUI?.PlayWinAnimationConfetti(),
        () =>
{
    laserSequenceRunning = false;
    SetFireButtonInteractable(true);
    boardManager?.StopAllPieceTraversalShakes();
    if (!solved)
        boardManager?.FadeCellTraversalAnimations(previousPathFadeDuration);

    ResolveLaserResultAfterVisual(result, solved);
}
    );
}

private void AnimateTraversedCell(Vector3 worldPosition)
{
    if (boardManager != null && boardManager.TryGetGridPositionFromWorld(worldPosition, out Vector2Int gridPosition))
    {
        boardManager.TryPlayCellTraversal(gridPosition);
        boardManager.StartPieceTraversalShake(gridPosition);
        BoardPiece piece = boardManager.GetPieceAt(gridPosition);
        if (piece != null && piece.PieceType != PieceType.Entry && piece.PieceType != PieceType.Target)
            AudioManager.Instance?.PlayWhoosh();
    }
}

private void HandleGotiBump()
{
    AudioManager.Instance?.PlayBump();
    if (screenShakeRoutine != null)
        StopCoroutine(screenShakeRoutine);
    screenShakeRoutine = StartCoroutine(ScreenShakeRoutine());
}

private IEnumerator ScreenShakeRoutine()
{
    Camera camera = Camera.main;
    if (camera == null) yield break;
    Transform cameraTransform = camera.transform;
    Vector3 origin = cameraTransform.localPosition;
    float elapsed = 0f;
    while (elapsed < bumpShakeDuration)
    {
        elapsed += Time.unscaledDeltaTime;
        cameraTransform.localPosition = origin + (Vector3)(UnityEngine.Random.insideUnitCircle * bumpShakeStrength);
        yield return null;
    }
    cameraTransform.localPosition = origin;
    screenShakeRoutine = null;
}


private void ResolveLaserResultAfterVisual(LaserSimulationResult result, bool solved)
{
    StopMoonLaunchFeedback();
    if (levelEnded)
        return;

    OnLaserResolved?.Invoke(result, solved);

    if (solved)
    {
        HandleLevelSolved();
        gameWinPanelUI?.ShowWin(); 
        return;
    }

    if (result != null && result.didHitAnyTarget)
        popupMessageUI?.ShowMessage(
            "To finish a level you must go through all pieces",
            incompletePathMessageDuration);

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
    OnLevelSolved?.Invoke();
}

private void ResolveMoonReference()
{
    GameObject skyCanvas = GameObject.Find("SkyCanvas");
    Transform moon = null;
    if (skyCanvas != null)
    {
        for (int i = 0; i < skyCanvas.transform.childCount; i++)
        {
            Transform child = skyCanvas.transform.GetChild(i);
            if (child.name == "Moon" && child.gameObject.activeInHierarchy)
            {
                moon = child;
                break;
            }
        }
    }
    if (moon == null) return;
    Image sourceImage = moon.GetComponent<Image>();
    if (sourceImage == null) return;

    Transform existingVisual = moon.Find("LaunchShakeVisual");
    if (existingVisual == null)
    {
        GameObject visualObject = new GameObject("LaunchShakeVisual", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        visualObject.transform.SetParent(moon, false);
        moonRect = visualObject.GetComponent<RectTransform>();
        moonRect.anchorMin = Vector2.zero;
        moonRect.anchorMax = Vector2.one;
        moonRect.offsetMin = Vector2.zero;
        moonRect.offsetMax = Vector2.zero;
        moonImage = visualObject.GetComponent<Image>();
        moonImage.sprite = sourceImage.sprite;
        moonImage.material = sourceImage.material;
        moonImage.type = sourceImage.type;
        moonImage.preserveAspect = sourceImage.preserveAspect;
        moonImage.raycastTarget = false;
        moonImage.color = sourceImage.color;
        sourceImage.enabled = false;
    }
    else
    {
        moonRect = existingVisual as RectTransform;
        moonImage = existingVisual.GetComponent<Image>();
    }
    if (moonRect != null) moonRestPosition = moonRect.anchoredPosition;
    if (moonImage != null) moonRestColor = moonImage.color;
}

private void StartMoonLaunchFeedback()
{
    if (moonRect == null || moonImage == null) ResolveMoonReference();
    if (moonRect == null) return;
    if (moonLaunchRoutine != null) StopCoroutine(moonLaunchRoutine);
    moonRestPosition = moonRect.anchoredPosition;
    if (moonImage != null) { moonRestColor = moonImage.color; moonImage.color = launchedMoonColor; }
    moonLaunchRoutine = StartCoroutine(MoonLaunchRoutine());
}

private IEnumerator MoonLaunchRoutine()
{
    while (true)
    {
        moonRect.anchoredPosition = moonRestPosition + UnityEngine.Random.insideUnitCircle * moonVibrationStrength;
        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, moonVibrationInterval));
    }
}

private void StopMoonLaunchFeedback()
{
    if (moonLaunchRoutine != null) { StopCoroutine(moonLaunchRoutine); moonLaunchRoutine = null; }
    if (moonRect != null) moonRect.anchoredPosition = moonRestPosition;
    if (moonImage != null) moonImage.color = moonRestColor;
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
