using System;
using UnityEngine;

[Serializable]
public class PieceData
{
    public PieceType pieceType;
    public Vector2Int gridPosition;
    public Direction direction;
    public bool isRequired;
}