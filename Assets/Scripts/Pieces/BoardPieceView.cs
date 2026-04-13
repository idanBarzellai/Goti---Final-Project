using UnityEngine;

[RequireComponent(typeof(BoardPiece))]
public class BoardPieceView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Colors")]
    [SerializeField] private Color entryColor = Color.green;
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private Color blockColor = Color.gray;
    [SerializeField] private Color reflectColor = Color.cyan;
    [SerializeField] private Color fixedOverlayTint = new Color(0.8f, 0.8f, 0.8f, 1f);

    private BoardPiece boardPiece;

    private void Awake()
    {
        boardPiece = GetComponent<BoardPiece>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (boardPiece == null || spriteRenderer == null)
            return;

        switch (boardPiece.PieceType)
        {
            case PieceType.Entry:
                spriteRenderer.color = entryColor;
                break;
            case PieceType.Target:
                spriteRenderer.color = targetColor;
                break;
            case PieceType.Block:
                spriteRenderer.color = blockColor;
                break;
            case PieceType.Reflect:
                spriteRenderer.color = reflectColor;
                break;
            default:
                spriteRenderer.color = Color.white;
                break;
        }

        if (!boardPiece.CanMove && boardPiece.PieceType != PieceType.Entry && boardPiece.PieceType != PieceType.Target)
        {
            spriteRenderer.color *= fixedOverlayTint;
        }
    }
}