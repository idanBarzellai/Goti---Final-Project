using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableInventoryPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private PieceSpriteLibrary spriteLibrary;

    [Header("Rotation Indicator")]
    // [SerializeField] private GameObject rotateIndicator;

    [Header("Used Visual")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float usedAlpha = 0f;

    private PieceData pieceData;
    private BoardManager boardManager;
    private Canvas canvas;
    private InventoryBarUI inventoryBarUI;

    private RectTransform rectTransform;

    public bool IsFullyUsed => usedCount >= stackCount;
public bool HasAvailableUses => usedCount < stackCount;

    private GameObject dragGhost;
private RectTransform dragGhostRect;
private CanvasGroup dragGhostCanvasGroup;
private bool iconHiddenForDrag;

[SerializeField] private TMP_Text stackCounterText;
[SerializeField] private RectTransform stackCounterObject;
private CanvasGroup iconCanvasGroup;


private int stackCount = 1;
private int usedCount = 0;

    public void Initialize(
    PieceData pieceData,
    BoardManager boardManager,
    Canvas canvas,
    InventoryBarUI inventoryBarUI,
    int stackCount = 1)
{
    this.pieceData = pieceData;
    this.boardManager = boardManager;
    this.canvas = canvas;
    this.inventoryBarUI = inventoryBarUI;
    this.stackCount = Mathf.Max(1, stackCount);
    usedCount = 0;

    rectTransform = GetComponent<RectTransform>();

    if (canvasGroup == null)
        canvasGroup = GetComponent<CanvasGroup>();

    if (canvasGroup == null)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

    EnsureIconCanvasGroup();
    RefreshVisual();
    RefreshUsedState();
}

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

    if (canvasGroup == null)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

    EnsureIconCanvasGroup();
    }

    public void OnBeginDrag(PointerEventData eventData)
{
    if (IsFullyUsed || canvas == null)
    return;

    CreateIconDragGhost();
    if (dragGhostRect == null || dragGhostCanvasGroup == null)
        return;

    dragGhostCanvasGroup.blocksRaycasts = false;
    SetBankIconVisible(false);
    iconHiddenForDrag = true;

    dragGhostRect.position = iconImage != null
        ? iconImage.rectTransform.position
        : rectTransform.position;
}

   public void OnDrag(PointerEventData eventData)
{
    if (IsFullyUsed || dragGhostRect == null || canvas == null)
        return;

    dragGhostRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
}

public void OnEndDrag(PointerEventData eventData)
{
    if (IsFullyUsed)
        return;

    Vector3 screenPoint = eventData.position;
    screenPoint.z = Mathf.Abs(Camera.main.transform.position.z);

    Vector3 worldDropPosition = Camera.main.ScreenToWorldPoint(screenPoint);
    worldDropPosition.z = 0f;

    bool placedOnBoard =
        boardManager != null &&
        boardManager.TryGetGridPositionFromWorld(worldDropPosition, out Vector2Int targetGridPosition) &&
        boardManager.TryPlaceNewPieceFromData(pieceData, targetGridPosition);

    if (placedOnBoard)
    {
        AudioManager.Instance?.PlayPlacePiece();
        iconHiddenForDrag = false;
        MarkUsedOnBoard();

           if (GameManager.Instance != null)
        GameManager.Instance.RefreshFireButtonAvailability();
    }
    else
    {
        iconHiddenForDrag = false;
        SetBankIconVisible(true);
    }

    DestroyDragGhost();
}

private void DestroyDragGhost()
{
    if (dragGhost != null)
        Destroy(dragGhost);

    dragGhost = null;
    dragGhostRect = null;
    dragGhostCanvasGroup = null;
}
 public bool MatchesPiece(BoardPiece boardPiece)
{
    if (boardPiece == null || pieceData == null)
        return false;

    return pieceData.pieceType == boardPiece.PieceType &&
           pieceData.portalPairId == boardPiece.PortalPairId &&
           usedCount > 0;
}

    public void MarkUsedOnBoard()
{
    usedCount = Mathf.Clamp(usedCount + 1, 0, stackCount);
    RefreshUsedState();
}
   public void MarkAvailable()
{
    usedCount = Mathf.Clamp(usedCount - 1, 0, stackCount);
    RefreshUsedState();
}
    private void RefreshVisual()
{
    if (iconImage == null || pieceData == null)
        return;

    if (spriteLibrary != null)
    {
        Sprite sprite = spriteLibrary.GetSprite(pieceData.pieceType);
        if (sprite != null)
            iconImage.sprite = sprite;

        float rotationOffset = spriteLibrary.GetRotationOffset(pieceData.pieceType);

        iconImage.rectTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            PieceRotationUtility.ToZRotation(pieceData.direction) + rotationOffset
        );
    }
    else
    {
        iconImage.rectTransform.localRotation = Quaternion.Euler(
            0f,
            0f,
            PieceRotationUtility.ToZRotation(pieceData.direction)
        );
    }

    iconImage.color = Color.white;

    // if (rotateIndicator != null)
    //     rotateIndicator.SetActive(true);
}

  private void RefreshUsedState()
{
    if (canvasGroup != null)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = !IsFullyUsed;
        canvasGroup.interactable = !IsFullyUsed;
    }

    if (iconCanvasGroup != null)
    {
        iconCanvasGroup.alpha = IsFullyUsed || iconHiddenForDrag ? usedAlpha : 1f;
        iconCanvasGroup.blocksRaycasts = !IsFullyUsed;
        iconCanvasGroup.interactable = !IsFullyUsed;
    }

    if (stackCounterText != null)
    {
        int remaining = stackCount - usedCount;

stackCounterText.transform.parent.gameObject.SetActive(stackCount > 1);
        stackCounterText.text = remaining.ToString();
    }
}

private void SetBankIconVisible(bool visible)
{
    EnsureIconCanvasGroup();

    if (iconCanvasGroup == null)
        return;

    iconCanvasGroup.alpha = visible ? 1f : usedAlpha;
}

private void EnsureIconCanvasGroup()
{
    if (iconImage == null)
        return;

    if (iconCanvasGroup == null)
        iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();

    if (iconCanvasGroup == null)
        iconCanvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
}

private void CreateIconDragGhost()
{
    if (iconImage == null)
        return;

    dragGhost = new GameObject(
        $"{gameObject.name}_IconDragGhost",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image),
        typeof(CanvasGroup)
    );

    dragGhost.transform.SetParent(canvas.transform, false);

    dragGhostRect = dragGhost.GetComponent<RectTransform>();
    RectTransform sourceRect = iconImage.rectTransform;
    dragGhostRect.anchorMin = new Vector2(0.5f, 0.5f);
    dragGhostRect.anchorMax = new Vector2(0.5f, 0.5f);
    dragGhostRect.pivot = sourceRect.pivot;
    dragGhostRect.sizeDelta = sourceRect.rect.size;
    dragGhostRect.localScale = sourceRect.lossyScale;
    dragGhostRect.localRotation = sourceRect.rotation;

    Image ghostImage = dragGhost.GetComponent<Image>();
    ghostImage.sprite = iconImage.sprite;
    ghostImage.color = iconImage.color;
    ghostImage.type = iconImage.type;
    ghostImage.preserveAspect = iconImage.preserveAspect;
    ghostImage.raycastTarget = false;

    dragGhostCanvasGroup = dragGhost.GetComponent<CanvasGroup>();
    dragGhostCanvasGroup.blocksRaycasts = false;
    dragGhostCanvasGroup.interactable = false;
}
}
