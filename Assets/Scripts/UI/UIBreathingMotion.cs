using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIBreathingMotion : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float height = 12f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool randomizeStartOffset;

    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private float phaseOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startAnchoredPosition = rectTransform.anchoredPosition;

        if (randomizeStartOffset)
            phaseOffset = Random.value * Mathf.PI * 2f;
    }

    private void OnEnable()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        startAnchoredPosition = rectTransform.anchoredPosition;
    }

    private void OnDisable()
    {
        if (rectTransform != null)
            rectTransform.anchoredPosition = startAnchoredPosition;
    }

    private void Update()
    {
        if (rectTransform == null)
            return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float offsetY = Mathf.Sin((time * speed * Mathf.PI * 2f) + phaseOffset) * height;
        rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(0f, offsetY);
    }
}
