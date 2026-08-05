using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class InfoHelpPopupUI
{
    public static void CreateEditableSceneUI(
        Transform parent,
        Sprite icon,
        RectTransform beside = null,
        Vector2? fallbackPosition = null)
    {
        if (parent == null || parent.Find("InfoButton") != null)
            return;

        Button launcher = CreateLauncherVisual(parent, ResolveIcon(icon), beside, fallbackPosition);
        if (launcher == null)
            return;

        GameObject overlay = CreateOverlay(parent);
        InfoHelpSceneUI sceneUI = launcher.gameObject.AddComponent<InfoHelpSceneUI>();
        Button close = overlay.transform.Find("Instructions/CLOSE").GetComponent<Button>();
        sceneUI.Configure(launcher, close, overlay.GetComponent<Button>(), overlay);
        overlay.SetActive(false);
    }

    public static Button CreateLauncher(
        Transform parent,
        Sprite icon,
        RectTransform beside = null,
        Vector2? fallbackPosition = null)
    {
        Button button = CreateLauncherVisual(parent, ResolveIcon(icon), beside, fallbackPosition);
        if (button == null)
            return null;
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            Transform overlay = parent.Find("InfoHelpOverlay");
            if (overlay != null && overlay.gameObject.activeSelf)
            {
                overlay.gameObject.SetActive(false);
                return;
            }
            Show(parent);
        });
        return button;
    }

    private static Sprite ResolveIcon(Sprite icon)
    {
        if (icon != null)
            return icon;

        InfoHelpIconProvider provider =
            Resources.Load<InfoHelpIconProvider>("InfoHelpIconProvider");
        return provider != null ? provider.infoIcon : null;
    }

    private static Button CreateLauncherVisual(
        Transform parent,
        Sprite icon,
        RectTransform beside,
        Vector2? fallbackPosition)
    {
        if (parent == null || icon == null)
            return null;

        GameObject launcherObject = new GameObject(
            "InfoButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        launcherObject.transform.SetParent(parent, false);
        launcherObject.transform.SetAsLastSibling();

        RectTransform launcher = launcherObject.GetComponent<RectTransform>();
        launcher.anchorMin = launcher.anchorMax = new Vector2(1f, 1f);
        launcher.pivot = new Vector2(1f, 1f);
        launcher.sizeDelta = new Vector2(76f, 76f);
        launcher.anchoredPosition = beside != null
            ? new Vector2(beside.anchoredPosition.x - beside.rect.width - 18f, beside.anchoredPosition.y)
            : fallbackPosition ?? new Vector2(-28f, -28f);

        Image image = launcherObject.GetComponent<Image>();
        image.sprite = icon;
        image.preserveAspect = true;
        return launcherObject.GetComponent<Button>();
    }

    private static void Show(Transform source)
    {
        Canvas canvas = source.GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.rootCanvas.transform : source;
        Transform existing = parent.Find("InfoHelpOverlay");
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            existing.SetAsLastSibling();
            Transform infoButton = source.Find("InfoButton");
            if (infoButton != null)
                infoButton.SetAsLastSibling();
            return;
        }

        GameObject overlayObject = CreateOverlay(parent);
        Button close = overlayObject.transform.Find("Instructions/CLOSE").GetComponent<Button>();
        void CloseOverlay()
        {
            AudioManager.Instance?.PlayButtonClick();
            overlayObject.SetActive(false);
        }
        close.onClick.AddListener(CloseOverlay);
        overlayObject.GetComponent<Button>().onClick.AddListener(CloseOverlay);
        Transform launcher = source.Find("InfoButton");
        if (launcher != null)
            launcher.SetAsLastSibling();
    }

    private static GameObject CreateOverlay(Transform parent)
    {
        GameObject overlayObject = CreateImage("InfoHelpOverlay", parent, new Color(0f, 0f, 0f, 0.72f));
        overlayObject.AddComponent<Button>().transition = Selectable.Transition.None;
        RectTransform overlay = overlayObject.GetComponent<RectTransform>();
        Stretch(overlay);

        GameObject panelObject = CreateImage("Instructions", overlay, new Color(0.12f, 0.1f, 0.18f, 0.98f));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(760f, 850f);

        VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(42, 42, 30, 30);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        CreateText(panel, "HOW TO PLAY", 42f, 70f, TextAlignmentOptions.Center);
        CreateText(panel, "Drag all pieces from the bank to the board", 29f, 70f);
        CreateText(panel, "Help Goti reach the grave by using all pieces on the board", 29f, 95f);
        CreateText(panel, "Tap on a piece on the board to rotate it", 29f, 70f);
        CreateLegend(panel, Color.white, "Piece can be rotated and moved", Color.black);
        CreateLegend(panel, new Color(0.55f, 0.55f, 0.55f), "Piece can only be rotated, not moved", Color.white);
        CreateLegend(panel, Color.black, "Piece cannot be rotated or moved", Color.white);

        CreateTextButton(panel, "CLOSE");
        return overlayObject;
    }

    private static void CreateLegend(Transform parent, Color cellColor, string text, Color borderColor)
    {
        GameObject rowObject = new GameObject("Legend", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowObject.transform.SetParent(parent, false);
        rowObject.GetComponent<LayoutElement>().preferredHeight = 82f;
        HorizontalLayoutGroup row = rowObject.GetComponent<HorizontalLayoutGroup>();
        row.spacing = 22f;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = false;
        row.childControlHeight = false;

        GameObject border = CreateImage("Cell", rowObject.transform, borderColor);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.sizeDelta = new Vector2(72f, 72f);
        LayoutElement borderLayout = border.AddComponent<LayoutElement>();
        borderLayout.preferredWidth = 72f;
        borderLayout.preferredHeight = 72f;

        GameObject inner = CreateImage("Background", border.transform, cellColor);
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        Stretch(innerRect);
        innerRect.offsetMin = new Vector2(5f, 5f);
        innerRect.offsetMax = new Vector2(-5f, -5f);

        TMP_Text label = CreateText(rowObject.transform, text, 26f, 76f);
        LayoutElement labelLayout = label.GetComponent<LayoutElement>();
        labelLayout.preferredWidth = 570f;
    }

    private static Button CreateTextButton(Transform parent, string label)
    {
        GameObject buttonObject = CreateImage(label, parent, new Color(0.55f, 0.23f, 0.65f, 1f));
        Button button = buttonObject.AddComponent<Button>();
        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 72f;
        CreateText(buttonObject.transform, label, 30f, 72f, TextAlignmentOptions.Center);
        return button;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string value,
        float fontSize,
        float height,
        TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        textObject.GetComponent<LayoutElement>().preferredHeight = height;
        return text;
    }

    private static GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
