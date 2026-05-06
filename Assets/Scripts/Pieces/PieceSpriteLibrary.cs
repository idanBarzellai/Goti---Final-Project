using UnityEngine;

[CreateAssetMenu(fileName = "PieceSpriteLibrary", menuName = "LaserPuzzle/Piece Sprite Library")]
public class PieceSpriteLibrary : ScriptableObject
{
    [Header("Piece Sprites")]
    public Sprite entrySprite;
    public Sprite targetSprite;
    public Sprite blockSprite;
    public Sprite reflectSprite;

    public Sprite GetSprite(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Entry:
                return entrySprite;
            case PieceType.Target:
                return targetSprite;
            case PieceType.Block:
                return blockSprite;
            case PieceType.Reflect:
                return reflectSprite;
            default:
                return null;
        }
    }
}