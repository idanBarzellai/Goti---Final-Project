using System.Collections.Generic;
using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform inventoryDropArea;
    public RectTransform InventoryDropArea => inventoryDropArea;

    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private DraggableInventoryPiece inventoryPiecePrefab;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Canvas canvas;

    private readonly List<DraggableInventoryPiece> spawnedInventoryPieces = new List<DraggableInventoryPiece>();

  public void LoadInventory(LevelData levelData)
{
    ClearInventory();

    if (levelData == null)
    {
        Debug.LogError("InventoryBarUI: No LevelData provided.");
        return;
    }

    if (levelData.inventoryPieces == null)
        return;

    Dictionary<string, InventoryStack> stacks = new Dictionary<string, InventoryStack>();

    foreach (PieceData pieceData in levelData.inventoryPieces)
    {
        string key = GetInventoryKey(pieceData);

        if (!stacks.ContainsKey(key))
        {
            stacks[key] = new InventoryStack
            {
                pieceData = pieceData,
                count = 0
            };
        }

        stacks[key].count++;
    }

    foreach (InventoryStack stack in stacks.Values)
    {
        AddInventoryPiece(stack.pieceData, stack.count);
    }
}

private class InventoryStack
{
    public PieceData pieceData;
    public int count;
}

private string GetInventoryKey(PieceData pieceData)
{
    return $"{pieceData.pieceType}_{pieceData.portalPairId}";
}

   public void AddInventoryPiece(PieceData pieceData, int stackCount = 1)
{
    DraggableInventoryPiece item = Instantiate(inventoryPiecePrefab, inventoryContainer);
    item.Initialize(pieceData, boardManager, canvas, this, stackCount);
    spawnedInventoryPieces.Add(item);
}

    public void ConsumeInventoryPiece(DraggableInventoryPiece piece)
    {
        if (piece == null)
            return;

        piece.MarkUsedOnBoard();
    }

    public void RestoreUsedPiece(BoardPiece boardPiece)
    {
        if (boardPiece == null)
            return;

        foreach (DraggableInventoryPiece inventoryPiece in spawnedInventoryPieces)
        {
            if (inventoryPiece != null && inventoryPiece.MatchesPiece(boardPiece))
            {
                inventoryPiece.MarkAvailable();
                return;
            }
        }

        Debug.LogWarning($"InventoryBarUI: Could not restore inventory piece for {boardPiece.PieceType}");
    }

    private void ClearInventory()
    {
        for (int i = inventoryContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryContainer.GetChild(i).gameObject);
        }

        spawnedInventoryPieces.Clear();
    }

   public bool HasUnusedInventoryPieces()
{
    foreach (DraggableInventoryPiece inventoryPiece in spawnedInventoryPieces)
    {
        if (inventoryPiece != null && inventoryPiece.HasAvailableUses)
            return true;
    }

    return false;
}
}