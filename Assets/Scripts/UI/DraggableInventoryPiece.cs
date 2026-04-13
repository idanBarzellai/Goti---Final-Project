using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableInventoryPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private PieceData pieceData;
    private BoardManager boardManager;
    private Canvas canvas;
    private InventoryBarUI inventoryBarUI;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;

    public void Initialize(PieceData pieceData, BoardManager boardManager, Canvas canvas, InventoryBarUI inventoryBarUI)
    {
        this.pieceData = pieceData;
        this.boardManager = boardManager;
        this.canvas = canvas;
        this.inventoryBarUI = inventoryBarUI;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        RefreshVisual();
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalAnchoredPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        Vector3 screenPoint = eventData.position;
screenPoint.z = Mathf.Abs(Camera.main.transform.position.z);
Vector3 worldDropPosition = Camera.main.ScreenToWorldPoint(screenPoint);
worldDropPosition.z = 0f;

        if (boardManager != null &&
            boardManager.TryGetGridPositionFromWorld(worldDropPosition, out Vector2Int targetGridPosition) &&
            boardManager.TryPlaceNewPieceFromData(pieceData, targetGridPosition))
        {
            inventoryBarUI.ConsumeInventoryPiece(this);
            return;
        }

        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    private void RefreshVisual()
    {
        if (iconImage == null || pieceData == null)
            return;

        switch (pieceData.pieceType)
        {
            case PieceType.Entry:
                iconImage.color = Color.green;
                break;
            case PieceType.Target:
                iconImage.color = Color.red;
                break;
            case PieceType.Block:
                iconImage.color = Color.gray;
                break;
            case PieceType.Reflect:
                iconImage.color = Color.cyan;
                break;
            default:
                iconImage.color = Color.white;
                break;
        }
    }
}