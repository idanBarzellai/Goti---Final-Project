using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LoopingCloudUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float speed = 6f;
    [SerializeField, Min(0f)] private float wrapPadding = 10f;

    private RectTransform cloudRect;
    private RectTransform parentRect;
    private RectTransform viewportRect;
    private readonly Vector3[] corners = new Vector3[4];

    private void Awake()
    {
        cloudRect = transform as RectTransform;
        parentRect = cloudRect != null ? cloudRect.parent as RectTransform : null;
        Canvas canvas = GetComponentInParent<Canvas>();
        viewportRect = canvas != null ? canvas.rootCanvas.transform as RectTransform : parentRect;
    }

    private void Update()
    {
        if (cloudRect == null || parentRect == null || viewportRect == null || speed <= 0f)
            return;

        Vector2 position = cloudRect.anchoredPosition;
        position.x -= speed * Time.unscaledDeltaTime;
        cloudRect.anchoredPosition = position;

        GetHorizontalBounds(cloudRect, out float cloudLeft, out float cloudRight);
        GetHorizontalBounds(viewportRect, out float viewportLeft, out float viewportRight);

        if (cloudRight < viewportLeft - wrapPadding)
        {
            position.x += viewportRight + wrapPadding - cloudLeft;
            cloudRect.anchoredPosition = position;
        }
    }

    private void GetHorizontalBounds(RectTransform rect, out float left, out float right)
    {
        rect.GetWorldCorners(corners);
        left = float.PositiveInfinity;
        right = float.NegativeInfinity;

        for (int i = 0; i < corners.Length; i++)
        {
            float x = parentRect.InverseTransformPoint(corners[i]).x;
            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x);
        }
    }
}
