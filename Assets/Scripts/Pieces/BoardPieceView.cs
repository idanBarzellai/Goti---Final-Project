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
    [SerializeField] private MoonShadowCaster moonShadowCaster;
[SerializeField] private PieceSpriteLibrary spriteLibrary;
    private BoardPiece boardPiece;
    [Header("Rotation Indicator")]
[SerializeField] private GameObject rotateIndicator;

    private void Awake()
    {
        boardPiece = GetComponent<BoardPiece>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        moonShadowCaster = FindAnyObjectByType<MoonShadowCaster>();
        Refresh();
    }


private void LateUpdate()
{
    RefreshShadow();
}

public float GetVisualRotationOffset()
{
    if (spriteLibrary == null || boardPiece == null)
        return 0f;

    return spriteLibrary.GetRotationOffset(boardPiece.PieceType);
}

public void Refresh()
{
    if (boardPiece == null || spriteRenderer == null)
        return;

    if (spriteLibrary != null)
    {
        Sprite sprite = spriteLibrary.GetSprite(boardPiece.PieceType);
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    spriteRenderer.color = Color.white;

    if (!boardPiece.CanMove &&
        boardPiece.PieceType != PieceType.Entry &&
        boardPiece.PieceType != PieceType.Target)
    {
        spriteRenderer.color *= fixedOverlayTint;
    }

    if (rotateIndicator != null)
    rotateIndicator.SetActive(boardPiece.CanRotate);

    RefreshShadow();
}
    private void RefreshShadow()
    {
        if (shadowRenderer == null || spriteRenderer == null)
            return;

        shadowRenderer.sprite = spriteRenderer.sprite;
        shadowRenderer.color = shadowColor;
        if (moonShadowCaster != null)
if (moonShadowCaster != null)
{
    shadowRenderer.transform.localPosition = moonShadowCaster.GetShadowOffset();
    shadowRenderer.color = moonShadowCaster.GetShadowColor();
}
else
{
    shadowRenderer.transform.localPosition = shadowLocalOffset;
    shadowRenderer.color = shadowColor;
}
shadowRenderer.transform.localRotation = Quaternion.identity;
        shadowRenderer.transform.localScale = Vector3.one;
    }
}