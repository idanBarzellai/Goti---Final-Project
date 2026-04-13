using System.Collections.Generic;
using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private DraggableInventoryPiece inventoryPiecePrefab;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Canvas canvas;

    private readonly List<DraggableInventoryPiece> spawnedInventoryPieces = new List<DraggableInventoryPiece>();

    private void Start()
    {
        BuildInventory();
    }

    public void BuildInventory()
    {
        ClearInventory();

        if (currentLevelData == null)
        {
            Debug.LogError("InventoryBarUI: No LevelData assigned.");
            return;
        }

        foreach (PieceData pieceData in currentLevelData.inventoryPieces)
        {
            DraggableInventoryPiece item = Instantiate(inventoryPiecePrefab, inventoryContainer);
            item.Initialize(pieceData, boardManager, canvas, this);
            spawnedInventoryPieces.Add(item);
        }
    }

    public void ConsumeInventoryPiece(DraggableInventoryPiece piece)
    {
        if (piece == null)
            return;

        spawnedInventoryPieces.Remove(piece);
        Destroy(piece.gameObject);
    }

    private void ClearInventory()
    {
        for (int i = inventoryContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryContainer.GetChild(i).gameObject);
        }

        spawnedInventoryPieces.Clear();
    }
}