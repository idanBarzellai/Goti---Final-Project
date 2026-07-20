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

    [Header("Traversal Animation")]
    [SerializeField] private Color traversalColor = new Color(0.282f, 0.949f, 0.255f, 1f);
    [SerializeField] private float traversalColorDuration = 0.4f;

    private Color defaultColor = Color.white;
    private Coroutine hintRoutine;
    private Coroutine traversalRoutine;
    private Sprite restingSprite;

    private void Awake()
    {
        EnsureReferences();

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
            restingSprite = spriteRenderer.sprite;
        }
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
        {
            spriteRenderer.sprite = targetSprite;
            restingSprite = targetSprite;
        }
    }

    public void PlayTraversalAnimation()
    {
        EnsureReferences();

        if (spriteRenderer == null)
            return;

        if (traversalRoutine != null)
            StopCoroutine(traversalRoutine);

        traversalRoutine = StartCoroutine(TraversalAnimationRoutine());
    }

    public void ResetTraversalAnimation()
    {
        if (traversalRoutine != null)
        {
            StopCoroutine(traversalRoutine);
            traversalRoutine = null;
        }

        if (spriteRenderer != null && restingSprite != null)
        {
            spriteRenderer.sprite = restingSprite;
            spriteRenderer.color = defaultColor;
        }
    }

    private IEnumerator TraversalAnimationRoutine()
    {
        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < traversalColorDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, traversalColor,
                Mathf.Clamp01(elapsed / Mathf.Max(0.01f, traversalColorDuration)));
            yield return null;
        }

        spriteRenderer.color = traversalColor;
        traversalRoutine = null;
    }

    public void FadeTraversalColor(float duration)
    {
        if (spriteRenderer == null)
            return;
        if (traversalRoutine != null)
            StopCoroutine(traversalRoutine);
        traversalRoutine = StartCoroutine(FadeTraversalColorRoutine(duration));
    }

    private IEnumerator FadeTraversalColorRoutine(float duration)
    {
        Color startColor = spriteRenderer.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, defaultColor,
                Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
            yield return null;
        }
        spriteRenderer.color = defaultColor;
        traversalRoutine = null;
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

    public void SetHintHighlighted(bool highlighted)
    {
        EnsureReferences();

        if (spriteRenderer == null)
            return;

        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        spriteRenderer.color = highlighted ? hintColor : defaultColor;
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
