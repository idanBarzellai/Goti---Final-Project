using UnityEngine;
using UnityEngine.UI;

public class SkyTimeVisual : MonoBehaviour
{
    [SerializeField] private LevelTimerManager levelTimerManager;

    [Header("Sky Material")]
    [SerializeField] private Image skyImage;
    [SerializeField] private Material skyMaterial;

[Header("Sky Gradients")]
[SerializeField] private Gradient topSkyGradient = new Gradient();
[SerializeField] private Gradient bottomSkyGradient = new Gradient();

    [Header("Celestial Bodies")]
    [SerializeField] private RectTransform moon;
    [SerializeField] private RectTransform sun;
    [SerializeField] private RectTransform moonStartCloud;

    [Header("Arc Settings")]
    [SerializeField] private Vector2 arcCenter = new Vector2(0f, 120f);
    [SerializeField] private float radiusX = 260f;
    [SerializeField] private float radiusY = 340f;

    [Header("Timing")]
    [SerializeField, Range(0f, 1f)] private float moonVisibleUntil = 0.55f;
    [SerializeField, Range(0f, 1f)] private float sunStartsAt = 0.45f;

    public Vector2 ActiveLightScreenPosition { get; private set; }

    private Material runtimeMaterial;

    private static readonly int TopColorId = Shader.PropertyToID("_Top_Color");
    private static readonly int BottomColorId = Shader.PropertyToID("_Bottom_Color");

    private void Awake()
    {
        // SetupDefaultGradients();

        if (skyImage != null)
        {
            runtimeMaterial = Instantiate(skyImage.material);
            skyImage.material = runtimeMaterial;
        }
        else if (skyMaterial != null)
        {
            runtimeMaterial = Instantiate(skyMaterial);
        }
    }

    private void Start()
    {
        PositionCloud();
        UpdateSky(0f);
    }

    private void Update()
    {
        if (levelTimerManager == null)
            return;

        float t = Mathf.Clamp01(levelTimerManager.Progress01);

        UpdateSky(t);
        UpdateMoon(t);
        UpdateSun(t);
        UpdateActiveLight(t);
    }

private void UpdateSky(float t)
{
    if (runtimeMaterial == null)
        return;

    t = Mathf.Clamp01(t);

    float sunriseProgress = 0f;

    if (t >= sunStartsAt)
    {
        sunriseProgress = Mathf.InverseLerp(sunStartsAt, 1f, t);
        sunriseProgress = Mathf.Clamp01(sunriseProgress);

        // Smooth start from black, no instant jump
        sunriseProgress = Mathf.SmoothStep(0f, 1f, sunriseProgress);
    }

    runtimeMaterial.SetColor(
        TopColorId,
        topSkyGradient.Evaluate(sunriseProgress));

    runtimeMaterial.SetColor(
        BottomColorId,
        bottomSkyGradient.Evaluate(sunriseProgress));
}


    private void UpdateMoon(float t)
    {
        if (moon == null)
            return;

        float moonT = Mathf.Clamp01(Mathf.InverseLerp(0f, moonVisibleUntil, t));

        moon.anchoredPosition = EvaluateCircleArc(90f, 0f, moonT);
        moon.gameObject.SetActive(t < moonVisibleUntil);
    }

    private void UpdateSun(float t)
    {
        if (sun == null)
            return;

        float sunT = Mathf.Clamp01(Mathf.InverseLerp(sunStartsAt, 1f, t));

        sun.anchoredPosition = EvaluateCircleArc(180f, 90f, sunT);
        sun.gameObject.SetActive(t >= sunStartsAt);
    }

    private void PositionCloud()
    {
        if (moonStartCloud == null)
            return;

        moonStartCloud.anchoredPosition =
            EvaluateCircleArc(90f, 90f, 0f) + new Vector2(0f, -30f);
    }

    private void UpdateActiveLight(float t)
    {
        if (t < sunStartsAt && moon != null)
        {
            ActiveLightScreenPosition = moon.anchoredPosition;
            return;
        }

        if (sun != null && sun.gameObject.activeSelf)
        {
            ActiveLightScreenPosition = sun.anchoredPosition;
            return;
        }

        if (moon != null)
            ActiveLightScreenPosition = moon.anchoredPosition;
    }

    private Vector2 EvaluateCircleArc(float startAngle, float endAngle, float t)
    {
        float angle = Mathf.Lerp(startAngle, endAngle, Mathf.Clamp01(t)) * Mathf.Deg2Rad;

        return new Vector2(
            arcCenter.x + Mathf.Cos(angle) * radiusX,
            arcCenter.y + Mathf.Sin(angle) * radiusY
        );
    }
}