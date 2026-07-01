using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public class TutorialOverlayUI : MonoBehaviour
{
    private const int MaxOverlayPanels = 25;
    private const int MaxHighlightBoxes = 2;
    private const int LinesPerHighlightBox = 4;

    private enum Step
    {
        Inactive,
        EntryIntro,
        TargetIntro,
        BoardIntro,
        BankIntro,
        DragPiece,
        Fire,
        AllPieces,
        Rotate,
        Complete
    }

    [Header("Scene References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private InventoryBarUI inventoryBarUI;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RectTransform fireButtonRect;
    [SerializeField] private SimplePopupMessageUI popupMessageUI;

    [Header("Overlay")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.72f);
    [SerializeField] private float highlightPadding = 36f;
    [SerializeField] private Color focusBoxColor = new Color(1f, 0.86f, 0.18f, 0.75f);
    [SerializeField] private float focusBoxLineThickness = 8f;
    [SerializeField] private float focusBreathPadding = 12f;
    [SerializeField] private float focusBreathSpeed = 0.55f;

    [Header("Hand Cue")]
    [SerializeField] private Vector2 handSize = new Vector2(96f, 96f);
    [SerializeField] private float handTravelDuration = 1.2f;
    [SerializeField] private float handPauseDuration = 0.25f;
    [SerializeField] private Color handColor = new Color(1f, 0.88f, 0.34f, 1f);

    private readonly List<Image> panels = new List<Image>();
    private readonly List<Image> focusBoxLines = new List<Image>();
    private RectTransform overlayRoot;
    private RectTransform canvasRect;
    private RectTransform handRect;
    private Step currentStep = Step.Inactive;
    private Coroutine handRoutine;
    private bool tutorialActive;
    private LaserSimulationResult pendingLaserResult;
    private bool pendingLaserSolved;
    private bool hasDragHintCell;
    private Vector2Int dragHintCell;

    private void Awake()
    {
        ResolveReferences();
        CreateOverlay();
        HideOverlay();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        if (ShouldRunTutorial())
            StartTutorial();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopHandCue();
    }

    private void Update()
    {
        if (!tutorialActive)
            return;

        RefreshHighlight();
        HandleSkipInput();
    }

    private void ResolveReferences()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;

        if (boardManager == null)
            boardManager = FindAnyObjectByType<BoardManager>();

        if (inventoryBarUI == null)
            inventoryBarUI = FindAnyObjectByType<InventoryBarUI>();

        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();

        if (popupMessageUI == null)
            popupMessageUI = FindAnyObjectByType<SimplePopupMessageUI>();

        if (fireButtonRect == null)
        {
            GameObject fireButtonObject = GameObject.Find("FireButton");
            if (fireButtonObject != null)
                fireButtonRect = fireButtonObject.transform as RectTransform;
        }
    }

    private void Subscribe()
    {
        ResolveReferences();

        if (boardManager != null)
        {
            boardManager.OnBoardLoaded += HandleBoardLoaded;
            boardManager.OnPiecePlacedFromInventory += HandlePiecePlacedFromInventory;
            boardManager.OnPieceRotated += HandlePieceRotated;
        }

        if (gameManager != null)
        {
            gameManager.OnFireLaserStarted += HandleFireLaserStarted;
            gameManager.OnLaserResolved += HandleLaserResolved;
        }
    }

    private void Unsubscribe()
    {
        if (boardManager != null)
        {
            boardManager.OnBoardLoaded -= HandleBoardLoaded;
            boardManager.OnPiecePlacedFromInventory -= HandlePiecePlacedFromInventory;
            boardManager.OnPieceRotated -= HandlePieceRotated;
        }

        if (gameManager != null)
        {
            gameManager.OnFireLaserStarted -= HandleFireLaserStarted;
            gameManager.OnLaserResolved -= HandleLaserResolved;
        }
    }

    private bool ShouldRunTutorial()
    {
        return LevelManager.Instance != null &&
               LevelManager.Instance.CurrentLevelIndex == 0 &&
               canvasRect != null;
    }

    private void StartTutorial()
    {
        tutorialActive = true;
        SetStep(Step.EntryIntro);
    }

    private void HandleBoardLoaded()
    {
        if (!ShouldRunTutorial())
        {
            CompleteTutorial();
            return;
        }

        if (!tutorialActive || currentStep == Step.Complete)
            StartTutorial();
    }

    private void SetStep(Step step)
    {
        ClearDragHintCell();
        currentStep = step;
        StopHandCue();

        switch (currentStep)
        {
            case Step.EntryIntro:
                ShowMessage("This is where GOTI starts");
                ShowOverlay(GetPieceRect(PieceType.Entry));
                break;

            case Step.TargetIntro:
                ShowMessage("This is the grave");
                ShowOverlay(GetPieceRect(PieceType.Target));
                break;

            case Step.BoardIntro:
                ShowMessage("Help GOTI get back to his Grave");
                ShowOverlay(GetBoardRect());
                break;

            case Step.BankIntro:
                ShowMessage("You must use all pieces in the bank");
                ShowOverlay(GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null));
                break;

            case Step.DragPiece:
                ShowMessage("Try dragging a piece to the board");
                HighlightDragTargetCell();
                ShowOverlay(Union(GetBoardRect(), GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null)));
                StartHandCue();
                break;

            case Step.Fire:
                ShowMessage("Try launching GOTI to find his way");
                ShowOverlay(GetRectTransformRect(fireButtonRect));
                break;

            case Step.AllPieces:
                ShowMessage("You must go through all pieces in order to finish the level");
                ShowOverlay(GetBoardRect());
                break;

            case Step.Rotate:
                ShowMessage("Tapping on an piece to rotate it");
                ShowOverlay(GetBoardRect());
                break;

            case Step.Complete:
                CompleteTutorial();
                break;
        }
    }

    private void ShowMessage(string message)
    {
        EnsurePopupVisibleAboveOverlay();
        popupMessageUI?.ShowPersistentMessage(message);
    }

    private void HandleSkipInput()
    {
        if (!CanSkipCurrentStep())
            return;

        if (!WasSkipPressed())
            return;

        AdvanceSkippableStep();
    }

    private bool WasSkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool clicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool tapped = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        bool keyboardSkip =
            Keyboard.current != null &&
            (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);

        return clicked || tapped || keyboardSkip;
#else
        bool clicked = Input.GetMouseButtonDown(0);
        bool tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool keyboardSkip = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space);

        return clicked || tapped || keyboardSkip;
#endif
    }

    private bool CanSkipCurrentStep()
    {
        switch (currentStep)
        {
            case Step.EntryIntro:
            case Step.TargetIntro:
            case Step.BoardIntro:
            case Step.BankIntro:
            case Step.AllPieces:
                return true;

            default:
                return false;
        }
    }

    private void AdvanceSkippableStep()
    {
        switch (currentStep)
        {
            case Step.EntryIntro:
                SetStep(Step.TargetIntro);
                break;

            case Step.TargetIntro:
                SetStep(Step.BoardIntro);
                break;

            case Step.BoardIntro:
                SetStep(Step.BankIntro);
                break;

            case Step.BankIntro:
                SetStep(Step.DragPiece);
                break;

            case Step.AllPieces:
                CompleteTutorial();
                break;
        }
    }

    private void HandlePiecePlacedFromInventory(BoardPiece piece)
    {
        if (tutorialActive && currentStep == Step.DragPiece)
            SetStep(Step.Fire);
    }

    private void HandleFireLaserStarted()
    {
        popupMessageUI?.HideMessage();

        if (tutorialActive && currentStep == Step.Fire)
            HideOverlay();
    }

    private void HandleLaserResolved(LaserSimulationResult result, bool solved)
    {
        if (!tutorialActive || currentStep != Step.Fire)
            return;

        pendingLaserResult = result;
        pendingLaserSolved = solved;

        if (!pendingLaserSolved && pendingLaserResult != null)
            SetStep(Step.Rotate);
        else
            CompleteTutorial();
    }

    private void HandlePieceRotated(BoardPiece piece)
    {
        if (tutorialActive && currentStep == Step.Rotate)
            SetStep(Step.AllPieces);
    }

    private void CreateOverlay()
    {
        if (canvasRect == null)
            return;

        overlayRoot = new GameObject("TutorialPanels", typeof(RectTransform)).GetComponent<RectTransform>();
        overlayRoot.SetParent(canvasRect, false);
        overlayRoot.SetAsLastSibling();
        overlayRoot.anchorMin = Vector2.zero;
        overlayRoot.anchorMax = Vector2.one;
        overlayRoot.offsetMin = Vector2.zero;
        overlayRoot.offsetMax = Vector2.zero;

        for (int i = 0; i < MaxOverlayPanels; i++)
        {
            Image panel = new GameObject($"TutorialPanel_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            panel.transform.SetParent(overlayRoot, false);
            panel.color = panelColor;
            panel.raycastTarget = true;
            panels.Add(panel);
        }

        for (int i = 0; i < MaxHighlightBoxes * LinesPerHighlightBox; i++)
        {
            Image line = new GameObject($"TutorialFocusLine_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            line.transform.SetParent(overlayRoot, false);
            line.color = focusBoxColor;
            line.raycastTarget = false;
            focusBoxLines.Add(line);
        }

        CreateHandCue();
    }

    private void CreateHandCue()
    {
        handRect = new GameObject("TutorialHandIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
        handRect.SetParent(overlayRoot, false);
        handRect.anchorMin = new Vector2(0.5f, 0.5f);
        handRect.anchorMax = new Vector2(0.5f, 0.5f);
        handRect.pivot = new Vector2(0.25f, 0.8f);
        handRect.sizeDelta = handSize;

        Image handImage = handRect.GetComponent<Image>();
        handImage.sprite = CreateHandSprite();
        handImage.color = handColor;
        handImage.raycastTarget = false;

        handRect.gameObject.SetActive(false);
    }

    private Sprite CreateHandSprite()
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color fill = Color.white;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
                texture.SetPixel(x, y, clear);
        }

        FillCircle(texture, 30, 17, 13, fill);
        FillRect(texture, 25, 17, 36, 44, fill);
        FillRect(texture, 31, 24, 39, 56, fill);
        FillRect(texture, 40, 24, 47, 49, fill);
        FillRect(texture, 17, 21, 26, 42, fill);
        FillRect(texture, 9, 24, 18, 38, fill);
        FillRect(texture, 23, 8, 42, 22, fill);

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64f);
    }

    private void FillRect(Texture2D texture, int minX, int minY, int maxX, int maxY, Color color)
    {
        for (int y = Mathf.Max(0, minY); y <= Mathf.Min(texture.height - 1, maxY); y++)
        {
            for (int x = Mathf.Max(0, minX); x <= Mathf.Min(texture.width - 1, maxX); x++)
                texture.SetPixel(x, y, color);
        }
    }

    private void FillCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        int radiusSquared = radius * radius;

        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
                    continue;

                int dx = x - centerX;
                int dy = y - centerY;

                if (dx * dx + dy * dy <= radiusSquared)
                    texture.SetPixel(x, y, color);
            }
        }
    }

    private void StartHandCue()
    {
        if (handRect == null)
            return;

        handRoutine = StartCoroutine(HandCueRoutine());
    }

    private IEnumerator HandCueRoutine()
    {
        handRect.gameObject.SetActive(true);

        while (currentStep == Step.DragPiece)
        {
            Rect bankRect = GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.FirstAvailablePieceRect : null);
            Rect targetRect = GetTutorialTargetCellRect();

            Vector2 start = bankRect.center;
            Vector2 end = targetRect.center;
            float time = 0f;

            while (time < handTravelDuration && currentStep == Step.DragPiece)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / handTravelDuration);
                t = Mathf.SmoothStep(0f, 1f, t);
                handRect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            yield return new WaitForSeconds(handPauseDuration);
        }
    }

    private void StopHandCue()
    {
        if (handRoutine != null)
        {
            StopCoroutine(handRoutine);
            handRoutine = null;
        }

        if (handRect != null)
            handRect.gameObject.SetActive(false);
    }

    private void RefreshHighlight()
    {
        switch (currentStep)
        {
            case Step.EntryIntro:
                ShowOverlay(GetPieceRect(PieceType.Entry), true);
                break;

            case Step.TargetIntro:
                ShowOverlay(GetPieceRect(PieceType.Target), true);
                break;

            case Step.BoardIntro:
            case Step.AllPieces:
            case Step.Rotate:
                ShowOverlay(GetBoardRect(), true);
                break;

            case Step.BankIntro:
                ShowOverlay(GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null), true);
                break;

            case Step.DragPiece:
                ShowOverlay(Union(GetBoardRect(), GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null)), true);
                break;

            case Step.Fire:
                ShowOverlay(GetRectTransformRect(fireButtonRect), true);
                break;
        }
    }

    private void ShowOverlay(Rect highlightRect, bool includePopup = false)
    {
        if (overlayRoot == null || panels.Count == 0 || canvasRect == null)
            return;

        overlayRoot.gameObject.SetActive(true);
        overlayRoot.SetAsLastSibling();
        EnsurePopupVisibleAboveOverlay();

        Rect canvasBounds = canvasRect.rect;
        List<Rect> highlightRects = new List<Rect>
        {
            ExpandAndClamp(highlightRect, canvasBounds, highlightPadding)
        };

        if (includePopup && popupMessageUI != null)
        {
            Rect popupRect = GetRectTransformRect(popupMessageUI.transform as RectTransform);
            highlightRects.Add(ExpandAndClamp(popupRect, canvasBounds, highlightPadding));
        }

        LayoutPanelsAroundHighlights(canvasBounds, highlightRects);
        LayoutFocusBoxes(canvasBounds, highlightRects);
    }

    private void LayoutPanelsAroundHighlights(Rect canvasBounds, List<Rect> highlightRects)
    {
        List<float> xCuts = new List<float> { canvasBounds.xMin, canvasBounds.xMax };
        List<float> yCuts = new List<float> { canvasBounds.yMin, canvasBounds.yMax };

        foreach (Rect highlightRect in highlightRects)
        {
            AddCut(xCuts, highlightRect.xMin, canvasBounds.xMin, canvasBounds.xMax);
            AddCut(xCuts, highlightRect.xMax, canvasBounds.xMin, canvasBounds.xMax);
            AddCut(yCuts, highlightRect.yMin, canvasBounds.yMin, canvasBounds.yMax);
            AddCut(yCuts, highlightRect.yMax, canvasBounds.yMin, canvasBounds.yMax);
        }

        xCuts.Sort();
        yCuts.Sort();

        int panelIndex = 0;

        for (int y = 0; y < yCuts.Count - 1; y++)
        {
            for (int x = 0; x < xCuts.Count - 1; x++)
            {
                Rect cell = Rect.MinMaxRect(xCuts[x], yCuts[y], xCuts[x + 1], yCuts[y + 1]);

                if (cell.width <= 0.01f || cell.height <= 0.01f || IsInsideAnyHighlight(cell.center, highlightRects))
                    continue;

                if (panelIndex >= panels.Count)
                    break;

                Image panel = panels[panelIndex];
                panel.gameObject.SetActive(true);
                SetPanel(panel.rectTransform, cell.xMin, cell.yMin, cell.width, cell.height);
                panelIndex++;
            }
        }

        for (int i = panelIndex; i < panels.Count; i++)
            panels[i].gameObject.SetActive(false);
    }

    private void LayoutFocusBoxes(Rect canvasBounds, List<Rect> highlightRects)
    {
        float breath = (Mathf.Sin(Time.unscaledTime * focusBreathSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float extraPadding = Mathf.Lerp(0f, focusBreathPadding, breath);
        Color lineColor = focusBoxColor;
        lineColor.a = Mathf.Lerp(focusBoxColor.a * 0.55f, focusBoxColor.a, breath);

        int lineIndex = 0;
        int boxCount = Mathf.Min(1, highlightRects.Count);

        for (int i = 0; i < boxCount; i++)
        {
            Rect rect = ExpandAndClamp(highlightRects[i], canvasBounds, extraPadding);

            if (lineIndex + LinesPerHighlightBox > focusBoxLines.Count)
                break;

            SetFocusLine(focusBoxLines[lineIndex++], rect.xMin, rect.yMax - focusBoxLineThickness, rect.width, focusBoxLineThickness, lineColor);
            SetFocusLine(focusBoxLines[lineIndex++], rect.xMin, rect.yMin, rect.width, focusBoxLineThickness, lineColor);
            SetFocusLine(focusBoxLines[lineIndex++], rect.xMin, rect.yMin, focusBoxLineThickness, rect.height, lineColor);
            SetFocusLine(focusBoxLines[lineIndex++], rect.xMax - focusBoxLineThickness, rect.yMin, focusBoxLineThickness, rect.height, lineColor);
        }

        for (int i = lineIndex; i < focusBoxLines.Count; i++)
            focusBoxLines[i].gameObject.SetActive(false);
    }

    private void SetFocusLine(Image line, float x, float y, float width, float height, Color color)
    {
        line.gameObject.SetActive(true);
        line.color = color;
        SetPanel(line.rectTransform, x, y, Mathf.Max(0f, width), Mathf.Max(0f, height));
    }

    private void AddCut(List<float> cuts, float value, float min, float max)
    {
        value = Mathf.Clamp(value, min, max);

        for (int i = 0; i < cuts.Count; i++)
        {
            if (Mathf.Abs(cuts[i] - value) <= 0.01f)
                return;
        }

        cuts.Add(value);
    }

    private bool IsInsideAnyHighlight(Vector2 point, List<Rect> highlightRects)
    {
        foreach (Rect highlightRect in highlightRects)
        {
            if (highlightRect.Contains(point))
                return true;
        }

        return false;
    }

    private void SetPanel(RectTransform rectTransform, float x, float y, float width, float height)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = new Vector2(x, y);
        rectTransform.sizeDelta = new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, height));
    }

    private Rect GetBoardRect()
    {
        if (boardManager == null || boardManager.BoardRoot == null || boardManager.Width <= 0 || boardManager.Height <= 0)
            return canvasRect != null ? canvasRect.rect : new Rect(-100f, -100f, 200f, 200f);

        float halfWidth = ((boardManager.Width - 1) * boardManager.CellStep + boardManager.CellSize) * 0.5f;
        float halfHeight = ((boardManager.Height - 1) * boardManager.CellStep + boardManager.CellSize) * 0.5f;

        Vector3[] localCorners =
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f)
        };

        return WorldCornersToCanvasRect(boardManager.BoardRoot, localCorners);
    }

    private Rect GetTutorialTargetCellRect()
    {
        Vector2Int target = GetTutorialTargetGridPosition();

        if (boardManager == null || boardManager.BoardRoot == null)
            return GetBoardRect();

        return GetCellRect(target);
    }

    private Vector2Int GetTutorialTargetGridPosition()
    {
        Vector2Int target = new Vector2Int(1, 1);
        LevelData currentLevel = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevel : null;

        if (currentLevel != null && currentLevel.solvedLevelConfig != null && currentLevel.solvedLevelConfig.pieces != null)
        {
            foreach (PieceData piece in currentLevel.solvedLevelConfig.pieces)
            {
                if (piece == null || IsInitiallyPlaced(currentLevel, piece.gridPosition))
                    continue;

                target = piece.gridPosition;
                break;
            }
        }

        return target;
    }

    private void HighlightDragTargetCell()
    {
        if (boardManager == null)
            return;

        dragHintCell = GetTutorialTargetGridPosition();
        hasDragHintCell = boardManager.TrySetCellHintHighlighted(dragHintCell, true);
    }

    private void ClearDragHintCell()
    {
        if (!hasDragHintCell || boardManager == null)
            return;

        boardManager.TrySetCellHintHighlighted(dragHintCell, false);
        hasDragHintCell = false;
    }

    private Rect GetPieceRect(PieceType pieceType)
    {
        BoardPiece piece = FindPiece(pieceType);

        if (piece == null || boardManager == null || boardManager.BoardRoot == null)
            return GetBoardRect();

        return GetCellRect(piece.GridPosition);
    }

    private BoardPiece FindPiece(PieceType pieceType)
    {
        if (boardManager == null)
            return null;

        foreach (BoardPiece piece in boardManager.GetAllPieces())
        {
            if (piece != null && piece.PieceType == pieceType)
                return piece;
        }

        return null;
    }

    private Rect GetCellRect(Vector2Int gridPosition)
    {
        if (boardManager == null || boardManager.BoardRoot == null)
            return GetBoardRect();

        float half = boardManager.CellSize * 0.5f;
        Vector3 center = boardManager.GridToLocalPosition(gridPosition);
        Vector3[] localCorners =
        {
            center + new Vector3(-half, -half, 0f),
            center + new Vector3(-half, half, 0f),
            center + new Vector3(half, half, 0f),
            center + new Vector3(half, -half, 0f)
        };

        return WorldCornersToCanvasRect(boardManager.BoardRoot, localCorners);
    }

    private bool IsInitiallyPlaced(LevelData levelData, Vector2Int position)
    {
        if (levelData == null || levelData.placedPieces == null)
            return false;

        foreach (PieceData piece in levelData.placedPieces)
        {
            if (piece != null && piece.gridPosition == position)
                return true;
        }

        return false;
    }

    private Rect WorldCornersToCanvasRect(Transform source, Vector3[] localCorners)
    {
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector3 localCorner in localCorners)
        {
            Vector3 world = source.TransformPoint(localCorner);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, world);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 canvasPoint))
                continue;

            min = Vector2.Min(min, canvasPoint);
            max = Vector2.Max(max, canvasPoint);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private Rect GetRectTransformRect(RectTransform rectTransform)
    {
        if (rectTransform == null || canvasRect == null)
            return new Rect(-100f, -100f, 200f, 200f);

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector3 corner in corners)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corner);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 canvasPoint))
                continue;

            min = Vector2.Min(min, canvasPoint);
            max = Vector2.Max(max, canvasPoint);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private Rect Union(Rect a, Rect b)
    {
        return Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin),
            Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax),
            Mathf.Max(a.yMax, b.yMax)
        );
    }

    private Rect ExpandAndClamp(Rect rect, Rect bounds, float padding)
    {
        rect.xMin -= padding;
        rect.xMax += padding;
        rect.yMin -= padding;
        rect.yMax += padding;

        rect.xMin = Mathf.Clamp(rect.xMin, bounds.xMin, bounds.xMax);
        rect.xMax = Mathf.Clamp(rect.xMax, bounds.xMin, bounds.xMax);
        rect.yMin = Mathf.Clamp(rect.yMin, bounds.yMin, bounds.yMax);
        rect.yMax = Mathf.Clamp(rect.yMax, bounds.yMin, bounds.yMax);

        return rect;
    }

    private void HideOverlay()
    {
        if (overlayRoot != null)
            overlayRoot.gameObject.SetActive(false);
    }

    private void EnsurePopupVisibleAboveOverlay()
    {
        if (popupMessageUI == null)
            return;

        Canvas popupCanvas = popupMessageUI.GetComponent<Canvas>();

        if (popupCanvas == null)
            popupCanvas = popupMessageUI.gameObject.AddComponent<Canvas>();

        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = canvas != null ? canvas.sortingOrder + 100 : 100;
        popupMessageUI.transform.SetAsLastSibling();
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;
        currentStep = Step.Complete;
        ClearDragHintCell();
        StopHandCue();
        HideOverlay();
        popupMessageUI?.HideMessage();
    }
}
