using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoardPiece))]
public class BoardPieceDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private BoardPiece boardPiece;
    private BoardManager boardManager;
    private Camera mainCamera;

    private Vector3 startWorldPosition;
    private Vector2Int startGridPosition;
    private bool dragStarted;
    private bool movedDuringDrag;

    public void Initialize(BoardManager boardManager)
    {
        this.boardManager = boardManager;
    }

    private void Awake()
    {
        boardPiece = GetComponent<BoardPiece>();
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (boardPiece == null || !boardPiece.CanMove)
            return;

        dragStarted = true;
        movedDuringDrag = false;
        startWorldPosition = transform.position;
        startGridPosition = boardPiece.GridPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (boardPiece == null || !boardPiece.CanMove || mainCamera == null || !dragStarted)
            return;

        movedDuringDrag = true;

        Vector3 screenPoint = eventData.position;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPosition.z = startWorldPosition.z;

        transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (boardPiece == null || !boardPiece.CanMove || boardManager == null || mainCamera == null || !dragStarted)
            return;

        dragStarted = false;

        RectTransform inventoryDropArea = null;
        if (GameManager.Instance != null && GameManager.Instance.InventoryBarUI != null)
        {
            inventoryDropArea = GameManager.Instance.InventoryBarUI.InventoryDropArea;
        }

        if (boardPiece.CanReturnToInventory &&
            GameManager.Instance != null &&
            inventoryDropArea != null &&
            GameManager.Instance.IsInventoryScreenArea(eventData.position, inventoryDropArea, eventData.pressEventCamera))
        {
            GameManager.Instance.ReturnPieceToInventory(boardPiece);
            return;
        }

        Vector3 screenPoint = eventData.position;
        screenPoint.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);
        worldPosition.z = 0f;

        if (boardManager.TryGetGridPositionFromWorld(worldPosition, out Vector2Int targetGridPosition))
        {
            if (targetGridPosition == startGridPosition)
            {
                transform.localPosition = boardManager.GridToLocalPosition(startGridPosition);
                return;
            }

            if (boardManager.TryMovePiece(boardPiece, targetGridPosition))
            {
                return;
            }
        }

        transform.localPosition = boardManager.GridToLocalPosition(startGridPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (boardPiece == null)
            return;

        if (movedDuringDrag)
        {
            movedDuringDrag = false;
            return;
        }

        if (boardPiece.CanRotate && boardManager != null)
        {
            boardManager.HandlePieceClicked(boardPiece);
        }
    }
}