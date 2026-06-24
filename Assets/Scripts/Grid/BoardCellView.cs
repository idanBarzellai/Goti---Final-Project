using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoardCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite cantBeRotatedSprite;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    public void SetCantBeRotated(bool cantBeRotated)
    {
        EnsureReferences();

        if (spriteRenderer == null)
            return;

        Sprite targetSprite = cantBeRotated && cantBeRotatedSprite != null
            ? cantBeRotatedSprite
            : defaultSprite;

        if (targetSprite != null)
            spriteRenderer.sprite = targetSprite;
    }

    private void EnsureReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (defaultSprite == null && spriteRenderer != null)
            defaultSprite = spriteRenderer.sprite;
    }
}
