using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform levelButtonSlotsContainer;
    [SerializeField] private RectTransform levelBackground;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private RectTransform[] levelSectionPrefabs;
    [SerializeField] private Sprite[] cloudSprites;

    [Header("Building Blocks")]
    [Tooltip("Vertical distance before the five-section pattern repeats.")]
    [SerializeField, Min(0f)] private float sectionPatternHeight = 1510f;

    [Header("Clouds")]
    [SerializeField, Min(0)] private int cloudsPerSection = 1;
    [Tooltip("Direct minimum and maximum X positions for generated clouds.")]
    [SerializeField] private Vector2 cloudHorizontalRange = new Vector2(-520f, 520f);
    [SerializeField] private Vector2 cloudVerticalRange = new Vector2(-90f, 110f);
    [SerializeField] private Vector2 cloudSize = new Vector2(200f, 65f);

    [Header("Scrolling")]
    [SerializeField] private Vector2 viewportVerticalInsets = Vector2.zero;
    [SerializeField, Min(0f)] private float topContentPadding = 800f;

    private void Start()
    {
        BuildLevelScreen();
    }

    private void BuildLevelScreen()
    {
        if (levelManager == null)
            levelManager = LevelManager.Instance;

        if (levelManager == null || levelManager.LevelCount <= 0)
            return;

        if (levelSectionPrefabs == null || levelSectionPrefabs.Length == 0)
        {
            Debug.LogWarning("LevelSelectUI: No level section prefabs assigned.");
            return;
        }

        // The prefabs now own the platforms, steps, and buttons.
        if (levelBackground != null)
            levelBackground.gameObject.SetActive(false);
        if (levelButtonSlotsContainer != null)
            levelButtonSlotsContainer.gameObject.SetActive(false);

        ScrollRect scrollRect = CreateScrollView(out RectTransform scrollContent);
        if (scrollRect == null || scrollContent == null)
            return;

        int sectionCount = Mathf.CeilToInt(levelManager.LevelCount / 2f);
        for (int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
            CreateSection(scrollContent, sectionIndex);

        ResizeAndPositionContent(scrollContent);
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        StartCoroutine(RefreshScrollPositionNextFrame(scrollRect, scrollContent));
    }

    private void CreateSection(RectTransform scrollContent, int sectionIndex)
    {
        int patternIndex = sectionIndex % levelSectionPrefabs.Length;
        RectTransform sectionPrefab = levelSectionPrefabs[patternIndex];
        if (sectionPrefab == null)
            return;

        RectTransform section = Instantiate(sectionPrefab, scrollContent);
        section.name = $"Level Section {sectionIndex + 1}";
        int patternCycle = sectionIndex / levelSectionPrefabs.Length;
        float horizontalSide = sectionIndex % 2 == 0 ? -1f : 1f;
        section.anchoredPosition = new Vector2(
            Mathf.Abs(sectionPrefab.anchoredPosition.x) * horizontalSide,
            sectionPrefab.anchoredPosition.y + sectionPatternHeight * patternCycle);

        LevelButtonUI[] buttons = section
            .GetComponentsInChildren<LevelButtonUI>(true)
            .OrderBy(button => GetButtonOrder(button.name))
            .ToArray();

        for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
        {
            int levelIndex = sectionIndex * 2 + buttonIndex;
            bool hasLevel = levelIndex < levelManager.LevelCount;
            buttons[buttonIndex].gameObject.SetActive(hasLevel);

            if (hasLevel)
                buttons[buttonIndex].Initialize(
                    levelIndex,
                    levelManager.IsLevelUnlocked(levelIndex));
        }

        AddClouds(section, sectionIndex);
    }

    private static int GetButtonOrder(string buttonName)
    {
        int open = buttonName.LastIndexOf('(');
        int close = buttonName.LastIndexOf(')');
        if (open >= 0 &&
            close > open &&
            int.TryParse(buttonName.Substring(open + 1, close - open - 1), out int order))
            return order;

        return 0;
    }

    private void AddClouds(RectTransform section, int sectionIndex)
    {
        if (cloudSprites == null || cloudSprites.Length == 0 || cloudsPerSection <= 0)
            return;

        for (int i = 0; i < cloudsPerSection; i++)
        {
            Sprite cloudSprite =
                cloudSprites[(sectionIndex * cloudsPerSection + i) % cloudSprites.Length];
            if (cloudSprite == null)
                continue;

            GameObject cloudObject = new GameObject(
                $"Cloud {sectionIndex + 1}-{i + 1}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            cloudObject.layer = gameObject.layer;
            cloudObject.transform.SetParent(section, false);
            RectTransform cloud = cloudObject.GetComponent<RectTransform>();
            cloud.SetAsFirstSibling();
            cloud.anchorMin = cloud.anchorMax = new Vector2(0.5f, 0.5f);
            cloud.pivot = new Vector2(0.5f, 0.5f);

            int cloudOrdinal = sectionIndex * cloudsPerSection + i;
            float evenlyDistributedX =
                Mathf.Repeat(0.11f + cloudOrdinal * 0.6180339f, 1f);
            float xJitter =
                (GetDeterministic01(cloudOrdinal * 43 + 11) - 0.5f) * 0.1f;
            // Older recovery-scene copies serialized this as two positive distances.
            // Convert either format into a wide signed range at runtime.
            float cloudHalfWidth = Mathf.Max(
                520f,
                Mathf.Abs(cloudHorizontalRange.x),
                Mathf.Abs(cloudHorizontalRange.y));
            float horizontalPosition = Mathf.Lerp(
                -cloudHalfWidth,
                cloudHalfWidth,
                Mathf.Repeat(evenlyDistributedX + xJitter, 1f));
            float verticalPosition = Mathf.Lerp(
                cloudVerticalRange.x,
                cloudVerticalRange.y,
                Mathf.Repeat(sectionIndex * 0.53f + i * 0.29f, 1f));
            cloud.anchoredPosition =
                new Vector2(horizontalPosition, verticalPosition);
            cloud.sizeDelta = cloudSize;

            Image cloudImage = cloudObject.GetComponent<Image>();
            cloudImage.sprite = cloudSprite;
            cloudImage.preserveAspect = true;
            cloudImage.raycastTarget = false;

            cloudObject.AddComponent<LoopingCloudUI>();
        }
    }

    private static float GetDeterministic01(int seed)
    {
        unchecked
        {
            uint hash = (uint)seed;
            hash ^= 2747636419u;
            hash *= 2654435769u;
            hash ^= hash >> 16;
            hash *= 2654435769u;
            return (hash & 0x00ffffffu) / 16777215f;
        }
    }

    private ScrollRect CreateScrollView(out RectTransform scrollContent)
    {
        scrollContent = null;
        RectTransform panel = transform as RectTransform;
        if (panel == null)
            return null;

        GameObject viewportObject = new GameObject(
            "LevelScrollViewport",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        viewportObject.layer = gameObject.layer;
        viewportObject.transform.SetParent(panel, false);
        viewportObject.transform.SetAsLastSibling();
        BringNavigationAboveScrollView(panel, viewportObject.transform);

        RectTransform viewport = viewportObject.GetComponent<RectTransform>();
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(0f, viewportVerticalInsets.x);
        viewport.offsetMax = new Vector2(0f, -viewportVerticalInsets.y);

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = Color.clear;
        viewportImage.raycastTarget = true;

        GameObject contentObject = new GameObject("LevelScrollContent", typeof(RectTransform));
        contentObject.layer = gameObject.layer;
        contentObject.transform.SetParent(viewport, false);
        scrollContent = contentObject.GetComponent<RectTransform>();
        scrollContent.anchorMin = new Vector2(0.5f, 0.5f);
        scrollContent.anchorMax = new Vector2(0.5f, 0.5f);
        scrollContent.pivot = new Vector2(0.5f, 0.5f);
        scrollContent.anchoredPosition = Vector2.zero;

        ScrollRect scrollRect = viewportObject.AddComponent<ScrollRect>();
        scrollRect.content = scrollContent;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.12f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.1f;
        scrollRect.scrollSensitivity = 80f;
        return scrollRect;
    }

    private static void BringNavigationAboveScrollView(
        RectTransform panel,
        Transform scrollView)
    {
        for (int i = 0; i < panel.childCount; i++)
        {
            Transform child = panel.GetChild(i);
            if (child == scrollView)
                continue;

            bool isButton = child.GetComponent<Button>() != null;
            bool isTitle =
                child.name.IndexOf("title", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (isButton || isTitle)
                child.SetAsLastSibling();
        }
    }

    private void ResizeAndPositionContent(RectTransform scrollContent)
    {
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < scrollContent.childCount; i++)
        {
            RectTransform section = scrollContent.GetChild(i) as RectTransform;
            if (section == null || !section.gameObject.activeSelf)
                continue;

            Bounds bounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(scrollContent, section);
            minY = Mathf.Min(minY, bounds.min.y);
            maxY = Mathf.Max(maxY, bounds.max.y);
        }

        if (minY == float.MaxValue)
            return;

        // Keep unsaved/recovery scene copies with stale serialized values usable too.
        float effectiveTopPadding = Mathf.Max(topContentPadding, 800f);
        if (effectiveTopPadding > 0f)
        {
            GameObject spacerObject = new GameObject("Top Scroll Buffer", typeof(RectTransform));
            spacerObject.layer = gameObject.layer;
            spacerObject.transform.SetParent(scrollContent, false);

            RectTransform spacer = spacerObject.GetComponent<RectTransform>();
            spacer.anchorMin = spacer.anchorMax = new Vector2(0.5f, 0.5f);
            spacer.pivot = new Vector2(0.5f, 0.5f);
            spacer.anchoredPosition = new Vector2(0f, maxY + effectiveTopPadding * 0.5f);
            spacer.sizeDelta = new Vector2(1f, effectiveTopPadding);
            maxY += effectiveTopPadding;
        }

        float contentHeight = maxY - minY;
        float shiftY = -(minY + maxY) * 0.5f;

        for (int i = 0; i < scrollContent.childCount; i++)
        {
            RectTransform section = scrollContent.GetChild(i) as RectTransform;
            if (section != null)
                section.anchoredPosition += Vector2.up * shiftY;
        }

        scrollContent.sizeDelta = new Vector2(0f, contentHeight);
        scrollContent.anchoredPosition = Vector2.zero;
    }

    private IEnumerator RefreshScrollPositionNextFrame(
        ScrollRect scrollRect,
        RectTransform scrollContent)
    {
        yield return null;

        if (scrollRect == null || scrollContent == null)
            yield break;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
