using UnityEngine;
using UnityEngine.UI;

public class SkyTimeVisual : MonoBehaviour
{
    [SerializeField] private LevelTimerManager levelTimerManager;
    [SerializeField] private Image skyImage;

    [Header("Sky Colors")]
    [SerializeField] private Color nightColor = new Color(0.02f, 0.03f, 0.1f, 1f);
    [SerializeField] private Color dawnColor = new Color(0.25f, 0.1f, 0.2f, 1f);
    [SerializeField] private Color sunriseColor = new Color(1f, 0.5f, 0.2f, 1f);

    private void Update()
    {
        if (levelTimerManager == null || skyImage == null)
            return;

        float t = levelTimerManager.Progress01;

        skyImage.color = EvaluateSkyColor(t);
    }

    private Color EvaluateSkyColor(float t)
    {
        if (t < 0.4f)
        {
            float localT = t / 0.4f;
            return Color.Lerp(nightColor, dawnColor, localT);
        }
        else
        {
            float localT = (t - 0.4f) / 0.6f;
            return Color.Lerp(dawnColor, sunriseColor, localT);
        }
    }
}