using UnityEngine;

public class MoonShadowCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelTimerManager levelTimerManager;

    [Header("Shadow Direction")]
    [SerializeField] private Vector2 nightShadowDirection = new Vector2(1f, -1f);
    [SerializeField] private Vector2 sunriseShadowDirection = new Vector2(-1f, -0.4f);

    [Header("Shadow Length")]
    [SerializeField] private float nightShadowDistance = 0.18f;
    [SerializeField] private float sunriseShadowDistance = 0.35f;

    [Header("Shadow Color")]
    [SerializeField] private Color nightShadowColor = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color sunriseShadowColor = new Color(0f, 0f, 0f, 0.18f);

    public Vector3 GetShadowOffset()
    {
        float progress = GetProgress();

        Vector2 direction = Vector2.Lerp(
            nightShadowDirection.normalized,
            sunriseShadowDirection.normalized,
            progress
        );

        float distance = Mathf.Lerp(
            nightShadowDistance,
            sunriseShadowDistance,
            progress
        );

        Vector2 offset = direction.normalized * distance;
        return new Vector3(offset.x, offset.y, 0.05f);
    }

    public Color GetShadowColor()
    {
        return Color.Lerp(
            nightShadowColor,
            sunriseShadowColor,
            GetProgress()
        );
    }

    private float GetProgress()
    {
        if (levelTimerManager == null)
            return 0f;

        return levelTimerManager.Progress01;
    }
}