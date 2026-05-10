using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "LaserPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 5;
    public int height = 5;

    [Header("Lose Conditions")]
    public int maxLaserTries = 3;

    [Header("Already placed on board")]
    public List<PieceData> placedPieces = new List<PieceData>();

    [Header("Available in inventory tray")]
    public List<PieceData> inventoryPieces = new List<PieceData>();
}