using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SolvedLevelConfig
{
    [Tooltip("Hint positions for pieces that come from the inventory. Do not include pieces already placed on the board; direction and rotation are ignored.")]
    public List<PieceData> pieces = new List<PieceData>();
}

[CreateAssetMenu(fileName = "LevelData", menuName = "LaserPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 5;
    public int height = 5;

    // [Header("Lose Conditions")]
    // public int maxLaserTries = 3;

    [Header("Already placed on board")]
    public List<PieceData> placedPieces = new List<PieceData>();

    [Header("Available in inventory tray")]
    public List<PieceData> inventoryPieces = new List<PieceData>();

    [Header("Hint positions for inventory pieces")]
    public SolvedLevelConfig solvedLevelConfig = new SolvedLevelConfig();
}
