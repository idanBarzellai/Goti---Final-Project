using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BreathingSprite : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 1f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (spriteRenderer == null)
            return;

        Color color = spriteRenderer.color;

        float t = Mathf.PingPong(Time.time * speed, 1f);

        color.a = Mathf.Lerp(minAlpha, maxAlpha, t);

        spriteRenderer.color = color;
    }
}