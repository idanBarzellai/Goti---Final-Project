using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableInventoryPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private PieceSpriteLibrary spriteLibrary;

    [Header("Rotation Indicator")]
    [SerializeField] private GameObject rotateIndicator;

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

[SerializeField] private TMP_Text stackCounterText;

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
    }

    public void OnBeginDrag(PointerEventData eventData)
{
    if (IsFullyUsed || canvas == null)
    return;

    dragGhost = Instantiate(gameObject, canvas.transform);
    dragGhost.name = $"{gameObject.name}_DragGhost";

    DraggableInventoryPiece ghostDrag = dragGhost.GetComponent<DraggableInventoryPiece>();
    if (ghostDrag != null)
        Destroy(ghostDrag);

    dragGhostRect = dragGhost.GetComponent<RectTransform>();
    dragGhostCanvasGroup = dragGhost.GetComponent<CanvasGroup>();

    if (dragGhostCanvasGroup == null)
        dragGhostCanvasGroup = dragGhost.AddComponent<CanvasGroup>();

    dragGhostCanvasGroup.blocksRaycasts = false;

    dragGhostRect.position = rectTransform.position;
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
        MarkUsedOnBoard();

           if (GameManager.Instance != null)
        GameManager.Instance.RefreshFireButtonAvailability();
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
        }

        iconImage.color = Color.white;

        if (rotateIndicator != null)
            rotateIndicator.SetActive(true);
    }

  private void RefreshUsedState()
{
    if (canvasGroup != null)
    {
        canvasGroup.alpha = IsFullyUsed ? usedAlpha : 1f;
        canvasGroup.blocksRaycasts = !IsFullyUsed;
        canvasGroup.interactable = !IsFullyUsed;
    }

    if (stackCounterText != null)
    {
        int remaining = stackCount - usedCount;

stackCounterText.transform.parent.gameObject.SetActive(stackCount > 1);
        stackCounterText.text = remaining.ToString();
    }
}
}