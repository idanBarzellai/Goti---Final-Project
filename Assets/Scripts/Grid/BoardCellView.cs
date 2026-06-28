using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoardCellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite cantBeRotatedSprite;
    [SerializeField] private Color hintColor = new Color(1f, 0.86f, 0.18f, 1f);
    [SerializeField] private float hintFlickerInterval = 0.15f;

    private Color defaultColor = Color.white;
    private Coroutine hintRoutine;

    private void Awake()
    {
        EnsureReferences();

        if (spriteRenderer != null)
            defaultColor = spriteRenderer.color;
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

    public void FlickerHint(float duration)
    {
        EnsureReferences();

        if (spriteRenderer == null)
            return;

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        hintRoutine = StartCoroutine(FlickerHintRoutine(duration));
    }

    private IEnumerator FlickerHintRoutine(float duration)
    {
        float elapsed = 0f;
        bool useHintColor = true;

        while (elapsed < duration)
        {
            spriteRenderer.color = useHintColor ? hintColor : defaultColor;
            useHintColor = !useHintColor;

            float waitTime = Mathf.Max(0.01f, hintFlickerInterval);
            elapsed += waitTime;
            yield return new WaitForSeconds(waitTime);
        }

        spriteRenderer.color = defaultColor;
        hintRoutine = null;
    }

    private void EnsureReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (defaultSprite == null && spriteRenderer != null)
            defaultSprite = spriteRenderer.sprite;
    }
}
