using UnityEngine;

[RequireComponent(typeof(BoardPiece))]
public class BoardPieceView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Shadow")]
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private Vector3 shadowLocalOffset = new Vector3(0.12f, -0.12f, 0.05f);
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.45f);

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

        RefreshShadow();
    }

    private void RefreshShadow()
    {
        if (shadowRenderer == null || spriteRenderer == null)
            return;

        shadowRenderer.sprite = spriteRenderer.sprite;
        shadowRenderer.color = shadowColor;
        shadowRenderer.transform.localPosition = shadowLocalOffset;
        shadowRenderer.transform.localRotation = Quaternion.identity;
        shadowRenderer.transform.localScale = Vector3.one;
    }
}