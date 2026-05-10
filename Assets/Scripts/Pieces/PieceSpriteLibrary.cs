using UnityEngine;

[CreateAssetMenu(fileName = "PieceSpriteLibrary", menuName = "LaserPuzzle/Piece Sprite Library")]
public class PieceSpriteLibrary : ScriptableObject
{
    [Header("Piece Sprites")]
    public Sprite entrySprite;
    public Sprite targetSprite;
    public Sprite blockSprite;
    public Sprite mirrorSprite;
    public Sprite reflectSprite;
    public Sprite checkpointSprite;
    public Sprite portalSprite;

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

            case PieceType.Mirror:
                return mirrorSprite;

            case PieceType.Reflect:
                return reflectSprite;

            case PieceType.Checkpoint:
                return checkpointSprite;

            case PieceType.Portal:
                return portalSprite;

            default:
                return null;
        }
    }
}