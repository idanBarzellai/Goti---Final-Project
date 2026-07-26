using System.Collections;
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
    private Coroutine idleRoutine;
    private Coroutine traversalShakeRoutine;
    private Transform shakeRoot;
    private Vector3 shakeOrigin;
    private bool entryAway;
    private const float AnimationFrameDuration = 0.06f;
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
        StartIdleLoop();
    }

    public PieceSpriteLibrary SpriteLibrary => spriteLibrary;

    private void StartIdleLoop()
    {
        Sprite[] frames = spriteLibrary != null && boardPiece != null ? spriteLibrary.GetIdleFrames(boardPiece.PieceType) : null;
        if (frames != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[0];
            idleRoutine = StartCoroutine(IdleLoop(frames));
        }
    }

    private IEnumerator IdleLoop(Sprite[] frames)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 8f));
            if (entryAway)
                continue;

            foreach (Sprite frame in frames)
            {
                if (entryAway)
                    break;

                if (frame != null)
                    spriteRenderer.sprite = frame;

                yield return new WaitForSeconds(AnimationFrameDuration);
            }

            if (!entryAway && frames[0] != null)
                spriteRenderer.sprite = frames[0];
        }
    }

    public void SetEntryAway(bool away)
    {
        if (boardPiece == null || boardPiece.PieceType != PieceType.Entry || spriteLibrary == null) return;
        entryAway = away;
        Sprite pointSprite = boardPiece.CanRotate ? spriteLibrary.rotatableEntryPointSprite : spriteLibrary.fixedEntryPointSprite;

        if (away)
        {
            if (pointSprite != null)
            {
                spriteRenderer.enabled = true;
                if (shadowRenderer != null)
                    shadowRenderer.enabled = true;
                spriteRenderer.sprite = pointSprite;
            }
            else
            {
                spriteRenderer.enabled = false;
                if (shadowRenderer != null)
                    shadowRenderer.enabled = false;
            }

            return;
        }

        spriteRenderer.enabled = true;
        if (shadowRenderer != null)
            shadowRenderer.enabled = true;
        Sprite[] idleFrames = spriteLibrary.GetIdleFrames(PieceType.Entry);
        if (idleFrames != null)
        {
            for (int i = 0; i < idleFrames.Length; i++)
            {
                if (idleFrames[i] != null)
                {
                    spriteRenderer.sprite = idleFrames[i];
                    return;
                }
            }
        }

        if (spriteLibrary.entrySprite != null)
            spriteRenderer.sprite = spriteLibrary.entrySprite;
    }

    public Quaternion VisualWorldRotation => spriteRenderer != null ? spriteRenderer.transform.rotation : transform.rotation;

    public void StartTraversalShake()
    {
        if (traversalShakeRoutine != null || spriteRenderer == null)
            return;

        shakeRoot = spriteRenderer.transform.parent != null ? spriteRenderer.transform.parent : spriteRenderer.transform;
        shakeOrigin = shakeRoot.localPosition;
        traversalShakeRoutine = StartCoroutine(TraversalShakeRoutine());
    }

    public void StopTraversalShake()
    {
        if (traversalShakeRoutine != null)
        {
            StopCoroutine(traversalShakeRoutine);
            traversalShakeRoutine = null;
        }

        if (shakeRoot != null)
            shakeRoot.localPosition = shakeOrigin;
    }

    private IEnumerator TraversalShakeRoutine()
    {
        while (true)
        {
            shakeRoot.localPosition = shakeOrigin + (Vector3)(Random.insideUnitCircle * 0.035f);
            yield return new WaitForSeconds(0.045f);
        }
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
