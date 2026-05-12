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

    [Header("Visual Rotation Offsets")]
    public float entryRotationOffset;
    public float targetRotationOffset;
    public float blockRotationOffset;
    public float mirrorRotationOffset;
    public float reflectRotationOffset;
    public float checkpointRotationOffset;
    public float portalRotationOffset;

    public Sprite GetSprite(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Entry: return entrySprite;
            case PieceType.Target: return targetSprite;
            case PieceType.Block: return blockSprite;
            case PieceType.Mirror: return mirrorSprite;
            case PieceType.Reflect: return reflectSprite;
            case PieceType.Checkpoint: return checkpointSprite;
            case PieceType.Portal: return portalSprite;
            default: return null;
        }
    }

    public float GetRotationOffset(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Entry: return entryRotationOffset;
            case PieceType.Target: return targetRotationOffset;
            case PieceType.Block: return blockRotationOffset;
            case PieceType.Mirror: return mirrorRotationOffset;
            case PieceType.Reflect: return reflectRotationOffset;
            case PieceType.Checkpoint: return checkpointRotationOffset;
            case PieceType.Portal: return portalRotationOffset;
            default: return 0f;
        }
    }
}