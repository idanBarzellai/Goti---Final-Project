using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

public class TutorialOverlayUI : MonoBehaviour
{
    private const int MaxOverlayPanels = 25;
    private const int MaxHighlightBoxes = 2;

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
    [SerializeField] private float focusBoxLineThickness = 12f;
    [SerializeField] private Color focusBoxOutlineColor = new Color(0f, 0f, 0f, 0.95f);
    [SerializeField, Min(0f)] private float focusBoxOutlineThickness = 4f;
    [SerializeField, Min(0f)] private float focusCornerRadius = 24f;
    [SerializeField] private float focusBreathPadding = 12f;
    [SerializeField] private float focusBreathSpeed = 0.55f;
    [SerializeField, Min(0f)] private float focusEdgeBlur = 32f;

    [Header("Hand Cue")]
    [Tooltip("Replace this with the hand image you want to use in the tutorial.")]
    [SerializeField] private Sprite handSprite;
    [SerializeField] private Vector2 handSize = new Vector2(96f, 96f);
    [SerializeField] private float handTravelDuration = 1.2f;
    [SerializeField] private float handPauseDuration = 0.25f;
    [SerializeField] private Color handColor = new Color(1f, 0.88f, 0.34f, 1f);
    [SerializeField] private Vector2 handStartPositionOffset;
    [SerializeField] private Vector2 handTargetPositionOffset;
    [SerializeField] private Vector2 launchHandPositionOffset = new Vector2(0f, -80f);
    [SerializeField] private Vector2 rotateHandPositionOffset = new Vector2(0f, -80f);
    [SerializeField, Min(0f)] private float tapHandBobDistance = 12f;
    [SerializeField, Min(0.01f)] private float tapHandBobSpeed = 2f;

    private readonly List<Image> panels = new List<Image>();
    private readonly List<Image> focusBoxOutlines = new List<Image>();
    private readonly List<Image> focusBoxLines = new List<Image>();
    private readonly List<Image> focusEdgeFeathers = new List<Image>();
    private readonly List<Sprite> generatedFeatherSprites = new List<Sprite>();
    private RectTransform overlayRoot;
    private RectTransform canvasRect;
    private RectTransform handRect;
    private Transform pauseScreenRoot;
    private Transform winScreenRoot;
    private PauseMenuUI[] pauseMenus;
    private GameWinPanelUI[] winMenus;
    private bool modalWasActive;
    private Step currentStep = Step.Inactive;
    private Coroutine handRoutine;
    private bool tutorialActive;
    private LaserSimulationResult pendingLaserResult;
    private bool pendingLaserSolved;
    private bool hasDragHintCell;
    private Vector2Int dragHintCell;
    private BoardPiece rotateFocusPiece;

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

    private void OnDestroy()
    {
        foreach (Sprite sprite in generatedFeatherSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite.texture);
                Destroy(sprite);
            }
        }
    }

    private void Update()
    {
        if (!tutorialActive)
            return;

        bool modalActive = IsModalScreenActive();
        if (modalActive)
        {
            if (overlayRoot != null)
                overlayRoot.gameObject.SetActive(false);

            ClearDragHintCell();
            modalWasActive = true;
            return;
        }

        if (modalWasActive)
        {
            modalWasActive = false;
            if (currentStep == Step.DragPiece)
                HighlightDragTargetCell();
        }

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

        if (canvasRect != null)
        {
            pauseScreenRoot = canvasRect.Find("PauseParent");
            winScreenRoot = canvasRect.Find("WinParent");
        }

        pauseMenus = FindObjectsByType<PauseMenuUI>(FindObjectsInactive.Include);
        winMenus = FindObjectsByType<GameWinPanelUI>(FindObjectsInactive.Include);

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
            boardManager.OnPieceReturnedToInventory += HandlePieceReturnedToInventory;
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
            boardManager.OnPieceReturnedToInventory -= HandlePieceReturnedToInventory;
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
                ShowMessage("This is GOTI");
                ShowOverlay(GetPieceRect(PieceType.Entry));
                break;

            case Step.TargetIntro:
                ShowMessage("This is his Grave");
                ShowOverlay(GetPieceRect(PieceType.Target));
                break;

            case Step.BoardIntro:
                ShowMessage("Help GOTI get back to his Grave");
                ShowOverlay(GetBoardRect());
                break;

            case Step.BankIntro:
                ShowMessage("You must use all pieces in the bank");
                ShowOverlay(
                    GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null),
                    true);
                break;

            case Step.DragPiece:
                ShowMessage("Try dragging a piece to the board");
                HighlightDragTargetCell();
                ShowOverlay(Union(GetBoardRect(), GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.InventoryDropArea : null)));
                StartHandCue();
                break;

            case Step.Fire:
                ShowMessage("Try launching GOTI to find his way to his Grave");
                ShowOverlay(GetRectTransformRect(fireButtonRect));
                StartTapHandCue(Step.Fire);
                break;

            case Step.AllPieces:
                ShowMessage("You must go through all pieces to finish a level");
                ShowOverlay(GetBoardRect());
                break;

            case Step.Rotate:
                rotateFocusPiece = FindRotateFocusPiece();
                ShowMessage("Tap on a piece to rotate it");
                ShowOverlay(GetRotateFocusRect());
                StartTapHandCue(Step.Rotate);
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

        AudioManager.Instance?.PlayButtonClick();
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

        bool pointerSkip = clicked || tapped;
        if (pointerSkip &&
            EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            pointerSkip = false;
        }

        return pointerSkip || keyboardSkip;
#else
        bool clicked = Input.GetMouseButtonDown(0);
        bool tapped = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool keyboardSkip = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space);

        bool pointerSkip = clicked || tapped;
        if (pointerSkip && EventSystem.current != null)
        {
            bool pointerOverUi = clicked && EventSystem.current.IsPointerOverGameObject();
            if (tapped)
                pointerOverUi |= EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

            if (pointerOverUi)
                pointerSkip = false;
        }

        return pointerSkip || keyboardSkip;
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

    private void HandlePieceReturnedToInventory(BoardPiece piece)
    {
        if (!tutorialActive)
            return;

        if (currentStep == Step.Fire ||
            currentStep == Step.Rotate ||
            currentStep == Step.AllPieces)
        {
            pendingLaserResult = null;
            pendingLaserSolved = false;
            rotateFocusPiece = null;
            SetStep(Step.DragPiece);
        }
    }

    private void HandleFireLaserStarted()
    {
        if (tutorialActive && currentStep == Step.Fire)
        {
            HideOverlay();
            return;
        }

        popupMessageUI?.HideMessage();
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
        if (tutorialActive && currentStep == Step.Rotate &&
            (rotateFocusPiece == null || piece == rotateFocusPiece))
            SetStep(Step.AllPieces);
    }

    private void CreateOverlay()
    {
        if (canvasRect == null)
            return;

        overlayRoot = new GameObject("TutorialPanels", typeof(RectTransform)).GetComponent<RectTransform>();
        overlayRoot.SetParent(canvasRect, false);
        PlaceOverlayBehindModalScreens();
        overlayRoot.anchorMin = Vector2.zero;
        overlayRoot.anchorMax = Vector2.one;
        overlayRoot.offsetMin = Vector2.zero;
        overlayRoot.offsetMax = Vector2.zero;

        Sprite roundedFocusBorder = CreateRoundedBorderSprite(
            focusBoxLineThickness,
            "TutorialRoundedFocusBorder");
        Sprite roundedOutlineBorder = CreateRoundedBorderSprite(
            focusBoxLineThickness + focusBoxOutlineThickness * 2f,
            "TutorialRoundedFocusOutline");

        for (int i = 0; i < MaxHighlightBoxes; i++)
        {
            Image outline = new GameObject($"TutorialFocusOutline_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            outline.transform.SetParent(overlayRoot, false);
            outline.sprite = roundedOutlineBorder;
            outline.type = Image.Type.Sliced;
            outline.color = focusBoxOutlineColor;
            outline.raycastTarget = false;
            focusBoxOutlines.Add(outline);

            Image border = new GameObject($"TutorialFocusBorder_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
            border.transform.SetParent(overlayRoot, false);
            border.sprite = roundedFocusBorder;
            border.type = Image.Type.Sliced;
            border.color = focusBoxColor;
            border.raycastTarget = false;
            focusBoxLines.Add(border);
        }

        CreateHandCue();
    }

    private Sprite CreateRoundedBorderSprite(float lineThickness, string textureName)
    {
        float radius = Mathf.Max(focusCornerRadius, lineThickness);
        int padding = Mathf.CeilToInt(radius + lineThickness + 2f);
        int size = Mathf.Max(8, padding * 2);
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = textureName;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        Vector2 outerHalfSize = new Vector2(size * 0.5f - 1f, size * 0.5f - 1f);
        Vector2 innerHalfSize = outerHalfSize - Vector2.one * lineThickness;
        float outerRadius = Mathf.Min(radius, Mathf.Min(outerHalfSize.x, outerHalfSize.y));
        float innerRadius = Mathf.Max(0f, outerRadius - lineThickness);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f) - center;
                float outerDistance = RoundedRectDistance(point, outerHalfSize, outerRadius);
                float innerDistance = RoundedRectDistance(point, innerHalfSize, innerRadius);
                float outerAlpha = 1f - Mathf.SmoothStep(-0.5f, 0.5f, outerDistance);
                float innerAlpha = Mathf.SmoothStep(-0.5f, 0.5f, innerDistance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, outerAlpha * innerAlpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        float border = padding;
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        generatedFeatherSprites.Add(sprite);
        return sprite;
    }

    private float RoundedRectDistance(Vector2 point, Vector2 halfSize, float radius)
    {
        Vector2 distance = new Vector2(Mathf.Abs(point.x), Mathf.Abs(point.y)) - halfSize + Vector2.one * radius;
        return Mathf.Min(Mathf.Max(distance.x, distance.y), 0f) +
               new Vector2(Mathf.Max(distance.x, 0f), Mathf.Max(distance.y, 0f)).magnitude -
               radius;
    }

    private void CreateFocusEdgeFeathers()
    {
        Sprite horizontalFadeOut = CreateFeatherSprite(32, 1, false);
        Sprite horizontalFadeIn = CreateFeatherSprite(32, 1, true);
        Sprite verticalFadeOut = CreateFeatherSprite(1, 32, false);
        Sprite verticalFadeIn = CreateFeatherSprite(1, 32, true);
        Sprite topLeftCorner = CreateCornerFeatherSprite(true, true);
        Sprite topRightCorner = CreateCornerFeatherSprite(false, true);
        Sprite bottomLeftCorner = CreateCornerFeatherSprite(true, false);
        Sprite bottomRightCorner = CreateCornerFeatherSprite(false, false);

        for (int box = 0; box < MaxHighlightBoxes; box++)
        {
            Sprite[] sprites =
            {
                verticalFadeOut,
                verticalFadeIn,
                horizontalFadeOut,
                horizontalFadeIn,
                topLeftCorner,
                topRightCorner,
                bottomLeftCorner,
                bottomRightCorner
            };

            foreach (Sprite sprite in sprites)
            {
                Image feather = new GameObject(
                    $"TutorialFocusFeather_{focusEdgeFeathers.Count + 1}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)).GetComponent<Image>();
                feather.transform.SetParent(overlayRoot, false);
                feather.sprite = sprite;
                feather.color = panelColor;
                feather.raycastTarget = false;
                focusEdgeFeathers.Add(feather);
            }
        }
    }

    private Sprite CreateFeatherSprite(int width, int height, bool reverse)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "TutorialFocusFeather";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        int length = Mathf.Max(width, height);
        Color[] pixels = new Color[length];

        for (int i = 0; i < length; i++)
        {
            float t = length > 1 ? i / (float)(length - 1) : 1f;
            t = t * t * (3f - 2f * t);
            pixels[i] = new Color(1f, 1f, 1f, reverse ? 1f - t : t);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        generatedFeatherSprites.Add(sprite);
        return sprite;
    }

    private Sprite CreateCornerFeatherSprite(bool outerLeft, bool outerTop)
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "TutorialFocusCornerFeather";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            float v = y / (float)(size - 1);
            float verticalAlpha = outerTop ? v : 1f - v;
            verticalAlpha = verticalAlpha * verticalAlpha * (3f - 2f * verticalAlpha);

            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);
                float horizontalAlpha = outerLeft ? 1f - u : u;
                horizontalAlpha = horizontalAlpha * horizontalAlpha * (3f - 2f * horizontalAlpha);
                float alpha = Mathf.Max(horizontalAlpha, verticalAlpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        generatedFeatherSprites.Add(sprite);
        return sprite;
    }

    private void CreateHandCue()
    {
        handRect = new GameObject("TutorialHandIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
        handRect.SetParent(overlayRoot, false);
        handRect.anchorMin = new Vector2(0.5f, 0.5f);
        handRect.anchorMax = new Vector2(0.5f, 0.5f);
        handRect.pivot = new Vector2(1f, 1f);
        handRect.sizeDelta = handSize;

        Image handImage = handRect.GetComponent<Image>();
        handImage.sprite = handSprite;
        handImage.color = handSprite != null ? handColor : Color.white;
        handImage.raycastTarget = false;

        Canvas handCanvas = handRect.gameObject.AddComponent<Canvas>();
        handCanvas.overrideSorting = true;
        handCanvas.sortingOrder = canvas != null ? canvas.sortingOrder + 101 : 101;

        handRect.gameObject.SetActive(false);
    }

    private void StartHandCue()
    {
        if (handRect == null)
            return;

        handRoutine = StartCoroutine(HandCueRoutine());
    }

    private void StartTapHandCue(Step step)
    {
        if (handRect == null)
            return;

        handRoutine = StartCoroutine(TapHandCueRoutine(step));
    }

    private IEnumerator TapHandCueRoutine(Step step)
    {
        handRect.gameObject.SetActive(true);

        while (currentStep == step)
        {
            Rect targetRect = step == Step.Fire
                ? GetRectTransformRect(fireButtonRect)
                : GetRotateFocusRect();
            Vector2 positionOffset = step == Step.Fire
                ? launchHandPositionOffset
                : rotateHandPositionOffset;
            float bob = (Mathf.Sin(Time.unscaledTime * tapHandBobSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            handRect.anchoredPosition = targetRect.center + positionOffset + Vector2.up * (bob * tapHandBobDistance);
            yield return null;
        }
    }

    private IEnumerator HandCueRoutine()
    {
        handRect.gameObject.SetActive(true);

        while (currentStep == Step.DragPiece)
        {
            Rect bankRect = GetRectTransformRect(inventoryBarUI != null ? inventoryBarUI.FirstAvailablePieceRect : null);
            Rect targetRect = GetTutorialTargetCellRect();

            Vector2 start = bankRect.center + handStartPositionOffset;
            Vector2 end = targetRect.center + handTargetPositionOffset;
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
        {
            handRect.localScale = Vector3.one;
            handRect.gameObject.SetActive(false);
        }
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
                ShowOverlay(GetBoardRect(), true);
                break;

            case Step.Rotate:
                ShowOverlay(GetRotateFocusRect(), true);
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
        if (overlayRoot == null || focusBoxLines.Count == 0 || canvasRect == null)
            return;

        overlayRoot.gameObject.SetActive(true);
        PlaceOverlayBehindModalScreens();
        EnsurePopupVisibleAboveOverlay();

        Rect canvasBounds = canvasRect.rect;
        List<Rect> highlightRects = new List<Rect>
        {
            ExpandAndClamp(highlightRect, canvasBounds, highlightPadding)
        };

        LayoutFocusBoxes(canvasBounds, highlightRects);
    }

    private void PlaceOverlayBehindModalScreens()
    {
        if (overlayRoot == null || canvasRect == null)
            return;

        int firstModalIndex = canvasRect.childCount;
        Transform pauseParent = canvasRect.Find("PauseParent");
        Transform winParent = canvasRect.Find("WinParent");

        if (pauseParent != null)
        {
            EnsureModalCanvasAboveTutorial(pauseParent);
            firstModalIndex = Mathf.Min(firstModalIndex, pauseParent.GetSiblingIndex());
        }

        if (winParent != null)
        {
            EnsureModalCanvasAboveTutorial(winParent);
            firstModalIndex = Mathf.Min(firstModalIndex, winParent.GetSiblingIndex());
        }

        if (firstModalIndex < canvasRect.childCount &&
            overlayRoot.GetSiblingIndex() > firstModalIndex)
        {
            overlayRoot.SetSiblingIndex(firstModalIndex);
        }
    }

    private bool IsModalScreenActive()
    {
        if (pauseMenus == null || pauseMenus.Length == 0)
        {
            pauseMenus = FindObjectsByType<PauseMenuUI>(FindObjectsInactive.Include);
        }

        for (int i = 0; i < pauseMenus.Length; i++)
        {
            if (pauseMenus[i] != null && pauseMenus[i].gameObject.activeInHierarchy)
                return true;
        }

        if (winMenus == null || winMenus.Length == 0)
        {
            winMenus = FindObjectsByType<GameWinPanelUI>(FindObjectsInactive.Include);
        }

        for (int i = 0; i < winMenus.Length; i++)
        {
            if (winMenus[i] != null && winMenus[i].gameObject.activeInHierarchy)
                return true;
        }

        if (canvasRect != null)
        {
            if (pauseScreenRoot == null)
                pauseScreenRoot = canvasRect.Find("PauseParent");

            if (winScreenRoot == null)
                winScreenRoot = canvasRect.Find("WinParent");
        }

        return (pauseScreenRoot != null && pauseScreenRoot.gameObject.activeInHierarchy) ||
               (winScreenRoot != null && winScreenRoot.gameObject.activeInHierarchy);
    }

    private void EnsureModalCanvasAboveTutorial(Transform modalRoot)
    {
        Canvas modalCanvas = modalRoot.GetComponent<Canvas>();
        if (modalCanvas == null)
            modalCanvas = modalRoot.gameObject.AddComponent<Canvas>();

        modalCanvas.overrideSorting = true;
        modalCanvas.sortingLayerID = canvas != null ? canvas.sortingLayerID : 0;
        modalCanvas.sortingOrder = canvas != null ? canvas.sortingOrder + 200 : 200;

        if (modalRoot.GetComponent<GraphicRaycaster>() == null)
            modalRoot.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void LayoutFocusEdgeFeathers(List<Rect> highlightRects)
    {
        int featherIndex = 0;
        int boxCount = Mathf.Min(MaxHighlightBoxes, highlightRects.Count);

        for (int i = 0; i < boxCount; i++)
        {
            Rect rect = highlightRects[i];
            float horizontalBlur = Mathf.Min(focusEdgeBlur, rect.width * 0.5f);
            float verticalBlur = Mathf.Min(focusEdgeBlur, rect.height * 0.5f);
            float centerWidth = Mathf.Max(0f, rect.width - horizontalBlur * 2f);
            float centerHeight = Mathf.Max(0f, rect.height - verticalBlur * 2f);

            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMin + horizontalBlur, rect.yMax - verticalBlur, centerWidth, verticalBlur);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMin + horizontalBlur, rect.yMin, centerWidth, verticalBlur);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMax - horizontalBlur, rect.yMin + verticalBlur, horizontalBlur, centerHeight);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMin, rect.yMin + verticalBlur, horizontalBlur, centerHeight);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMin, rect.yMax - verticalBlur, horizontalBlur, verticalBlur);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMax - horizontalBlur, rect.yMax - verticalBlur, horizontalBlur, verticalBlur);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMin, rect.yMin, horizontalBlur, verticalBlur);
            SetFeather(focusEdgeFeathers[featherIndex++], rect.xMax - horizontalBlur, rect.yMin, horizontalBlur, verticalBlur);
        }

        for (int i = featherIndex; i < focusEdgeFeathers.Count; i++)
            focusEdgeFeathers[i].gameObject.SetActive(false);
    }

    private void SetFeather(Image feather, float x, float y, float width, float height)
    {
        bool visible = focusEdgeBlur > 0f && width > 0.01f && height > 0.01f;
        feather.gameObject.SetActive(visible);

        if (visible)
            SetPanel(feather.rectTransform, x, y, width, height);
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

        int borderIndex = 0;
        // The first rect is the interactive tutorial target. Additional rects,
        // such as the message popup, stay visible but should not look actionable.
        int boxCount = Mathf.Min(1, highlightRects.Count);

        for (int i = 0; i < boxCount; i++)
        {
            Rect rect = ExpandAndClamp(highlightRects[i], canvasBounds, extraPadding);

            if (borderIndex >= focusBoxLines.Count)
                break;

            Rect outlineRect = ExpandAndClamp(rect, canvasBounds, focusBoxOutlineThickness);
            SetFocusLine(
                focusBoxOutlines[borderIndex],
                outlineRect.xMin,
                outlineRect.yMin,
                outlineRect.width,
                outlineRect.height,
                focusBoxOutlineColor);
            SetFocusLine(focusBoxLines[borderIndex++], rect.xMin, rect.yMin, rect.width, rect.height, lineColor);
        }

        for (int i = borderIndex; i < focusBoxLines.Count; i++)
        {
            focusBoxOutlines[i].gameObject.SetActive(false);
            focusBoxLines[i].gameObject.SetActive(false);
        }
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

    private Rect GetRotateFocusRect()
    {
        if (rotateFocusPiece == null)
            rotateFocusPiece = FindRotateFocusPiece();

        return rotateFocusPiece != null ? GetCellRect(rotateFocusPiece.GridPosition) : GetBoardRect();
    }

    private BoardPiece FindRotateFocusPiece()
    {
        if (pendingLaserResult != null && pendingLaserResult.hitPieces != null)
        {
            for (int i = pendingLaserResult.hitPieces.Count - 1; i >= 0; i--)
            {
                BoardPiece hitPiece = pendingLaserResult.hitPieces[i];
                if (hitPiece != null && hitPiece.CanRotate)
                    return hitPiece;
            }
        }

        if (boardManager != null)
        {
            foreach (BoardPiece piece in boardManager.GetAllPieces())
            {
                if (piece != null && piece.CanRotate)
                    return piece;
            }
        }

        return null;
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
