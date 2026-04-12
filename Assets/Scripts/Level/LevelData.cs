using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "LaserPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    public int width = 5;
    public int height = 5;
    public List<PieceData> placedPieces = new List<PieceData>();
}