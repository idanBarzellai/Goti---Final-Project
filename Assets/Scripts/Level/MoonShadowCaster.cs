using UnityEngine;

public class MoonShadowCaster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelTimerManager levelTimerManager;
    [SerializeField] private SkyTimeVisual skyTimeVisual;

    [Header("Shadow")]
    [SerializeField] private float minShadowDistance = 0.12f;
    [SerializeField] private float maxShadowDistance = 0.38f;
    [SerializeField] private float shadowZ = 0.05f;

    [Header("Shadow Color")]
    [SerializeField] private Color nightShadowColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color sunriseShadowColor = new Color(0f, 0f, 0f, 0.18f);

   public Vector3 GetShadowOffset()
{
    float progress = GetProgress();

    if (skyTimeVisual == null)
        return new Vector3(0.15f, -0.15f, shadowZ);

    Vector2 lightPosition = skyTimeVisual.ActiveLightScreenPosition;

    if (lightPosition.sqrMagnitude < 0.001f)
        lightPosition = new Vector2(0f, 1f);

    Vector2 shadowDirection = -lightPosition.normalized;

    float distance = Mathf.Lerp(maxShadowDistance, minShadowDistance, progress);

    Vector2 offset = shadowDirection * distance;

    return new Vector3(offset.x, offset.y, shadowZ);
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