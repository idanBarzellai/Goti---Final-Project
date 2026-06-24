using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioMuteToggleButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text label;

    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Text")]
    [SerializeField] private string unmutedText = "SOUND ON";
    [SerializeField] private string mutedText = "MUTED";

    [Header("Colors")]
    [SerializeField] private Color unmutedColor = new Color(0.12f, 0.38f, 0.26f, 0.95f);
    [SerializeField] private Color mutedColor = new Color(0.45f, 0.12f, 0.12f, 0.95f);

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();

        if (targetImage == null)
            targetImage = targetGraphic as Image;

        if (button != null)
            button.onClick.AddListener(HandleClicked);
    }

    private void OnEnable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnMuteChanged += Refresh;

        Refresh(AudioManager.Instance != null && AudioManager.Instance.IsMuted);
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnMuteChanged -= Refresh;
    }

    private void HandleClicked()
    {
        if (AudioManager.Instance == null)
            return;

        bool wasMuted = AudioManager.Instance.IsMuted;

        if (!wasMuted)
            AudioManager.Instance.PlayButtonClick();

        AudioManager.Instance.ToggleMuted();

        if (wasMuted && !AudioManager.Instance.IsMuted)
            AudioManager.Instance.PlayButtonClick();
    }

    private void Refresh(bool muted)
    {
        bool hasStateSprites = soundOnSprite != null && soundOffSprite != null;

        if (targetImage != null && hasStateSprites)
        {
            targetImage.sprite = muted ? soundOffSprite : soundOnSprite;
            targetImage.color = Color.white;
            targetImage.preserveAspect = true;
        }

        if (targetGraphic != null)
            targetGraphic.color = hasStateSprites ? Color.white : muted ? mutedColor : unmutedColor;

        if (label != null)
        {
            label.gameObject.SetActive(!hasStateSprites);
            label.text = muted ? mutedText : unmutedText;
        }
    }

    public static AudioMuteToggleButtonUI CreateDefault(Transform parent, string objectName)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement),
            typeof(AudioMuteToggleButtonUI)
        );

        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-28f, -28f);
        buttonRect.sizeDelta = new Vector2(190f, 58f);

        Image image = buttonObject.GetComponent<Image>();
        image.raycastTarget = true;

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 4f);
        labelRect.offsetMax = new Vector2(-10f, -4f);

        TextMeshProUGUI labelText = labelObject.GetComponent<TextMeshProUGUI>();
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 14f;
        labelText.fontSizeMax = 24f;
        labelText.raycastTarget = false;

        AudioMuteToggleButtonUI muteToggle = buttonObject.GetComponent<AudioMuteToggleButtonUI>();
        muteToggle.button = buttonObject.GetComponent<Button>();
        muteToggle.targetGraphic = image;
        muteToggle.targetImage = image;
        muteToggle.label = labelText;
        muteToggle.Refresh(AudioManager.Instance != null && AudioManager.Instance.IsMuted);

        return muteToggle;
    }
}
